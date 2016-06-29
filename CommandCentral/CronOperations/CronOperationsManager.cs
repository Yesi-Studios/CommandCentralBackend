using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;

namespace CommandCentral.CronOperations
{
    /// <summary>
    /// Provides support for chronologically running operations.
    /// </summary>
    public static class CronOperationsManager
    {

        private static ConcurrentBag<Action> _cronOperationsCache = new ConcurrentBag<Action>();

        /// <summary>
        /// Gets the actions assigned to the cron operations cache.
        /// </summary>
        public static List<Action> CronOperationActions
        {
            get { return _cronOperationsCache.ToList(); }
        }

        private static readonly Timer timer = new Timer(TimeSpan.FromMinutes(15).TotalMilliseconds); //Interval is in milliseconds

        /// <summary>
        /// Gets a TimeSpan object which represents the cron operations's timer's interval.
        /// </summary>
        public static TimeSpan Interval
        {
            get { return TimeSpan.FromMilliseconds(timer.Interval); }
        }

        /// <summary>
        /// Gets a boolean indicating whether or not the Cron Operations timer is ticking.
        /// </summary>
        public static bool IsActive
        {
            get { return timer.Enabled; }
        }

        /// <summary>
        /// Adds a new action to be executed during the cron cycle.
        /// </summary>
        /// <param name="cronOperation"></param>
        public static void RegisterCronOperation(Action cronOperation)
        {
            _cronOperationsCache.Add(cronOperation);
        }

        /// <summary>
        /// Starts the timer that controls the cron operations cycle with the given interval.
        /// </summary>
        /// <param name="interval"></param>
        public static void StartCronOperations(double interval)
        {
            timer.Interval = interval;
            timer.Elapsed += timer_Elapsed;
            timer.Start();
        }
        
        /// <summary>
        /// Starts the timer that controls the cron operations with the default interval of 15 minutes or whatever interval was set previously.
        /// </summary>
        public static void StartCronOperations()
        {
            timer.Elapsed += timer_Elapsed;
            timer.Start();
        }

        /// <summary>
        /// Stops the timer that controls the cron operations cycle and empties the cron operations cache.
        /// <para />
        /// Any operations running when the timer is stopped will continue running.
        /// </summary>
        public static void StopAndRelease()
        {
            timer.Stop();
            _cronOperationsCache = new ConcurrentBag<Action>();
            timer.Elapsed -= timer_Elapsed;
        }

        /// <summary>
        /// Stops the timer that controls the cron operations cycle while leaving the cron operations cache alone.
        /// <para />
        /// Any operations running when the timer is stopped will continue running.
        /// </summary>
        public static void Stop()
        {
            timer.Stop();
            timer.Elapsed -= timer_Elapsed;
        }

        private static void timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Communicator.PostMessageToHost(string.Format("Starting {0} cron operations in parallel with no callback...", _cronOperationsCache.Count), Communicator.MessagePriority.Informational);
            Parallel.ForEach(_cronOperationsCache.ToList(), action => action());
        }

    }
}
