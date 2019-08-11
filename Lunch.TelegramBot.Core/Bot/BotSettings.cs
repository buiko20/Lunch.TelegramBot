using System.Collections.Generic;
using Lunch.TelegramBot.Core.Commands;

namespace Lunch.TelegramBot.Core.Bot
{
    public class BotSettings
    {
        public int InitializationDelay { get; set; }

        public string Key { get; set; }

        public long ChatId { get; set; }

        public IList<CommandSettings> CommandsSettings { get; set; } = new List<CommandSettings>();
    }
}
