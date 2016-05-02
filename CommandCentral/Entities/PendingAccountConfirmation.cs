﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentNHibernate.Mapping;

namespace CommandCentral.Entities
{
    /// <summary>
    /// Describes a single account confirmation.  This is created when a client attempts to register an account.
    /// </summary>
    public class PendingAccountConfirmation
    {

        /// <summary>
        /// The max age after which an account confirmation will have expired and it will become invalid.
        /// </summary>
        public static readonly TimeSpan MaxAge = TimeSpan.FromDays(1);

        #region Properties

        /// <summary>
        /// The unique ID of this account confirmation event.
        /// </summary>
        public virtual Guid ID { get; set; }

        /// <summary>
        /// The person to which it belongs.
        /// </summary>
        public virtual Person Person { get; set; }

        /// <summary>
        /// The time at which this was created.
        /// </summary>
        public virtual DateTime Time { get; set; }

        #endregion

        #region Helper Methods

        /// <summary>
        /// returns a boolean indicating whether or not this account confirmation is still valid or if it has aged off.
        /// </summary>
        /// <returns></returns>
        public virtual bool IsValid()
        {
            return DateTime.Now.Subtract(this.Time) > MaxAge;
        }

        #endregion

        /// <summary>
        /// Maps this class to the database.
        /// </summary>
        public class PendingAccountConfirmationMapping : ClassMap<PendingAccountConfirmation>
        {
            /// <summary>
            /// Maps this class to the database.
            /// </summary>
            public PendingAccountConfirmationMapping()
            {
                Table("pending_account_confirmations");

                Id(x => x.ID).GeneratedBy.Guid();

                References(x => x.Person).Not.Nullable().Unique();

                Map(x => x.Time).Not.Nullable();
            }
        }

    }
}
