using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using log4net;
using Lunch.TelegramBot.Common.Extensions;
using Lunch.TelegramBot.Common.Utils;
using Lunch.TelegramBot.Core.Commands;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Lunch.TelegramBot.Core.Bot
{
    public class LunchBot : TelegramBot
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(LunchBot));
        private readonly List<Command> _commands;

        public LunchBot(BotSettings settings, List<Command> commands) : base(settings)
        {
            _commands = commands?.OrderBy(c => c.Settings.Order).ToList() ?? throw new ArgumentNullException(nameof(commands));
        }

        public override async Task InitializeAsync()
        {
            await base.InitializeAsync().ConfigureAwait(false);
            var commandsToSchedule = _commands.Where(c => c.Settings.Time.HasValue).ToList();
            for (int i = 0; i < commandsToSchedule.Count; i++)
            {
                var command = commandsToSchedule[i];
                ScheduleDailyCommand(command);
                _commands.Remove(command);
                commandsToSchedule.Remove(command);
                i--;
            }

            Logger.Info($"{nameof(LunchBot)} initialized");
        }

        protected override async void OnMessage(object sender, MessageEventArgs e)
        {
            var m = e?.Message;
            if (m is null) return;

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

        protected override void Dispose(bool disposing)
        {
            if (IsDisposed) return;

            ReleaseUnmanagedResources();
            if (disposing)
            {
                foreach (Command command in _commands)
                {
                    SafeDispose(command);
                }
            }

            base.Dispose(disposing);
            IsDisposed = true;
        }

        private async Task OnTextMessageAsync(Message m)
        {
            Logger.Info($"[{m.Chat.Id}] {m.From.FirstName} {m.From.LastName} ({m.From.Username}) отправил сообщение: {Environment.NewLine}\t{m.Text}");
            foreach (Command command in _commands)
            {
                bool isContinue = await command.ExecuteAsync(Bot, m).ConfigureAwait(false);
                if (!isContinue) break;
            }
        }

        private void ScheduleDailyCommand(Command command)
        {
            var message = new Message
            {
                Text = $"/бот, {command.Help}",
                Chat = new Chat { Id = Settings.ChatId }
            };

            string time = command.Settings.Time.Value.To24Time();
            Scheduler.ScheduleDailyAction(time, () =>
            {
                bool isExecutable = command.IsExecutableNow();
                Logger.Info($"Try execute daily command {command.GetType().Name}." +
                            $"{nameof(command.Settings.DaysToExclude)}=[{command.Settings.DaysToExclude?.Aggregate() ?? "null"}]. " +
                            $"Will execute: {isExecutable}");
                if (isExecutable)
                {
                    command.ExecuteAsync(Bot, message).Wait();
                }
            });
            Logger.Info($"{command.GetType().Name} command scheduled daily at {time}, exclude {command.Settings.DaysToExclude?.Aggregate() ?? "null"}");
        }

        private static void SafeDispose(IDisposable obj)
        {
            try
            {
                obj.Dispose();
            }
            catch (Exception e)
            {
                Logger.Error("Dispose error.", e);
            }
        }

        private void ReleaseUnmanagedResources()
        {
            // TODO release unmanaged resources here
        }
    }
}
