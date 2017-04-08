using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AtwoodUtils
{
    //http://codereview.stackexchange.com/questions/71896/finding-overlapping-time-intervals
    /// <summary>
    /// Describes the comprator for time ranges.
    /// </summary>
    public class TimeRangeComparer : IEqualityComparer<TimeRange>
    {
        /// <summary>
        /// Compares two time ranges.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public bool Equals(TimeRange x, TimeRange y)
        {
            return x.Start.CompareTo(y.Start) >= 0 ? x.Start.CompareTo(y.End) <= 0 : y.Start.CompareTo(x.End) <= 0;
        }

        /// <summary>
        /// The hashcode returns 1, forcing the use of equals.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public int GetHashCode(TimeRange obj)
        {
            // Make all time ranges have the same hash code.
            // In this case the Equals method will be called for each object.
            return 1;
        }
    }
}
