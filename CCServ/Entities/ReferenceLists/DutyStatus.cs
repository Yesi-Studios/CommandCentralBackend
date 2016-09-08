using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CCServ.ClientAccess;
using FluentNHibernate.Mapping;
using FluentValidation.Results;

namespace CCServ.Entities.ReferenceLists
{
    public class DutyStatus : ReferenceListItemBase
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
                    return session.QueryOver<DutyStatus>().List<ReferenceListItemBase>().ToList();
                }
                else
                {
                    return new[] { (ReferenceListItemBase)session.Get<DutyStatus>(id) }.ToList();
                }
            }
        }

        public class DutyStatusMapping : ClassMap<DutyStatus>
        {
            public DutyStatusMapping()
            {
                Id(x => x.Id).GeneratedBy.Assigned();

                Map(x => x.Value).Not.Nullable().Unique();
                Map(x => x.Description);
            }
        }

    }
}
