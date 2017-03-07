using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentNHibernate.Mapping;
using FluentValidation;

namespace CCServ.Entities.Watchbill
{
    /// <summary>
    /// A watch assignment which ties a person to a watch shift and indicates if the assignment has been completed, or what status it is in.
    /// </summary>
    public class WatchAssignment
    {

        #region Properties

        /// <summary>
        /// The unique Id of this watch assignment.
        /// </summary>
        public virtual Guid Id { get; set; }

        /// <summary>
        /// The watch shift that this assignment assigns the person to.
        /// </summary>
        public virtual WatchShift WatchShift { get; set; }

        /// <summary>
        /// The person that this watch assignment assigns to a watch shift.
        /// </summary>
        public virtual Person PersonAssigned { get; set; }

        /// <summary>
        /// The person who assigned the assigned person to the watch shift.  Basically, the person who created this assignment.
        /// </summary>
        public virtual Person AssignedBy { get; set; }

        /// <summary>
        /// The person who acknowledged this watch assignment.  Either the person assigned or someone who did it on their behalf.
        /// </summary>
        public virtual Person AcknowledgedBy { get; set; }
        
        /// <summary>
        /// The datetime at which this assignment was created.
        /// </summary>
        public virtual DateTime DateAssigned { get; set; }

        /// <summary>
        /// The datetime at which a person acknowledged this watch assignment.
        /// </summary>
        public virtual DateTime? DateAcknowledged { get; set; }

        /// <summary>
        /// Indicates if this watch assignment has been acknowledged.
        /// </summary>
        public virtual bool IsAcknowledged { get; set; }

        /// <summary>
        /// The current state of this watch assignment.
        /// </summary>
        public virtual ReferenceLists.Watchbill.WatchAssignmentState CurrentState { get; set; }

        #endregion

        /// <summary>
        /// Maps this object to the database.
        /// </summary>
        public class WatchAssignmentMapping : ClassMap<WatchAssignment>
        {
            /// <summary>
            /// Maps this object to the database.
            /// </summary>
            public WatchAssignmentMapping()
            {
                Id(x => x.Id).GeneratedBy.Guid();

                References(x => x.WatchShift).Not.Nullable();
                References(x => x.PersonAssigned).Not.Nullable();
                References(x => x.AssignedBy).Not.Nullable();
                References(x => x.CurrentState).Not.Nullable();
                References(x => x.AcknowledgedBy);

                Map(x => x.DateAssigned).Not.Nullable();
                Map(x => x.DateAcknowledged);
            }
        }

        /// <summary>
        /// Validates the parent object.
        /// </summary>
        public class WatchAssignmentValidator : AbstractValidator<WatchAssignment>
        {
            /// <summary>
            /// Validates the parent object.
            /// </summary>
            public WatchAssignmentValidator()
            {
                RuleFor(x => x.WatchShift).NotEmpty();
                RuleFor(x => x.PersonAssigned).NotEmpty();
                RuleFor(x => x.AssignedBy).NotEmpty();
                RuleFor(x => x.CurrentState).NotEmpty();
                RuleFor(x => x.DateAssigned).NotEmpty();

                When(x => x.AcknowledgedBy != null || x.DateAcknowledged.HasValue || x.DateAcknowledged.Value != default(DateTime) || x.IsAcknowledged, () =>
                {
                    RuleFor(x => x.DateAcknowledged).NotEmpty();
                    RuleFor(x => x.DateAcknowledged).Must(x => x.Value != default(DateTime));
                    RuleFor(x => x.IsAcknowledged).Must(x => x == true);
                    RuleFor(x => x.AcknowledgedBy).NotEmpty();
                });
            }
        }

    }
}
