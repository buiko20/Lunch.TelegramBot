using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using log4net;
using Lunch.TelegramBot.Common.Configuration;
using Lunch.TelegramBot.Common.Models;
using Lunch.TelegramBot.Common.Utils;
using Telegram.Bot;
using Telegram.Bot.Args;

namespace Lunch.TelegramBot.Core.Api
{
    public abstract class TelegramBot : IDisposable
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(TelegramBot));

        protected int IsInitialized;
        protected readonly TelegramBotClient Bot;
        protected readonly BotSettings Settings;
        protected readonly List<Command> Commands;

        protected TelegramBot(BotSettings settings, List<Command> commands)
        {
            Settings = settings ?? throw new ArgumentNullException(nameof(settings));
            Commands = commands ?? throw new ArgumentNullException(nameof(commands));

            if (string.IsNullOrWhiteSpace(settings.Key))
                throw new ArgumentException("Telegram bot key is empty.", nameof(settings.Key));

            Bot = new TelegramBotClient(settings.Key);
            Bot.OnMessage += OnBotEvent;
            Bot.OnMessageEdited += OnBotEvent;
            Bot.StartReceiving();
        }

        public virtual async Task InitializeAsync()
        {
            await Task.Delay(Settings.InitializationDelay).ConfigureAwait(false);
            Interlocked.Increment(ref IsInitialized);
        }

        public async Task<BotInfo> GetBotInfoAsync()
        {
            var info = await Bot.GetMeAsync().ConfigureAwait(false);
            return new BotInfo
            {
                FirstName = info.FirstName,
                LastName = info.LastName,
                Username = info.Username,
                LanguageCode = info.LanguageCode,
                Id = info.Id
            };
        }

        protected virtual void OnBotEvent(object sender, MessageEventArgs e)
        {
            if (Interlocked.CompareExchange(ref IsInitialized, 1, 1) == 0)
                return;

            try
            {
                OnMessage(sender, e);
            }
            catch (Exception exception)
            {
                Logger.Error("Message processing error", exception);
                throw;
            }
        }

        protected abstract void OnMessage(object sender, MessageEventArgs e);

        #region Dispose Pattern

        ~TelegramBot() => Dispose(false);

        public bool IsDisposed { get; protected set; }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (IsDisposed) return;

            try
            {
                Scheduler.Dispose();
                Bot.OnMessage -= OnBotEvent;
                Bot.OnMessageEdited -= OnBotEvent;
                Bot.StopReceiving();

                foreach (var command in Commands)
                {
                    SafeDispose(command);
                }
            }
            finally
            {
                IsDisposed = true;
            }
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

        #endregion Dispose Pattern
    }
}
