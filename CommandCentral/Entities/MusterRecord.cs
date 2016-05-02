using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Collections.Concurrent;
using System.Reflection;
using MySql.Data.MySqlClient;
using MySql.Data.Common;
using UnifiedServiceFramework.Framework;
using AtwoodUtils;
using FluentNHibernate.Mapping;

namespace CommandCentral.Entities
{
    /// <summary>
    /// Describes a single muster record, intended to archive the fact that a person claimed that another person was in a given state at a given time.
    /// </summary>
    public class MusterRecord
    {
        #region Properties

        /// <summary>
        /// Unique GUID of this muster record
        /// </summary>
        public virtual Guid ID { get; set; }

        /// <summary>
        /// Musterer - I hate that word
        /// </summary>
        public virtual Person Musterer { get; set; }

        /// <summary>
        /// The Person being mustered by the musterer, which is the person mustering the person that must be mustered. muster.
        /// </summary>
        public virtual Person Musteree { get; set; }

        /// <summary>
        /// The Person being mustered's rank. Fucking Mustard.
        /// </summary>
        public virtual string Rank { get; set; }

        /// <summary>
        /// The person that is having the muster happen to them's division.
        /// </summary>
        public virtual string Division { get; set; }

        /// <summary>
        /// The individual that is being made accountable for through the process of mustering's department
        /// </summary>
        public virtual string Department { get; set; }

        /// <summary>
        /// The human being chosen to say their name out loud in front of their peers to make sure they are alive and where they should be at that specific time's Command
        /// </summary>
        public virtual string Command { get; set; }

        /// <summary>
        /// The one tiny human being on this planet out of all the other people that signed a contract that has binded him into a life of accountability's muster state.
        /// </summary>
        public virtual string MusterStatus { get; set; }

        /// <summary>
        /// That same person from above's duty status
        /// </summary>
        public virtual string DutyStatus { get; set; }

        /// <summary>
        /// The date and time the person was mustered at.
        /// </summary>
        public virtual DateTime MusterTime { get; set; }

        #endregion

        /// <summary>
        /// Maps a record to the database.
        /// </summary>
        public class MusterRecordMapping : ClassMap<MusterRecord>
        {
            /// <summary>
            /// Maps a record to the database.
            /// </summary>
            public MusterRecordMapping()
            {
                Table("muster_records");

                Id(x => x.ID).GeneratedBy.Guid();

                References(x => x.Musterer).Not.Nullable();
                References(x => x.Musteree).Not.Nullable();

                Map(x => x.Rank).Not.Nullable().Length(10);
                Map(x => x.Division).Not.Nullable().Length(10);
                Map(x => x.Department).Not.Nullable().Length(10);
                Map(x => x.Command).Not.Nullable().Length(10);
                Map(x => x.MusterStatus).Not.Nullable().Length(20);
                Map(x => x.DutyStatus).Not.Nullable().Length(20);
                Map(x => x.MusterTime).Not.Nullable();
            }
        }

    }
}
