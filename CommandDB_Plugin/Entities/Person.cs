using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommandCentral.Entities
{
    /// <summary>
    /// Describes a single person and all their properties and data access methods.
    /// </summary>
    public class Person
    {

        #region Properties

        /// <summary>
        /// The person's unique ID.
        /// </summary>
        public string ID { get; set; }

        #region Main Properties

        /// <summary>
        /// The person's last name.
        /// </summary>
        public string LastName { get; set; }

        /// <summary>
        /// The person's first name.
        /// </summary>
        public string FirstName { get; set; }

        /// <summary>
        /// The person's middle name.
        /// </summary>
        public string MiddleName { get; set; }

        /// <summary>
        /// The person's SSN.
        /// </summary>
        public string SSN { get; set; }

        /// <summary>
        /// The person's date of birth.
        /// </summary>
        public DateTime? DateOfBirth { get; set; }

        /// <summary>
        /// The person's sex.
        /// </summary>
        public ReferenceLists.Sex Sex { get; set; }

        /// <summary>
        /// The person's remarks.  This is the primary comments section
        /// </summary>
        public string Remarks { get; set; }

        /// <summary>
        /// Stores the person's ethnicity.
        /// </summary>
        public ReferenceLists.Ethnicity Ethnicity { get; set; }

        /// <summary>
        /// The person's religious preference
        /// </summary>
        public ReferenceLists.ReligiousPreference ReligiousPreference { get; set; }

        /// <summary>
        /// The person's suffix, sch as IV, Esquire, etc.
        /// </summary>
        public ReferenceLists.Suffix Suffix { get; set; }

        /// <summary>
        /// The person's rank (e5, etc.)
        /// </summary>
        public ReferenceLists.Rank Rank { get; set; }

        /// <summary>
        /// The person's rate (CTI2, CTR1)
        /// </summary>
        public ReferenceLists.Rate Rate { get; set; }

        /// <summary>
        /// The person's division
        /// </summary>
        public ReferenceLists.Division Division { get; set; }

        /// <summary>
        /// The person's department
        /// </summary>
        public ReferenceLists.Department Department { get; set; }

        /// <summary>
        /// The person's command
        /// </summary>
        public ReferenceLists.Command Command { get; set; }

        #endregion

        #region Work Properties

        /// <summary>
        ///The person's billet.
        /// </summary>
        public Billet Billet { get; set; }

        /// <summary>
        /// The NECs of the person.
        /// </summary>
        public List<ReferenceLists.NEC> NECs { get; set; }

        /// <summary>
        /// The person's supervisor
        /// </summary>
        public string Supervisor { get; set; }

        /// <summary>
        /// The person's work center.
        /// </summary>
        public string WorkCenter { get; set; }

        /// <summary>
        /// The room in which the person works.
        /// </summary>
        public string WorkRoom { get; set; }

        /// <summary>
        /// A free form text field intended to let the client store the shift of a person - however the client wants to do that.
        /// </summary>
        public string Shift { get; set; }

        /// <summary>
        /// The comments section for the work page
        /// </summary>
        public string WorkRemarks { get; set; }

        /// <summary>
        /// The person's duty status
        /// </summary>
        public ReferenceLists.DutyStatus DutyStatus { get; set; }

        /// <summary>
        /// The person's UIC
        /// </summary>
        public ReferenceLists.UIC UIC { get; set; }

        /// <summary>
        /// The date/time that the person arrived at the command.
        /// </summary>
        public DateTime? DateOfArrival { get; set; }

        /// <summary>
        /// The client's job title.
        /// </summary>
        public string JobTitle { get; set; }

        /// <summary>
        /// The date/time of the end of active obligatory service (EAOS) for the person.
        /// </summary>
        public DateTime? EAOS { get; set; }

        /// <summary>
        /// The date/time that the client left/will leave the command.
        /// </summary>
        public DateTime? DateOfDeparture { get; set; }

        #endregion

        #region Contacts Properties

        /// <summary>
        /// The email addresses of this person.
        /// </summary>
        public List<EmailAddress> EmailAddresses { get; set; }

        /// <summary>
        /// The Phone Numbers of this person.
        /// </summary>
        public List<PhoneNumber> PhoneNumbers { get; set; }

        /// <summary>
        /// The Physical Addresses of this person
        /// </summary>
        public List<PhysicalAddress> PhysicalAddresses { get; set; }

        /// <summary>
        /// Instructions from the user on what avenues of contact to follow in the case of an emergency.
        /// </summary>
        public string EmergencyContactInstructions { get; set; }

        /// <summary>
        /// A free form text field intended to allow the user to make comments about their contact fields.
        /// </summary>
        public string ContactRemarks { get; set; }

        #endregion

        #region Account

        /// <summary>
        /// A boolean indicating whether or not this account has been claimed.
        /// </summary>
        public bool IsClaimed { get; set; }

        /// <summary>
        /// The list of the person's permissions.
        /// </summary>
        public List<Authorization.PermissionGroup> PermissionGroups { get; set; }

        /// <summary>
        /// The list of change events to which the person is subscribed.
        /// </summary>
        public List<ChangeEvents.ChangeEvent> SubscribedChangeEvents { get; set; }

        /// <summary>
        /// A list containing account history events, these are events that track things liks login, password reset, etc.
        /// </summary>
        public List<AccountHistoryEvents.AccountHistoryEvent> AccountHistory { get; set; }

        #endregion

        #endregion

    }
}
