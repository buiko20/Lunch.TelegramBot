using System;
using System.Collections.Generic;
using System.Threading;
using log4net;
using Timer = System.Timers.Timer;

namespace Lunch.TelegramBot.Common.Utils
{
    public static class Scheduler
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(Scheduler));
        private static readonly List<Timer> Timers = new List<Timer>();

        public static Timer ScheduleDailyAction(string time, Action action)
        {
            string[] timeParts = time.Split(new[] { ':', ' ' }, StringSplitOptions.RemoveEmptyEntries);
            Action capturedAction = action;
            var timer = new Timer(GetWaitingTime(timeParts).TotalMilliseconds) { AutoReset = false };
            timer.Elapsed += (sender, args) =>
            {
                timer.Stop();
                capturedAction();
                Thread.Sleep(3000);
                timer.Interval = GetWaitingTime(timeParts).TotalMilliseconds;
                timer.Start();
            };

            timer.Start();
            Timers.Add(timer);
            return timer;
        }

        public static void Dispose()
        {
            foreach (var timer in Timers)
            {
                try
                {
                    timer.Stop();
                    timer.Dispose();
                }
                catch (Exception e)
                {
                    Logger.Error("Timer dispose error", e);
                }
            }
        }

        private static TimeSpan GetWaitingTime(string[] timeParts)
        {
            var dateNow = DateTime.Now;
            var scheduledDate = new DateTime(dateNow.Year, dateNow.Month, dateNow.Day,
                int.Parse(timeParts[0]), int.Parse(timeParts[1]), int.Parse(timeParts[2]), 0);

            TimeSpan ts;
            if (scheduledDate > dateNow)
            {
                ts = scheduledDate.Subtract(dateNow);
            }
            else
            {
                scheduledDate = scheduledDate.AddDays(1);
                ts = scheduledDate.Subtract(dateNow);
            }

            return ts;
        }
    }
}
