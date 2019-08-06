using System;

namespace Lunch.TelegramBot.Core.Commands
{
    public class CommandSettings
    {
        public string Name { get; set; }

        public int Order { get; set; }

        public TimeSpan? Time { get; set; }

        public DayOfWeek[] DaysToExclude { get; set; }

        public string[] TriggerWords { get; set; }

        public string Token { get; set; }

        public object Data { get; set; }
    }
}
