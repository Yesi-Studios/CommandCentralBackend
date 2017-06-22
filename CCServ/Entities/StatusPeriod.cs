using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentNHibernate.Mapping;
using FluentValidation;
using AtwoodUtils;
using NHibernate.Type;

namespace CCServ.Entities
{
    /// <summary>
    /// Represents a status period, which is used to indiciate where Sailors are and when.  
    /// Used by the watchbill and muster to determine Sailor availability.
    /// </summary>
    public class StatusPeriod : ICommentable
    {

        #region Properties

        /// <summary>
        /// The unique Id of this status period.
        /// </summary>
        public virtual Guid Id { get; set; }

        /// <summary>
        /// The person for whom this status period was submitted.
        /// </summary>
        public virtual Person Person { get; set; }

        /// <summary>
        /// The person who submitted this status period.
        /// </summary>
        public virtual Person SubmittedBy { get; set; }

        /// <summary>
        /// The date time at which this status period was submitted.
        /// </summary>
        public virtual DateTime DateSubmitted { get; set; }

        /// <summary>
        /// The person who confirmed this status period.
        /// </summary>
        public virtual Person ConfirmedBy { get; set; }

        /// <summary>
        /// The date time at which this status period was confirmed.
        /// </summary>
        public virtual DateTime DateConfirmed { get; set; }

        /// <summary>
        /// The status period type which broadly describes this status period. (Leave, SIQ, etc.)
        /// </summary>
        public virtual ReferenceLists.StatusPeriodType StatusPeriodType { get; set; }

        /// <summary>
        /// A free text field that allows the client to further describe the location at which the status period will take place.
        /// </summary>
        public virtual string Location { get; set; }

        /// <summary>
        /// The start and end datetimes of this status period.
        /// </summary>
        public virtual TimeRange Range { get; set; }

        /// <summary>
        /// Indicates that this status period exempts the Sailor from watch.
        /// </summary>
        public virtual bool ExemptsFromWatch { get; set; }

        /// <summary>
        /// The comments on this status period.
        /// </summary>
        public virtual IList<Comment> Comments { get; set; }

        #endregion

        /// <summary>
        /// Maps this object to the database.
        /// </summary>
        public class StatusPeriodMapping : ClassMap<StatusPeriod>
        {
            /// <summary>
            /// Maps this object to the database.
            /// </summary>
            public StatusPeriodMapping()
            {
                Id(x => x.Id).GeneratedBy.Assigned();

                References(x => x.Person);
                References(x => x.SubmittedBy);
                References(x => x.ConfirmedBy);
                References(x => x.StatusPeriodType);

                Map(x => x.DateConfirmed);
                Map(x => x.DateSubmitted);
                Map(x => x.Location);
                Map(x => x.ExemptsFromWatch);

                HasMany(x => x.Comments)
                    .Cascade.AllDeleteOrphan()
                    .KeyColumn("EntityOwner_id")
                    .ForeignKeyConstraintName("none");

                Component(x => x.Range, x =>
                {
                    x.Map(y => y.Start).Not.Nullable().CustomType<UtcDateTimeType>();
                    x.Map(y => y.End).Not.Nullable().CustomType<UtcDateTimeType>();
                });
            }
        }
    }
}
