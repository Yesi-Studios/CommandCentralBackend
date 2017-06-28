using CommandCentral.ClientAccess;
using FluentNHibernate.Mapping;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommandCentral.Entities.ReferenceLists.Watchbill
{
    /// <summary>
    /// A watch bill group is the means by which a watchbill coordinator or other administrator 
    /// indicates the pool of persons from which a watchbill may pull and assign people.
    /// </summary>
    public class WatchEligibilityGroup : ReferenceListItemBase
    {

        #region Properties

        /// <summary>
        /// The list of those persons that are eligible for this watchbill.
        /// </summary>
        public virtual IList<Person> EligiblePersons { get; set; }

        /// <summary>
        /// The chain of command that owns this eligibility group and may make changes to it.
        /// </summary>
        public virtual Authorization.ChainsOfCommand OwningChainOfCommand { get; set; }

        #endregion

        #region ctors

        /// <summary>
        /// Creates a blank watch eligibiltiy group.
        /// </summary>
        public WatchEligibilityGroup()
        {
            EligiblePersons = new List<Person>();
        }

        #endregion

        /// <summary>
        /// Loads all object or a single object if given an Id.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="token"></param>
        public override List<ReferenceListItemBase> Load(Guid id, MessageToken token)
        {
            using (var session = DataAccess.NHibernateHelper.CreateStatefulSession())
            {
                if (id == default(Guid))
                {
                    return session.QueryOver<WatchEligibilityGroup>()
                        .Cacheable().CacheMode(NHibernate.CacheMode.Normal)
                        .List<ReferenceListItemBase>().ToList();
                }
                else
                {
                    return new[] { (ReferenceListItemBase)session.Get<WatchEligibilityGroup>(id) }.ToList();
                }
            }
        }

        /// <summary>
        /// Maps this object to the database.
        /// </summary>
        public class WatchEligibilityGroupMapping : ClassMap<WatchEligibilityGroup>
        {
            /// <summary>
            /// Maps this object to the database.
            /// </summary>
            public WatchEligibilityGroupMapping()
            {
                Id(x => x.Id).GeneratedBy.Assigned();

                HasManyToMany(x => x.EligiblePersons);

                Map(x => x.Value).Not.Nullable().Unique();
                Map(x => x.Description);
                Map(x => x.OwningChainOfCommand);

                Cache.ReadWrite();
            }
        }

        /// <summary>
        /// Validates the parent object.
        /// </summary>
        public class WatchEligibilityGroupValidator : AbstractValidator<WatchEligibilityGroup>
        {
            /// <summary>
            /// Validates the parent object.
            /// </summary>
            public WatchEligibilityGroupValidator()
            {
                //We only have validation for eligible persons because no other value should change.
                RuleFor(x => x.EligiblePersons).Must((group, persons) =>
                {
                    if (persons.GroupBy(x => x.Id).Any(x => x.Count() != 1))
                        return false;

                    return true;
                })
                    .WithMessage("You may not list a person more than once in an eligibility group.");
            }
        }

    }
}
