using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandCentral.ClientAccess;
using FluentNHibernate.Mapping;

namespace CommandCentral.Entities.ReferenceLists
{
    public class Paygrade : ReferenceListItemBase
    {
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
                    return session.QueryOver<Paygrade>()
                        .Cacheable().CacheMode(NHibernate.CacheMode.Normal)
                        .List<ReferenceListItemBase>().ToList();
                }
                else
                {
                    return new[] { (ReferenceListItemBase)session.Get<Paygrade>(id) }.ToList();
                }
            }
        }

        public class PaygradeMapping : ClassMap<Paygrade>
        {
            public PaygradeMapping()
            {
                Id(x => x.Id).GeneratedBy.Assigned();

                Map(x => x.Value).Not.Nullable().Unique();
                Map(x => x.Description);

                Cache.ReadWrite();
            }
        }
    }
}
