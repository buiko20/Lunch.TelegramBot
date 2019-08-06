using System;
using System.Threading;
using System.Threading.Tasks;
using log4net;
using Lunch.TelegramBot.Common.Utils;
using Lunch.TelegramBot.Core.Models;
using Telegram.Bot;
using Telegram.Bot.Args;

namespace Lunch.TelegramBot.Core.Bot
{
    public abstract class TelegramBot : IDisposable
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(TelegramBot));

        protected int IsInitialized;
        protected readonly TelegramBotClient Bot;
        protected readonly BotSettings Settings;

        protected TelegramBot(BotSettings settings)
        {
            Settings = settings ?? throw new ArgumentNullException(nameof(settings));

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
                ReleaseUnmanagedResources();
                if (disposing)
                {
                    Scheduler.Dispose();
                    Bot.OnMessage -= OnBotEvent;
                    Bot.OnMessageEdited -= OnBotEvent;
                    Bot.StopReceiving();
                }
            }
            finally
            {
                IsDisposed = true;
            }
        }

        private void ReleaseUnmanagedResources()
        {
            // TODO release unmanaged resources here
        }

        #endregion Dispose Pattern
    }
}
