using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CCServ.Authorization.Groups.Definitions
{
    public class DivisionLeadership : PermissionGroup
    {
        /// <summary>
        /// The Chiefs permission group. Should be given to all division and department Chiefs, so they can manage sailors in their charge.
        /// </summary>
        public DivisionLeadership()
        {
            CanEditMembershipOf(typeof(Users), typeof(DivisionLeadership));

            HasAccessLevel(PermissionGroupLevels.Division);

            CanAccessModule("Main")
                .CanReturn(PropertySelector.SelectPropertiesFrom<Entities.Person>(
                    x => x.DateOfBirth,
                    x => x.Ethnicity,
                    x => x.ReligiousPreference,
                    x => x.PrimaryNEC,
                    x => x.SecondaryNECs,
                    x => x.Shift,
                    x => x.WorkRemarks,
                    x => x.DutyStatus,
                    x => x.DateOfArrival,
                    x => x.JobTitle,
                    x => x.EAOS,
                    x => x.DateOfDeparture,
                    x => x.EmailAddresses,
                    x => x.PhoneNumbers,
                    x => x.PhysicalAddresses,
                    x => x.ContactRemarks,
                    x => x.IsClaimed,
                    x => x.Username,
                    x => x.PermissionGroupNames,
                    x => x.AccountHistory,
                    x => x.Changes,
                    x => x.SSN))
                    .IfInChainOfCommand()
                .And.CanEdit(PropertySelector.SelectPropertiesFrom<Entities.Person>(
                    x => x.LastName,
                    x => x.FirstName,
                    x => x.MiddleName,
                    x => x.Suffix,
                    x => x.DateOfBirth,
                    x => x.Sex,
                    x => x.Remarks,
                    x => x.Ethnicity,
                    x => x.Paygrade,
                    x => x.Designation,
                    x => x.Division,
                    x => x.Department,
                    x => x.Supervisor,
                    x => x.WorkCenter,
                    x => x.WorkRoom,
                    x => x.Shift,
                    x => x.WorkRemarks,
                    x => x.JobTitle,
                    x => x.CurrentMusterStatus,
                    x => x.EmailAddresses,
                    x => x.PhoneNumbers,
                    x => x.PhysicalAddresses,
                    x => x.EmergencyContactInstructions,
                    x => x.ContactRemarks,
                    x => x.ReligiousPreference,
                    x => x.Command,
                    x => x.DutyStatus,
                    x => x.UIC,
                    x => x.PrimaryNEC,
                    x => x.SecondaryNECs))
                    .IfInChainOfCommand();

            CanAccessModule("Muster");
        }
    }
}
