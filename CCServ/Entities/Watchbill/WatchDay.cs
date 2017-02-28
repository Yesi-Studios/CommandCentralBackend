using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CCServ.Entities.Watchbill
{
    /// <summary>
    /// A watchday acts as a collection of watchshifts and also knows its rank among the other watchdays in its parent watchbill.
    /// </summary>
    public class WatchDay
    {

        #region Properties

        /// <summary>
        /// The unique Id of this watch day.
        /// </summary>
        public virtual Guid Id { get; set; }

        /// <summary>
        /// The watchbill that owns this watchday.
        /// </summary>
        public virtual Watchbill Watchbill { get; set; }

        /// <summary>
        /// The date of this watch day.
        /// <para />
        /// No two watch days should share the same date, but the parent Watchbill is responsible for this enforcement.
        /// </summary>
        public virtual DateTime Date { get; set; }

        /// <summary>
        /// The collection of watch shifts contained in this watch day.  These represent the actual watches... eg: A shift from 0800-1200.
        /// </summary>
        public virtual IList<WatchShift> WatchShifts { get; set; }

        #endregion

    }
}
