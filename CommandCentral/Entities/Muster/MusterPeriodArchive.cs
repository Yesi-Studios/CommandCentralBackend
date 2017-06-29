using AtwoodUtils;
using CommandCentral.Entities.ReferenceLists;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommandCentral.Entities.Muster
{
    public class MusterPeriodArchive
    {

        public Guid Id { get; set; }

        public Command Command { get; set; }

        public TimeRange Range { get; set; }

        public IList<MusterRecord_Archived> MusterArchiveRecords { get; set; }

    }
}
