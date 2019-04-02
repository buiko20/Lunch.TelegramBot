using System;
using System.Linq;
using System.Threading.Tasks;
using log4net;
using Lunch.TelegramBot.Common.Configuration;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Lunch.TelegramBot.Core.Api
{
    public abstract class Command : IDisposable, IEquatable<Command>, IComparable<Command>, IComparable
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(Command));
        private bool _isDisposed;

        protected Command(CommandSettings settings)
        {
            Settings = settings ?? throw new ArgumentNullException(nameof(settings));
        }

        public abstract string Help { get; }

        // Will be injected by command Name;
        public virtual CommandSettings Settings { get; }

        /// <summary>
        /// MUST RETURN CONSTANT STRING!!!!
        /// </summary>
        /// <remarks>
        /// "this" can be null in method!
        /// </remarks>
        /// <returns>CONSTANT STRING</returns>
        public abstract string GetName();

        public abstract Task<bool> ExecuteAsync(TelegramBotClient bot, Message message);

        public bool IsExecutableNow(bool isLoggable = false)
        {
            bool isExecutableNow = true;
            bool isExecutableToday = Settings.Enabled && !Settings.DaysToExclude.Contains(DateTime.Now.DayOfWeek);
            if (Settings.Time.Year >= DateTime.Now.Year)
            {
                DateTime startDate = DateTime.Now.AddMinutes(-1);
                DateTime endDate = DateTime.Now.AddMinutes(1);
                DateTime dateToCheck = Settings.Time;
                isExecutableNow = dateToCheck >= startDate && dateToCheck <= endDate;
                if (isLoggable)
                {
                    Logger.Info($"{dateToCheck}>={startDate}: {dateToCheck >= startDate}; " +
                                $"{dateToCheck}<={endDate}: {dateToCheck <= endDate}");
                }
            }

            if (isLoggable)
            {
                Logger.Info($"{nameof(isExecutableNow)}={isExecutableNow}; {nameof(isExecutableToday)}={isExecutableToday}");
            }

            return isExecutableNow && isExecutableToday;
        }

        #region Interfaces Implementation

        public bool Equals(Command other)
        {
            if (other == null) return false;
            return GetName().Equals(other.GetName(), StringComparison.OrdinalIgnoreCase);
        }

        public int CompareTo(Command other)
        {
            if (other == null) return 1;
            return string.Compare(GetName(), other.GetName(), StringComparison.OrdinalIgnoreCase);
        }

        public int CompareTo(object other)
        {
            if (other == null) return 1;
            if (GetType() == other.GetType())
                return CompareTo((Command)other);
            return 1;
        }

        #endregion Interfaces Implementation

        #region Overrides

        public override string ToString() => GetName();

        public override bool Equals(object other)
        {
            if (other == null) return false;
            if (GetType() == other.GetType())
                return Equals((Command)other);
            return false;
        }

        public override int GetHashCode() => GetName().GetHashCode();

        #endregion Overrides

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
                Logger.Error($"Command {GetName()} dispose error", e);
            }
        }

        #endregion Dispose Pattern
    }
}
