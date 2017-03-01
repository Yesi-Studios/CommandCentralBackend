﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CCServ.Entities.Watchbill
{
    /// <summary>
    /// This is how a person is told they must provide input on a watchbill.  A requirement may be answered by as few (even 0) watch inputs as the person wants.
    /// </summary>
    public class WatchInputRequirement
    {

        #region Properties

        /// <summary>
        /// The unique Id of this watch input requirement.
        /// </summary>
        public virtual Guid Id { get; set; }

        /// <summary>
        /// The person who is being told they need to provide inputs.
        /// </summary>
        public virtual Person Person { get; set; }

        /// <summary>
        /// The watchbill for which the person needs to provide inputs.
        /// </summary>
        public virtual Watchbill Watchbill { get; set; }

        /// <summary>
        /// Indicates if this requirement has been answered, regardless of how many inputs there might be.
        /// </summary>
        public virtual bool IsAnswered { get; set; }

        /// <summary>
        /// The person who answered this requirement.  If it's not the Person assigned, then this is the person who did it on the person's behalf.
        /// </summary>
        public virtual Person AnsweredBy { get; set; }

        /// <summary>
        /// The datetime at which this requirement was marked as answered.
        /// </summary>
        public virtual DateTime DateAnswered { get; set; }

        #endregion

    }
}
