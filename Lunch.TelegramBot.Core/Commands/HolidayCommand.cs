using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using HtmlAgilityPack;
using log4net;
using Lunch.TelegramBot.Common.Extensions;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Lunch.TelegramBot.Core.Commands
{
    public class HolidayCommand : Command
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(HolidayCommand));

        private readonly HttpClient _httpClient = new HttpClient();
        private bool _isDisposed;
        private string _cachedData;
        private DateTime _lastDataGetTime;

        public HolidayCommand(CommandSettings settings) : base(settings)
        {
        }

        protected override async Task<bool> ExecuteInternalAsync(ITelegramBotClient bot, Message m)
        {
            ThrowIfDisposed();
            Logger.Info($"{nameof(HolidayCommand)} execute");
            if (string.IsNullOrWhiteSpace(_cachedData) || _lastDataGetTime.Day != DateTime.Now.Day || Debugger.IsAttached)
            {
                string html = await _httpClient.GetStringAsync(GetUrl()).ConfigureAwait(false);
                html = html.Replace("&#160;", " ");
                string data = $"{GetTodayDate()}{Environment.NewLine}Праздники и памятные дни";
                data += GetData(html, "Международные").TrimEnd().Trim(' ', ',', '-', '—');
                data += GetData(html, "Национальные").TrimEnd().Trim(' ', ',', '-', '—');
                data += GetData(html, "Профессиональные").TrimEnd().Trim(' ', ',', '-', '—');
                data += GetData(html, "Региональные").TrimEnd().Trim(' ', ',', '-', '—');

                if (data.EndsWith("Праздники и памятные дни", StringComparison.OrdinalIgnoreCase))
                {
                    data += $"{Environment.NewLine}{Environment.NewLine}Есть несколько религиозных праздников.";
                }

                _lastDataGetTime = DateTime.Now;
                _cachedData = data;
            }

            await bot.SendTextMessageAsync(m.Chat.Id, _cachedData, ParseMode.Markdown).ConfigureAwait(false);
            return true;
        }

        public override string Help => $"Каждый день в {Settings.Time.Value.To24Time()} осуществляется автооповещение о текущем дне.";

        ~HolidayCommand() => Dispose(false);

        protected override void Dispose(bool disposing)
        {
            if (_isDisposed) return;

            try
            {
                _httpClient?.Dispose();
                base.Dispose(disposing);
            }
            finally
            {
                _isDisposed = true;
            }
        }

        private static string GetUrl() =>
            $"https://ru.wikipedia.org/wiki/{DateTime.Now.Day}_{GetRuMonth()}";

        private static string GetTodayDate() =>
            $"Сегодня {DateTime.Now.Day} {GetRuMonth()} {DateTime.Now.Year}";

        private static string GetRuMonth()
        {
            string month = DateTime.Now.ToString("MMMM", CultureInfo.GetCultureInfo("en-us"));
            switch (month)
            {
                case "February":
                    month = "февраля";
                    break;
                case "March":
                    month = "марта";
                    break;
                case "April":
                    month = "апреля";
                    break;
                case "May":
                    month = "мая";
                    break;
                case "June":
                    month = "июня";
                    break;
                case "July":
                    month = "июля";
                    break;
                case "August":
                    month = "августа";
                    break;
                case "September":
                    month = "сентября";
                    break;
                case "October":
                    month = "октября";
                    break;
                case "November":
                    month = "ноября";
                    break;
                case "December":
                    month = "декабря";
                    break;
                case "January":
                    month = "января";
                    break;
            }

            return month;
        }

        private static HtmlNode GetLiParent(HtmlNode node)
        {
            var parent = node;
            while (!string.Equals(parent.ParentNode.Name, "li", StringComparison.OrdinalIgnoreCase))
            {
                parent = parent.ParentNode;
            }

            return parent.ParentNode;
        }

        private static string GetData(string html, string id)
        {
            if (html.IndexOf($"id=\"{id}\"", StringComparison.OrdinalIgnoreCase) == -1)
                return string.Empty;

            string temp = id.First().ToString().ToUpper() + id.Substring(1);
            string result = $"{Environment.NewLine}{Environment.NewLine}*{temp}*{Environment.NewLine}";
            html = html.Substring(html.IndexOf($"id=\"{id}\"", StringComparison.OrdinalIgnoreCase));
            string ul = RemoveSiteNotes(GetFirstTag(html, "ul"));

            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(ul);
            foreach (var li in htmlDocument.DocumentNode.SelectNodes("//li"))
            {
                string prefix = string.Empty;
                if (li.InnerText.IndexOf("—", StringComparison.OrdinalIgnoreCase) != -1)
                    prefix = li.InnerText.Substring(li.InnerText.IndexOf("—", StringComparison.OrdinalIgnoreCase));

                foreach (var a in li.SelectNodes("//a"))
                {
                    if (string.IsNullOrWhiteSpace(a.InnerText)) continue;

                    temp = a.InnerText;
                    if (prefix.IndexOf(temp, StringComparison.OrdinalIgnoreCase) == -1)
                    {
                        if (GetLiParent(a).Equals(li))
                        {
                            result += $"_{a.InnerText}_, ";
                        }
                    }
                }

                if (result.LastIndexOf(", ", StringComparison.OrdinalIgnoreCase) != -1)
                    result = result.Remove(result.LastIndexOf(", ", StringComparison.OrdinalIgnoreCase), 1);
                if (string.Equals("Региональные", id, StringComparison.OrdinalIgnoreCase))
                    prefix = prefix.Trim(' ', ',', '-', '—');
                result += prefix;
                result += Environment.NewLine + Environment.NewLine;
            }

            return result.Trim(' ', ',', '-', '—');
        }

        private static string GetFirstTag(string html, string tag)
        {
            int startIndex = html.IndexOf($"<{tag}>", StringComparison.OrdinalIgnoreCase);
            int endIndex = html.IndexOf($"</{tag}>", StringComparison.OrdinalIgnoreCase);
            return html.Substring(startIndex, endIndex - startIndex + $"</{tag}>".Length);
        }

        private static string RemoveSiteNotes(string html)
        {
            while (html.IndexOf("<sup", StringComparison.OrdinalIgnoreCase) != -1)
            {
                int startIndex = html.IndexOf("<sup", StringComparison.OrdinalIgnoreCase);
                int endIndex = html.IndexOf("</sup>", StringComparison.OrdinalIgnoreCase);
                html = html.Remove(startIndex, endIndex - startIndex + "</sup>".Length);
            }

            return html;
        }

        private void ThrowIfDisposed()
        {
            if (_isDisposed)
                throw new ObjectDisposedException(nameof(HolidayCommand));
        }
    }
}
