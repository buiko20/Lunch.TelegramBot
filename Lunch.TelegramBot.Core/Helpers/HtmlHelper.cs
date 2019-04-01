using System;

namespace Lunch.TelegramBot.Core.Helpers
{
    internal static class HtmlHelper
    {
        public static string GetFirstTag(string html, string tag)
        {
            int startIndex = html.IndexOf($"<{tag}>", StringComparison.OrdinalIgnoreCase);
            int endIndex = html.IndexOf($"</{tag}>", StringComparison.OrdinalIgnoreCase);
            return html.Substring(startIndex, endIndex - startIndex + $"</{tag}>".Length);
        }

        public static string RemoveSiteNotes(string html)
        {
            while (html.IndexOf("<sup", StringComparison.OrdinalIgnoreCase) != -1)
            {
                int startIndex = html.IndexOf("<sup", StringComparison.OrdinalIgnoreCase);
                int endIndex = html.IndexOf("</sup>", StringComparison.OrdinalIgnoreCase);
                html = html.Remove(startIndex, endIndex - startIndex + "</sup>".Length);
            }

            return html;
        }
    }
}
