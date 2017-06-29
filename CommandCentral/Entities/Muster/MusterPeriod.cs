using AtwoodUtils;
using CommandCentral.Entities.ReferenceLists;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommandCentral.Entities.Muster
{
    public class MusterPeriod
    {

        public Guid Id { get; set; }

        public TimeRange Range { get; set; }

        public Command Command { get; set; }

        public IList<MusterRecord> MusterArchives { get; set; }

    }
}
