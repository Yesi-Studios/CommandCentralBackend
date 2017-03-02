using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentNHibernate.Mapping;
using FluentValidation;
using Humanizer;
using AtwoodUtils;

namespace CCServ.Entities.Watchbill
{
    /// <summary>
    /// A watch bill group is the means by which a watchbill coordinator or other administrator 
    /// indicates the pool of persons from which a watchbill may pull and assign people.
    /// </summary>
    public class WatchbillElligibilityGroup
    {

        #region Properties

        /// <summary>
        /// The unique Id of this watchbill group.
        /// </summary>
        public virtual Guid Id { get; set; }

        /// <summary>
        /// The name of this watchbill group.
        /// </summary>
        public virtual string Name { get; set; }

        /// <summary>
        /// The list of those persons that are elligible for this watchbill.
        /// </summary>
        public virtual IList<Person> ElligiblePersons { get; set; }

        #endregion

        /// <summary>
        /// Maps this object to the database.
        /// </summary>
        public class WatchbillElligibilityGroupMapping : ClassMap<WatchbillElligibilityGroup>
        {
            /// <summary>
            /// Maps this object to the database.
            /// </summary>
            public WatchbillElligibilityGroupMapping()
            {
                Id(x => x.Id).GeneratedBy.Guid();

                Map(x => x.Name).Not.Nullable().Unique();
                HasMany(x => x.ElligiblePersons);
            }
        }

        /// <summary>
        /// Validates the parent object.
        /// </summary>
        public class WatchbillElligibilityGroupValidator : AbstractValidator<WatchbillElligibilityGroup>
        {
            /// <summary>
            /// Validates the parent object.
            /// </summary>
            public WatchbillElligibilityGroupValidator()
            {
                RuleFor(x => x.Name).NotEmpty().Length(1, 50)
                    .WithMessage("The name of this group must not be blank and be no more than 50 characters.");

                RuleFor(x => x.ElligiblePersons).Must((group, persons) =>
                    {
                        if (persons.GroupBy(x => x.Id).Any(x => x.Count() != 1))
                            return false;

                        return true;
                    })
                    .WithMessage("You may not list a person more than once in an elligibility group.");
                    
            }
        }

    }
}
