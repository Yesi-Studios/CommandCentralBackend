using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AtwoodUtils;
using CustomDBTypes;

namespace CCServ.Config
{
    public static class Muster
    {
        /// <summary>
        /// The hour at which the muster will roll over, starting a new muster day, regardless of the current muster's status.
        /// </summary>
        public static readonly Time RolloverTime = new Time(20, 00, 0);

        /// <summary>
        /// The hour at which the muster _should_ be completed.  This governs when email are sent and their urgency.
        /// </summary>
        public static readonly Time DueTime = new Time(13, 30, 0);

        /// <summary>
        /// Tracks whether or not the muster has been finalized.  If it has been, no more muster records should be accepted.
        /// <para />
        /// This is used in situations where a client forces the muster to finalize prior to its rollover time.
        /// </summary>
        public static bool IsMusterFinalized { get; set; }
    }
}
