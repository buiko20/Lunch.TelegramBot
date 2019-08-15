using System;
using System.Linq;
using System.Threading.Tasks;
using log4net;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Lunch.TelegramBot.Core.Commands
{
    public abstract class Command : IDisposable
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(Command));
        private bool _isDisposed;

        protected Command(CommandSettings settings)
        {
            Settings = settings ?? throw new ArgumentNullException(nameof(settings));
        }

        /// <summary>
        /// Gets instructions for using the command.
        /// </summary>
        public abstract string Help { get; }

        public CommandSettings Settings { get; }

        /// <summary>
        /// Executes bot's command.
        /// </summary>
        /// <param name="bot">telegram bot</param>
        /// <param name="message">message from telegram</param>
        /// <returns>True if the execution thread should continue to execute commands, False otherwise.</returns>
        public Task<bool> ExecuteAsync(ITelegramBotClient bot, Message message)
        {
            return IsExecutableNow() ? ExecuteInternalAsync(bot, message) : Task.FromResult(true);
        }

        public bool IsExecutableNow()
        {
            bool isExecutableToday = !Settings.DaysToExclude?.Contains(DateTime.Now.DayOfWeek) ?? true;
            Logger.Debug($"{nameof(isExecutableToday)}={isExecutableToday}");
            if (!isExecutableToday) return false;

            Logger.Debug($"Settings.Time.HasValue={Settings.Time.HasValue}");
            if (!Settings.Time.HasValue) return true;

            TimeSpan startTime = DateTime.Now.AddSeconds(-3).TimeOfDay;
            TimeSpan endTime = DateTime.Now.AddSeconds(3).TimeOfDay;
            TimeSpan timeToCheck = Settings.Time.Value;
            bool isExecutableNow = timeToCheck >= startTime && timeToCheck <= endTime;

            Logger.Debug($"startTime={startTime}; endTime={endTime}; timeToCheck={timeToCheck}");
            Logger.Debug($"isExecutableNow={isExecutableNow}; timeToCheck >= startTime:{timeToCheck >= startTime}; timeToCheck <= endTime:{timeToCheck <= endTime}");

            return isExecutableNow;
        }

        protected abstract Task<bool> ExecuteInternalAsync(ITelegramBotClient bot, Message message);

        protected bool IsMessageForCommand(Message message)
        {
            string text = message.Text;
            return Settings.TriggerWords.Any(word => text.StartsWith(word, StringComparison.OrdinalIgnoreCase));
        }

        #region Dispose Pattern

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~Command() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            try
            {
                if (!_isDisposed)
                {
                    if (disposing)
                    {
                        // TODO: dispose managed state (managed objects).
                    }

                    // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                    // TODO: set large fields to null.

                    _isDisposed = true;
                }
            }
            catch (Exception e)
            {
                Logger.Error($"Command {GetType().FullName} dispose error", e);
            }
        }

        #endregion Dispose Pattern
    }
}
