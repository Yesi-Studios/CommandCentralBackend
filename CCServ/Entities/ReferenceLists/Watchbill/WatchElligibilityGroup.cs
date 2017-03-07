using CCServ.ClientAccess;
using FluentNHibernate.Mapping;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CCServ.Entities.ReferenceLists.Watchbill
{
    /// <summary>
    /// A watch bill group is the means by which a watchbill coordinator or other administrator 
    /// indicates the pool of persons from which a watchbill may pull and assign people.
    /// </summary>
    public class WatchElligibilityGroup : ReferenceListItemBase
    {

        #region Properties

        /// <summary>
        /// The list of those persons that are elligible for this watchbill.
        /// </summary>
        public virtual IList<Person> ElligiblePersons { get; set; }

        #endregion

        #region ctors

        /// <summary>
        /// Creates a blank watch elligibiltiy group.
        /// </summary>
        public WatchElligibilityGroup()
        {
            ElligiblePersons = new List<Person>();
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
                    return session.QueryOver<WatchElligibilityGroup>()
                        .Cacheable().CacheMode(NHibernate.CacheMode.Normal)
                        .List<ReferenceListItemBase>().ToList();
                }
                else
                {
                    return new[] { (ReferenceListItemBase)session.Get<WatchElligibilityGroup>(id) }.ToList();
                }
            }
        }

        /// <summary>
        /// Maps this object to the database.
        /// </summary>
        public class WatchElligibilityGroupMapping : ClassMap<WatchElligibilityGroup>
        {
            /// <summary>
            /// Maps this object to the database.
            /// </summary>
            public WatchElligibilityGroupMapping()
            {
                Id(x => x.Id).GeneratedBy.Assigned();

                HasManyToMany(x => x.ElligiblePersons);

                Map(x => x.Value).Not.Nullable().Unique();
                Map(x => x.Description);

                Cache.ReadWrite();
            }
        }

        /// <summary>
        /// Validates the parent object.
        /// </summary>
        public class WatchElligibilityGroupValidator : AbstractValidator<WatchElligibilityGroup>
        {
            /// <summary>
            /// Validates the parent object.
            /// </summary>
            public WatchElligibilityGroupValidator()
            {
                //We only have validation for elligible persons because no other value should change.
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
