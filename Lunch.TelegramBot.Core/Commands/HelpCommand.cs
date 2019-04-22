using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lunch.TelegramBot.Common.Configuration;
using Lunch.TelegramBot.Core.Api;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Lunch.TelegramBot.Core.Commands
{
    public class HelpCommand : Command
    {
        public HelpCommand(CommandSettings settings) : base(settings) { }

        // Will be injected.
        public IEnumerable<Command> Commands { get; set; }

        public override string Help => @"/help — запрос описания команд бота";

        public override string GetName() => "Help";

        public override async Task<bool> ExecuteAsync(TelegramBotClient bot, Message m)
        {
            if (m.Text.IndexOf("/help", StringComparison.OrdinalIgnoreCase) != -1)
            {
                string help = Commands.Aggregate(string.Empty, (current, command) => current + $"{command.Help}{Environment.NewLine}{Environment.NewLine}");
                await bot.SendTextMessageAsync(m.Chat.Id, help).ConfigureAwait(false);
                return false;
            }

            return true;
        }
    }
}
