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
        public virtual DateTime? DateAnswered { get; set; }

        #endregion

        /// <summary>
        /// Maps this object to the database.
        /// </summary>
        public class WatchInputRequirementMapping : ClassMap<WatchInputRequirement>
        {
            /// <summary>
            /// Maps this object to the database.
            /// </summary>
            public WatchInputRequirementMapping()
            {
                Id(x => x.Id).GeneratedBy.Guid();

                References(x => x.Person).Not.Nullable();
                References(x => x.Watchbill).Not.Nullable();
                References(x => x.AnsweredBy);

                Map(x => x.IsAnswered).Default(false.ToString());
                Map(x => x.DateAnswered);
            }
        }

        /// <summary>
        /// Validates the parent object.
        /// </summary>
        public class WatchInputRequirementValidator : AbstractValidator<WatchInputRequirement>
        {
            /// <summary>
            /// Validates the parent object.
            /// </summary>
            public WatchInputRequirementValidator()
            {
                RuleFor(x => x.Person).NotEmpty();
                RuleFor(x => x.Watchbill).NotEmpty();

                When(x => x.AnsweredBy != null || x.DateAnswered.HasValue || x.DateAnswered.Value != default(DateTime) || x.IsAnswered, () =>
                {
                    RuleFor(x => x.DateAnswered).NotEmpty();
                    RuleFor(x => x.DateAnswered).Must(x => x.Value != default(DateTime));
                    RuleFor(x => x.IsAnswered).Must(x => x == true);
                    RuleFor(x => x.AnsweredBy).NotEmpty();
                });
            }
        }
    }
}
