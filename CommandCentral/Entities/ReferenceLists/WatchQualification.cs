using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandCentral.ClientAccess;
using FluentNHibernate.Mapping;

namespace CommandCentral.Entities.ReferenceLists
{
    /// <summary>
    /// A watch qualification, such as JOOD, OOD, etc.
    /// </summary>
    public class WatchQualification : ReferenceListItemBase
    {
        /// <summary>
        /// Loads the watch qualification.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public override List<ReferenceListItemBase> Load(Guid id, MessageToken token)
        {
            using (var session = DataAccess.DataProvider.CreateStatefulSession())
            {
                if (id == default(Guid))
                {
                    return session.QueryOver<WatchQualification>()
                        .Cacheable().CacheMode(NHibernate.CacheMode.Normal)
                        .List<ReferenceListItemBase>().ToList();
                }
                else
                {
                    return new[] { (ReferenceListItemBase)session.Get<WatchQualification>(id) }.ToList();
                }
            }
        }

        /// <summary>
        /// Maps this object to the database.
        /// </summary>
        public class WatchQualificationMapping : ClassMap<WatchQualification>
        {
            /// <summary>
            /// Maps this object to the database.
            /// </summary>
            public WatchQualificationMapping()
            {
                Id(x => x.Id).GeneratedBy.Assigned();

                Map(x => x.Value).Not.Nullable().Unique();
                Map(x => x.Description);

                Cache.ReadWrite();
            }
        }
    }
}
