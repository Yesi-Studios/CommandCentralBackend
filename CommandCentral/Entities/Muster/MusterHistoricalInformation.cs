using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommandCentral.Entities.Muster
{
    public class MusterHistoricalInformation
    {

        public string Paygrade { get; set; }
        public string Designation { get; set; }
        public string Division { get; set; }
        public string Department { get; set; }
        public string Command { get; set; }
        public string UIC { get; set; }
        public string DutyStatus { get; set; }

        public MusterHistoricalInformation(Person person)
        {
            this.Paygrade = 
        }

    }
}
