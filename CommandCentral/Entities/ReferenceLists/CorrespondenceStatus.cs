using FluentNHibernate.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommandCentral.Entities.ReferenceLists
{
    public class CorrespondenceStatus : ReferenceListItemBase
    {
        public class CorrespondenceStatusMapping : ClassMap<CorrespondenceStatus>
        {
            public CorrespondenceStatusMapping()
            {
                Id(x => x.Id).GeneratedBy.Guid();

                Map(x => x.Value).Not.Nullable().Length(50).Unique();
                Map(x => x.Description).Not.Nullable().Length(50);

                Cache.ReadWrite();
            }
        }
    }
}
