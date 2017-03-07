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
