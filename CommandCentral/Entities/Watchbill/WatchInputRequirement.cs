using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentNHibernate.Mapping;
using FluentValidation;
using NHibernate.Type;

namespace CommandCentral.Entities.Watchbill
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
        /// Indicates if this requirement has been answered, regardless of how many inputs there might be.
        /// </summary>
        public virtual bool IsAnswered { get; set; }

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
                Id(x => x.Id).GeneratedBy.Assigned();

                References(x => x.Person).Not.Nullable();

                Map(x => x.IsAnswered).Default(false.ToString());
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
            }
        }
    }
}
