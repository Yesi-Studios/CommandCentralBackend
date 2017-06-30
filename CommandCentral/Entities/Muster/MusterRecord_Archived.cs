using FluentNHibernate.Mapping;
using NHibernate.Type;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommandCentral.Entities.Muster
{
    /// <summary>
    /// Describes a record in the muster archive.
    /// </summary>
    public class MusterRecord_Archived
    {

        public virtual Guid Id { get; set; }

        public virtual Person SubmittedBy { get; set; }

        public virtual DateTime DateSubmitted { get; set; }

        public virtual Person Person { get; set; }

        public virtual string MusterStatus { get; set; }

        public virtual MusterHistoricalInformation HistoricalInformation { get; set; }

        public class MusterRecord_ArchivedMapping : ClassMap<MusterRecord_Archived>
        {
            public MusterRecord_ArchivedMapping()
            {
                Id(x => x.Id).GeneratedBy.Assigned();

                Map(x => x.DateSubmitted).Not.Nullable().CustomType<UtcDateTimeType>();
                Map(x => x.MusterStatus).Not.Nullable();

                References(x => x.SubmittedBy);
                References(x => x.Person).Not.Nullable();

                Component(x => x.HistoricalInformation, x =>
                {
                    x.Map(y => y.Command).Not.Nullable();
                    x.Map(y => y.Department).Not.Nullable();
                    x.Map(y => y.Designation).Not.Nullable();
                    x.Map(y => y.Division).Not.Nullable();
                    x.Map(y => y.DutyStatus).Not.Nullable();
                    x.Map(y => y.Paygrade).Not.Nullable();
                    x.Map(y => y.UIC).Not.Nullable();
                });
            }
        }
    }
}
