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
    public class MusterPeriod
    {
        public virtual Guid Id { get; set; }

        public virtual TimeRange Range { get; set; }

        public virtual Command Command { get; set; }

        public virtual Person FinalizedBy { get; set; }

        public virtual DateTime DateFinalized { get; set; }
 
        public virtual IList<MusterRecord> MusterArchives { get; set; }

        public class MusterPeriodMapping : ClassMap<MusterPeriod>
        {
            public MusterPeriodMapping()
            {
                Id(x => x.Id).GeneratedBy.Assigned();

                Component(x => x.Range, x =>
                {
                    x.Map(y => y.Start).Not.Nullable().CustomType<UtcDateTimeType>();
                    x.Map(y => y.End).Not.Nullable().CustomType<UtcDateTimeType>();
                });

                References(x => x.Command).Not.Nullable();

                HasMany(x => x.MusterArchives).Cascade.All();
            }
        }

        public void Finalize(DateTime dateFinalized, Person finalizedBy = null)
        {
            if (FinalizedBy != null)
                throw new Exception("Attempted to finalize a muster that was already finalized.");

            FinalizedBy = finalizedBy;
            DateFinalized = dateFinalized;


                
        }
    }
}
