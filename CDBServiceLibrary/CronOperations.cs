using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace UnifiedServiceFramework
{
    /// <summary>
    /// Provides support for chronologically running operations.
    /// </summary>
    public static class CronOperations
    {

        private static ConcurrentBag<Action> _cronOperationsCache = new ConcurrentBag<Action>();

        /// <summary>
        /// Gets the actions assigned ot the cron operations cache.
        /// </summary>
        public static List<Action> CronOperationActions
        {
            get
            {
                return _cronOperationsCache.ToList();

            }
        }

        private static Timer _timer = new Timer(TimeSpan.FromMinutes(15).TotalMilliseconds); //Interval is in milliseconds

        /// <summary>
        /// Gets a TimeSpan object which represents the cron operations's timer's interval.
        /// </summary>
        public static TimeSpan Interval
        {
            get
            {
                return TimeSpan.FromMilliseconds(_timer.Interval);
            }
        }

        /// <summary>
        /// Gets a boolean indicating whether or not the Cron Operations timer is ticking.
        /// </summary>
        public static bool IsActive
        {
            get
            {
                return _timer.Enabled;
            }
        }

        /// <summary>
        /// Adds a new action to be executed during the cron cycle.
        /// </summary>
        /// <param name="cronOperation"></param>
        public static void RegisterCronOperation(Action cronOperation)
        {
            try
            {
                _cronOperationsCache.Add(cronOperation);
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Starts the timer that controls the cron operations cycle with the given interval.
        /// </summary>
        /// <param name="interval"></param>
        public static void StartCronOperations(double interval)
        {
            try
            {
                _timer.Interval = interval;
                _timer.Elapsed += timer_Elapsed;
                _timer.Start();
            }
            catch
            {
                throw;
            }
        }
        
        /// <summary>
        /// Starts the timer that controls the cron operations with the default interval of 15 minutes or whatever interval was set previously.
        /// </summary>
        public static void StartCronOperations()
        {
            try
            {
                _timer.Elapsed += timer_Elapsed;
                _timer.Start();
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Stops the timer that controls the cron operations cycle and empties the cron operations cache.
        /// <para />
        /// Any operations running when the timer is stopped will continue running.
        /// </summary>
        public static void StopAndRelease()
        {
            try
            {
                _timer.Stop();
                _cronOperationsCache = new ConcurrentBag<Action>();
                _timer.Elapsed -= timer_Elapsed;
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Stops the timer that controls the cron operations cycle while leaving the cron operations cache alone.
        /// <para />
        /// Any operations running when the timer is stopped will continue running.
        /// </summary>
        public static void Stop()
        {
            try
            {
                _timer.Stop();
                _timer.Elapsed -= timer_Elapsed;
            }
            catch
            {
                throw;
            }
        }

        private static void timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                Communicator.PostMessageToHost(string.Format("Starting {0} cron operations in parallel with no callback...", _cronOperationsCache.Count), Communicator.MessagePriority.Informational);
                Parallel.ForEach(_cronOperationsCache.ToList(), (action) => action());
            }
            catch
            {
                throw;
            }
        }

    }
}
