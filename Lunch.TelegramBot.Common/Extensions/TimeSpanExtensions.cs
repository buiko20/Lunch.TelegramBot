using System;
using System.Collections.Generic;

namespace Lunch.TelegramBot.Common.Extensions
{
    public static class TimeSpanExtensions
    {
        private static readonly List<string> TimeFormats;

        static TimeSpanExtensions()
        {
            TimeFormats = GetTimeFormats();
        }

        public static string To24Time(this TimeSpan time)
        {
            foreach (var timeFormat in TimeFormats)
            {
                if (TryToString(time, timeFormat, out string result))
                {
                    return result;
                }
            }

            throw new FormatException();
        }

        private static List<string> GetTimeFormats()
        {
            var result = new List<string>();
            for (var f = "fffffff"; !string.IsNullOrWhiteSpace(f); f = f.Remove(startIndex: 0, count: 1))
            {
                result.Add($"hh\\:mm\\:ss\\.{f}");
            }

            result.Add("hh\\:mm\\:ss");
            result.Reverse();
            return result;
        }

        private static bool TryToString(TimeSpan time, string format, out string result)
        {
            result = null;
            try
            {
                result = time.ToString(format);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}