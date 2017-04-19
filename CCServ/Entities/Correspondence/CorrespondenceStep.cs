using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentNHibernate.Mapping;
using FluentValidation;
using CCServ.Entities.ReferenceLists;
using CCServ.ClientAccess;
using System.Globalization;
using AtwoodUtils;
using NHibernate.Type;

namespace CCServ.Entities.Correspondence
{
    /// <summary>
    /// Describes a single correspondence step which is used in the parents object as a list.
    /// </summary>
    public class CorrespondenceStep
    {

        #region Properties

        /// <summary>
        /// The unique Id for this step.
        /// </summary>
        public virtual Guid Id { get; set; }

        /// <summary>
        /// The correspondence that owns this step.
        /// </summary>
        [ConditionalJsonIgnore]
        public virtual Correspondence Correspondence { get; set; }

        /// <summary>
        /// The number order of this step.  If this step shares its order with another step, then they occur at the same time.
        /// </summary>
        public virtual int Order { get; set; }

        /// <summary>
        /// Indicates whether or not this step is a review step or not.  
        /// A review step is considered optional, and not necessary to be approved for overall approval of the parent correspondence.
        /// </summary>
        public virtual bool IsReview { get; set; }

        /// <summary>
        /// The time limit, in number of days, that this step must be completed in.  
        /// If a step's time limit expires without action, the step is considered denied.
        /// A step marked as NOT a review step can not expire.
        /// </summary>
        public virtual int TimeLimit { get; set; }

        /// <summary>
        /// The list of persons who are authorized to approve this step.
        /// </summary>
        public virtual IList<Person> Persons { get; set; }

        /// <summary>
        /// The person who either denied or approved this step.  This should be a person from the Persons collection.
        /// </summary>
        public virtual Person ActionPerson { get; set; }

        /// <summary>
        /// Indicates that this step has been approved.  A null value means this step hasn't been visted yet.
        /// </summary>
        public virtual bool? IsApproved { get; set; }

        /// <summary>
        /// Indicates the date/time this step was approved or denied.
        /// </summary>
        public virtual DateTime? DateOfAction { get; set; }

        /// <summary>
        /// Indicates the date/time that this step started, or in other words, when the previous step was denied or approved.
        /// </summary>
        public virtual DateTime? DateStarted { get; set; }

        /// <summary>
        /// The title of this step.  This should be a short description of this step such as "Div Chief" or "CMC".
        /// </summary>
        public virtual string Title { get; set; }

        #endregion

        /// <summary>
        /// Maps this object to the database.
        /// </summary>
        public class CorrespondenceStepMapping : ClassMap<CorrespondenceStep>
        {
            /// <summary>
            /// Maps this object to the database.
            /// </summary>
            public CorrespondenceStepMapping()
            {
                Id(x => x.Id).GeneratedBy.Assigned();

                References(x => x.Correspondence).Not.Nullable();
                References(x => x.ActionPerson);

                HasMany(x => x.Persons);

                Map(x => x.Title).Not.Nullable();
                Map(x => x.Order).Not.Nullable();
                Map(x => x.IsReview).Not.Nullable().Default(false.ToString());
                Map(x => x.TimeLimit).Not.Nullable();
                Map(x => x.IsApproved);
                Map(x => x.DateOfAction).CustomType<UtcDateTimeType>();
                Map(x => x.DateStarted).CustomType<UtcDateTimeType>();
            }
        }

        public class CorrespondenceStepValidator : AbstractValidator<CorrespondenceStep>
        {
            public CorrespondenceStepValidator()
            {
                RuleFor(x => x.Correspondence).NotEmpty();
                RuleFor(x => x.Order).GreaterThanOrEqualTo(1);
                RuleFor(x => x.TimeLimit).GreaterThanOrEqualTo(1);
                RuleForEach(x => x.Persons).Must(person =>
                    {
                        using (var session = DataAccess.NHibernateHelper.CreateStatefulSession())
                        {
                            var personFromDB = session.Get<Person>(person.Id);

                            if (personFromDB == null || personFromDB.DutyStatus == DutyStatuses.Loss)
                                return false;
                        }

                        return true;
                    }).WithMessage("All persons must be real persons and their duty stauses may not be LOSS.");
                RuleFor(x => x.ActionPerson).NotEmpty();
            }
        }

    }
}
