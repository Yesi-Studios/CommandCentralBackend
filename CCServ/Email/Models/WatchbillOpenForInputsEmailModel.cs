using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CCServ.Email.Models
{
    /// <summary>
    /// The email model that is sent to the watchbill open for inputs template.
    /// </summary>
    public class WatchbillOpenForInputsEmailModel
    {
        /// <summary>
        /// The name or title of the watchbill referenced in the email.
        /// </summary>
        public Entities.Watchbill.Watchbill Watchbill { get; set; }

        /// <summary>
        /// The collection of those people who are not qualified for any watches on this watchbill.
        /// </summary>
        public List<Entities.Person> NotQualledPersons
        {
            get
            {
                //We're also going to go see who is in the eligibility group and has no watch qualifications that pertain to this watchbill.
                //First we need to know all the possible needed watch qualifications.
                var watchQualifications = this.Watchbill.WatchShifts.SelectMany(x => x.ShiftType.RequiredWatchQualifications);

                var personsWithoutAtLeastOneQualification = this.Watchbill.EligibilityGroup.EligiblePersons
                    .Where(person => !person.WatchQualifications.Any(qual => watchQualifications.Contains(qual)));

                return personsWithoutAtLeastOneQualification.ToList();
            }
        }
    }
}
