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

        public Guid Id { get; set; }

        public Person SubmittedBy { get; set; }

        public DateTime DateSubmitted { get; set; }

        public Person Person { get; set; }

        public string MusterStatus { get; set; }

        public MusterHistoricalInformation HistoricalInformation { get; set; }

    }
}
