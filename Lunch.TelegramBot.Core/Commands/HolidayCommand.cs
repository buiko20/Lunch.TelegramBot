using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using HtmlAgilityPack;
using log4net;
using Lunch.TelegramBot.Common.Configuration;
using Lunch.TelegramBot.Common.Extensions;
using Lunch.TelegramBot.Core.Api;
using Lunch.TelegramBot.Core.Helpers;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Lunch.TelegramBot.Core.Commands
{
    internal class HolidayCommand : Command
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(HolidayCommand));
        private static readonly string[] WhatDayPhrases =
        {
            // ВНИМАНИЕ КОСТЫЛИ!
            "праздник", // Должно быть первым, в команде дня стоит skip(1), чтобы обращение требовало только "праздник".
            "повод нажраться",
            "повод выпить",
            "повод бухнуть",
        };

        private readonly HttpClient _httpClient = new HttpClient();
        private bool _isDisposed;
        private string _cachedData;
        private DateTime _lastDataGetTime;

        public HolidayCommand(CommandSettings settings) : base(settings)
        {
        }

        public override string GetName() => "Holiday";

        public override async Task<bool> ExecuteAsync(TelegramBotClient bot, Message m)
        {
            ThrowIfDisposed();
            if (!IsExecutableNow()) return true;
            if ((m.Text.IndexOf("/бот", StringComparison.OrdinalIgnoreCase) != -1 &&
                 WhatDayPhrases.Any(s => m.Text.IndexOf(s, StringComparison.OrdinalIgnoreCase) != -1)) ||
                (WhatDayPhrases.Skip(1).Any(s => m.Text.IndexOf(s, StringComparison.OrdinalIgnoreCase) != -1)))
            {
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
                return false;
            }

            return true;
        }

        public override string Help
        {
            get
            {
                string allPhases = WhatDayPhrases.Aggregate();
                string help = $"{allPhases} — при обращении к боту с данной фразой будет показан список сегодняшних поводов выпить";
                return $"{help}{Environment.NewLine}Так же каждый день в {Settings.Time:HH:mm:ss} осуществляется автооповещение о текущем дне.";
            }
        }

        ~HolidayCommand() => Dispose(false);

        protected override void Dispose(bool disposing)
        {
            if (_isDisposed) return;

            try
            {
                _httpClient?.Dispose();
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
            string ul = HtmlHelper.RemoveSiteNotes(HtmlHelper.GetFirstTag(html, "ul"));

            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(ul);
            foreach (var li in htmlDocument.DocumentNode.SelectNodes("//li"))
            {
                string prefix = string.Empty;
                if (li.InnerText.IndexOf("—", StringComparison.OrdinalIgnoreCase) != -1)
                   prefix  = li.InnerText.Substring(li.InnerText.IndexOf("—", StringComparison.OrdinalIgnoreCase));

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

        private void ThrowIfDisposed()
        {
            if (_isDisposed)
                throw new ObjectDisposedException(nameof(HolidayCommand));
        }
    }
}
