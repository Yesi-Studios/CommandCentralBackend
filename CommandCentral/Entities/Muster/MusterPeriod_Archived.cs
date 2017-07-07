using AtwoodUtils;
using CommandCentral.Entities.ReferenceLists;
using FluentNHibernate.Mapping;
using NHibernate.Type;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommandCentral.Entities.Muster
{
    public class MusterPeriod_Archived
    {

        public virtual Guid Id { get; set; }

        public virtual string Command { get; set; }

        public virtual TimeRange Range { get; set; }

        public virtual IList<MusterRecord_Archived> MusterArchiveRecords { get; set; }

        public class MusterPeriod_ArchivedMapping : ClassMap<MusterPeriod_Archived>
        {
            public MusterPeriod_ArchivedMapping()
            {
                Id(x => x.Id).GeneratedBy.Assigned();

                Map(x => x.Command).Not.Nullable();

                Component(x => x.Range, x =>
                {
                    x.Map(y => y.Start).Not.Nullable().CustomType<UtcDateTimeType>();
                    x.Map(y => y.End).Not.Nullable().CustomType<UtcDateTimeType>();
                });

                HasMany(x => x.MusterArchiveRecords).Cascade.All();
            }
        }

    }
}
