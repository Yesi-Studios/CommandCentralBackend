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
    public class MusterRecord
    {

        public virtual Guid Id { get; set; }

        public virtual Person SubmittedBy { get; set; }

        public virtual DateTime DateSubmitted { get; set; }

        public virtual Person Person { get; set; }

        public virtual MusterStatus MusterStatus { get; set; }

        public class MusterRecordMapping : ClassMap<MusterRecord>
        {
            public MusterRecordMapping()
            {
                Id(x => x.Id).GeneratedBy.Assigned();

                Map(x => x.DateSubmitted).Not.Nullable().CustomType<UtcDateTimeType>();

                References(x => x.SubmittedBy);
                References(x => x.Person).Not.Nullable();
                References(x => x.MusterStatus);
            }
        }

    }
}
