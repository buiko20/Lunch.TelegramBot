using System;

namespace Lunch.TelegramBot.Common.Extensions
{
    public static class TimeSpanExtensions
    {
        public static string To24Time(this TimeSpan time) =>
            time.ToString("hh\\:mm\\:ss");
    }
}
