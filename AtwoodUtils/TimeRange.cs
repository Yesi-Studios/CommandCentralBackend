using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AtwoodUtils
{
    /// <summary>
    /// Defines a time range, with a start and end time.
    /// </summary>
    public class TimeRange
    {

        #region Properties

        /// <summary>
        /// The start period.
        /// </summary>
        public DateTime Start { get; set; }

        /// <summary>
        /// The end period.
        /// </summary>
        public DateTime End { get; set; }

        #endregion

        #region Overrides

        /// <summary>
        /// Compares two ranges' start and end dates.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
                return false;

            var other = (TimeRange)obj;

            return End == other.End && Start == other.Start;
        }

        /// <summary>
        /// Compares the values of two time ranges.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static bool operator ==(TimeRange x, TimeRange y)
        {
            if (ReferenceEquals(null, x))
                return ReferenceEquals(null, y);

            return x.Equals(y);
        }

        /// <summary>
        /// Compares the values of two time ranges.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static bool operator !=(TimeRange x, TimeRange y)
        {
            return !(x == y);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = 13;
                hashCode = (hashCode * 397) ^ Start.GetHashCode();
                hashCode = (hashCode * 397) ^ End.GetHashCode();
                return hashCode;
            }
        }

        /// <summary>
        /// Prints the start and end times for this range.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return Start.ToString() + "..." + End.ToString();
        }

        #endregion

    }
}
