using System;
using System.Collections.Generic;

namespace Lunch.TelegramBot.Common.Configuration
{
    public class CommandSettings : IEquatable<CommandSettings>, IComparable<CommandSettings>, IComparable
    {
        public CommandSettings()
        {
        }

        public CommandSettings(string name, bool enabled, TimeSpan time, int order, IEnumerable<DayOfWeek> daysToExclude)
        {
            Name = name;
            Enabled = enabled;
            Time = time;
            Order = order;
            DaysToExclude = daysToExclude;
        }

        public string Name { get; set; }

        public bool Enabled { get; set; }

        public TimeSpan Time { get; set; }

        public IEnumerable<DayOfWeek> DaysToExclude { get; set; }

        public int Order { get; set; }

        public override string ToString() => Name;

        public override bool Equals(object other)
        {
            if (other == null)
                return false;

            if (this == other)
                return true;

            if (GetType() == other.GetType())
                return Equals((CommandSettings) other);
            return false;
        }

        public override int GetHashCode() => Name.GetHashCode();

        public bool Equals(CommandSettings other)
        {
            if (other == null)
                return false;

            return string.Equals(Name, other.Name, StringComparison.OrdinalIgnoreCase);
        }

        public int CompareTo(CommandSettings other)
        {
            if (other == null)
                return 1;

            return string.Compare(Name, other.Name, StringComparison.OrdinalIgnoreCase);
        }

        public int CompareTo(object other)
        {
            if (other == null)
                return 1;

            if (GetType() == other.GetType())
                return CompareTo((CommandSettings)other);
            return 1;
        }
    }

    public class BotSettings
    {
        public string Key { get; set; }

        public int InitializationDelay { get; set; }

        public string ClientAccessToken { get; set; } // ApiAiSDK

        public List<CommandSettings> CommandSettings { get; } = new List<CommandSettings>();
    }
}
