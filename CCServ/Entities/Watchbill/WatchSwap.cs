﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CCServ.Entities.Watchbill
{
    /// <summary>
    /// Represents a request by a person to swap an assigned watch with someone else.
    /// </summary>
    public class WatchSwap
    {

        #region Properties

        /// <summary>
        /// The unique Id of this watch swap.
        /// </summary>
        public virtual Guid Id { get; set; }

        /// <summary>
        /// The watch assignment that this watch swap seeks to change.
        /// </summary>
        public virtual WatchAssignment WatchAssignment { get; set; }

        /// <summary>
        /// The datetime at which this watch swap was created.
        /// </summary>
        public virtual DateTime DateCreated { get; set; }

        /// <summary>
        /// The person who created this watch swap request.
        /// </summary>
        public virtual Person CreatedBy { get; set; }

        /// <summary>
        /// This is the person who will be assigned to the watch in the event that this watch swap is approved.
        /// </summary>
        public virtual Person PersonToAssign { get; set; }

        /// <summary>
        /// The person who approved this watch swap.
        /// </summary>
        public virtual Person ApprovedBy { get; set; }

        /// <summary>
        /// The comments on this watch swap.
        /// </summary>
        public virtual IList<Comment> Comments { get; set; }

        /// <summary>
        /// The datetime at which this watch swap was approved.
        /// </summary>
        public virtual DateTime DateApproved { get; set; }

        /// <summary>
        /// Indicates if this watch swap has been approved.
        /// </summary>
        public virtual bool IsApproved { get; set; }

        #endregion

    }
}