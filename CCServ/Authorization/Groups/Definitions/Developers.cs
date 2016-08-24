using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CCServ.Authorization.Groups.Definitions
{
    public class Developers : PermissionGroup
    {
        /// <summary>
        /// The developers permission group. This permission group is to be granted exclusively to developers, and no one else under any circumstances.
        /// <para />
        /// These high permissions are necessary for testing and high level management. No one else should ever require this.
        /// </summary>
        public Developers()
        {
            HasAccessLevel(PermissionGroupLevels.Command);

            CanAccessSubModules(new[] { SubModules.EditNews, SubModules.AdminTools, SubModules.CreatePerson }.Select(x => x.ToString()).ToArray());

            CanEditMembershipOf(typeof(Users), typeof(DivisionLeadership), typeof(DepartmentLeadership), typeof(CommandLeadership), typeof(Admin), typeof(Developers));

            CanAccessModule("Main")
                .CanReturn(PropertySelector.SelectPropertiesFrom<Entities.Person>(
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
                .And.CanEdit(PropertySelector.SelectPropertiesFrom<Entities.Person>(
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
                    x => x.Username,
                    x => x.PermissionGroupNames));

            CanAccessModule("Muster");
        }
    }
}
