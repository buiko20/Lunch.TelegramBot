using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Lunch.TelegramBot.Core.Commands
{
    public class BotHelpCommand : Command
    {
        private readonly List<Command> _commands;

        public BotHelpCommand(CommandSettings settings, List<Command> commands) : base(settings)
        {
            _commands = commands ?? throw new ArgumentNullException(nameof(commands));
        }

        public override string Help => @"/help - описание команд бота";

        protected override async Task<bool> ExecuteInternalAsync(ITelegramBotClient bot, Message m)
        {
            if (IsMessageForCommand(m))
            {
                string help = _commands.Aggregate(string.Empty, (result, command) => result + $"{command.Help}{Environment.NewLine}{Environment.NewLine}");
                await bot.SendTextMessageAsync(m.Chat.Id, help).ConfigureAwait(false);
                return false;
            }

            return true;
        }
    }
}
