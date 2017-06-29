using CommandCentral.Entities.ReferenceLists;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommandCentral.Entities.Muster
{
    public class MusterRecord
    {

        public Guid Id { get; set; }

        public Person SubmittedBy { get; set; }

        public DateTime DateSubmitted { get; set; }

        public Person Person { get; set; }

        public MusterStatus MusterStatus { get; set; }

    }
}
