using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CCServ.Authorization.Groups.Definitions
{
    /// <summary>
    /// The users permission group.  Default, and assigned to all persons.
    /// </summary>
    public class Users : PermissionGroup
    {
        /// <summary>
        /// The users permission group.  Default, and assigned to all persons.
        /// </summary>
        public Users()
        {
            Name("Users");
            Default();
            
            CanEditMembershipOf();

            HasAccessLevel(PermissionGroupLevels.Self);

            CanAccessModule("Muster");

            CanAccessModule("Main")
                .CanReturn(PropertySelector.SelectPropertiesFrom<Entities.Person>(
                    x => x.Id,
                    x => x.LastName,
                    x => x.FirstName,
                    x => x.MiddleName,
                    x => x.Suffix,
                    x => x.Remarks,
                    x => x.Supervisor,
                    x => x.WorkCenter,
                    x => x.WorkRoom,
                    x => x.Shift,
                    x => x.CurrentMusterStatus,
                    x => x.EmailAddresses,
                    x => x.PhoneNumbers,
                    x => x.EmergencyContactInstructions))
                .And.CanReturn(PropertySelector.SelectPropertiesFrom<Entities.Person>(
                    x => x.Id,
                    x => x.LastName,
                    x => x.FirstName,
                    x => x.MiddleName,
                    x => x.SSN,
                    x => x.Suffix,
                    x => x.DateOfBirth,
                    x => x.Sex,
                    x => x.Remarks,
                    x => x.Ethnicity,
                    x => x.ReligiousPreference,
                    x => x.Paygrade,
                    x => x.Designation,
                    x => x.Division,
                    x => x.Department,
                    x => x.Command,
                    x => x.PrimaryNEC,
                    x => x.SecondaryNECs,
                    x => x.Supervisor,
                    x => x.WorkCenter,
                    x => x.WorkRoom,
                    x => x.Shift,
                    x => x.WorkRemarks,
                    x => x.DutyStatus,
                    x => x.UIC,
                    x => x.DateOfArrival,
                    x => x.JobTitle,
                    x => x.EAOS,
                    x => x.DateOfDeparture,
                    x => x.CurrentMusterStatus,
                    x => x.EmailAddresses,
                    x => x.PhoneNumbers,
                    x => x.PhysicalAddresses,
                    x => x.EmergencyContactInstructions,
                    x => x.ContactRemarks,
                    x => x.IsClaimed,
                    x => x.Username,
                    x => x.PermissionGroupNames,
                    x => x.AccountHistory,
                    x => x.Changes))
                    .IfSelf()
                .And.CanEdit(PropertySelector.SelectPropertiesFrom<Entities.Person>(
                    x => x.LastName,
                    x => x.FirstName,
                    x => x.MiddleName,
                    x => x.Suffix,
                    x => x.ReligiousPreference,
                    x => x.CurrentMusterStatus,
                    x => x.EmailAddresses,
                    x => x.PhoneNumbers,
                    x => x.PhysicalAddresses,
                    x => x.EmergencyContactInstructions,
                    x => x.ContactRemarks))
                    .IfSelf();
        }
    }
}
