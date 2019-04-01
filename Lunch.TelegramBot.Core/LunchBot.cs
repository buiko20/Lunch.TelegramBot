using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using log4net;
using Lunch.TelegramBot.Common.Configuration;
using Lunch.TelegramBot.Common.Extensions;
using Lunch.TelegramBot.Common.Utils;
using Lunch.TelegramBot.Core.Api;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Lunch.TelegramBot.Core
{
    public class LunchBot : Api.TelegramBot
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(LunchBot));

        public LunchBot(BotSettings settings, List<Command> commands)
            : base(settings, commands)
        {
        }

        public override async Task InitializeAsync()
        {
           await base.InitializeAsync().ConfigureAwait(false);
           var commandsToSchedule = Commands.Where(c => c.Settings.Time.Year >= DateTime.Now.Year).ToArray();
           foreach (var command in commandsToSchedule)
           {
               if (command.Settings.Enabled)
                   ScheduleDailyCommand(command);
           }
           Logger.Info($"{nameof(LunchBot)} initialized");
        }

        protected override async void OnMessage(object sender, MessageEventArgs e)
        {
            var m = e?.Message;
            if (object.ReferenceEquals(m, null))
                return;
            switch (m.Type)
            {
                case MessageType.Text:
                    await OnTextMessageAsync(m).ConfigureAwait(false);
                    break;
                case MessageType.Sticker:
                    Logger.Info($"{m.From.FirstName} {m.From.LastName} ({m.From.Username}) отправил стикер: {Environment.NewLine}\t{m.Sticker.SetName} {m.Sticker.FileId}");
                    break;
                default:
                    Logger.Info($"Unsupported message arrived. {m.Type} : {m.Text}");
                    break;
            }
        }

        private async Task OnTextMessageAsync(Message m)
        {
            Logger.Info($"[{m.Chat.Id}] {m.From.FirstName} {m.From.LastName} ({m.From.Username}) отправил сообщение: {Environment.NewLine}\t{m.Text}");
            foreach (var command in Commands)
            {
                if (command.IsExecutableNow())
                {
                    bool isContinue = await command.ExecuteAsync(Bot, m).ConfigureAwait(false);
                    if (!isContinue)
                        break;
                }
            }
        }

        private void ScheduleDailyCommand(Command command)
        {
            const int lunchChatId = -357076564, me = 569665095;
            int chatId;
#if DEBUG
            chatId = me;
#else
            chatId = lunchChatId;
#endif
            var message = new Message
            {
                Text = $"/бот, {command.Help}",
                Chat = new Chat { Id = chatId }
            };

            string time = command.Settings.Time.ToString("HH:mm:ss");
            Scheduler.ScheduleDailyAction(time, () =>
            {
                bool isExecutable = command.IsExecutableNow();
                Logger.Info($"Execute daily command {command.GetName()}. {nameof(command.Settings.Enabled)}={command.Settings.Enabled} " +
                            $"{nameof(command.Settings.DaysToExclude)}={command.Settings.DaysToExclude.Aggregate()}." +
                            $"Will execute: {isExecutable}");
                if (isExecutable)
                {
                    command.ExecuteAsync(Bot, message);
                }
            });
            Logger.Info($"{command.GetName()} command scheduled daily at {time}, exclude {command.Settings.DaysToExclude.Aggregate()}");
        }
    }
}
