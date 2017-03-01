using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CCServ.ClientAccess;
using FluentNHibernate.Mapping;

namespace CCServ.Entities.ReferenceLists.Watchbill
{
    /// <summary>
    /// Defines a watch shift type, such as super, ood, jood, etc.
    /// </summary>
    public class WatchShiftType : ReferenceListItemBase
    {

        #region Properties

        /// <summary>
        /// The collection of watch qualifications a person must have in order to be assigned a watch with this watch type.
        /// </summary>
        public virtual IList<WatchQualification> RequiredWatchQualifications { get; set; }

        #endregion

        /// <summary>
        /// Loads all objects or a single object if given an Id.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public override List<ReferenceListItemBase> Load(Guid id, MessageToken token)
        {
            using (var session = DataAccess.NHibernateHelper.CreateStatefulSession())
            {
                if (id == default(Guid))
                {
                    return session.QueryOver<WatchShiftType>()
                        .Cacheable().CacheMode(NHibernate.CacheMode.Normal)
                        .List<ReferenceListItemBase>().ToList();
                }
                else
                {
                    return new[] { (ReferenceListItemBase)session.Get<WatchShiftType>(id) }.ToList();
                }
            }
        }

        /// <summary>
        /// Maps this object to the database.
        /// </summary>
        public class WatchShiftTypeMapping : ClassMap<WatchShiftType>
        {
            /// <summary>
            /// Maps this object to the database.
            /// </summary>
            public WatchShiftTypeMapping()
            {
                Id(x => x.Id).GeneratedBy.Assigned();

                Map(x => x.Value).Not.Nullable().Unique();
                Map(x => x.Description);

                HasMany(x => x.RequiredWatchQualifications);

                Cache.ReadWrite();
            }
        }

    }
}
