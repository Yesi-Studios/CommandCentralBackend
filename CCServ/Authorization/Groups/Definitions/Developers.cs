using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AtwoodUtils;

namespace CCServ.Authorization.Groups.Definitions
{
    /// <summary>
    /// The gods in the system.  
    /// </summary>
    public class Developers : PermissionGroup
    {
        /// <summary>
        /// The developers permission group. This permission group is to be granted exclusively to developers, and no one else under any circumstances.
        /// <para />
        /// These high permissions are necessary for testing and high level management. No one else should ever require this.
        /// </summary>
        public Developers()
        {
            HasAccessLevel(ChainOfCommandLevels.Command);

            CanAccessSubModules(SubModules.EditNews, SubModules.AdminTools, SubModules.CreatePerson, SubModules.EditFAQ);

            CanEditMembershipOf(typeof(Users), typeof(DivisionLeadership), typeof(DepartmentLeadership), typeof(CommandLeadership), 
                typeof(Admin), typeof(Developers), typeof(DivisionMuster), typeof(DepartmentMuster), typeof(CommandMuster));

            InChainsOfCommand(ChainsOfCommand.Main, ChainsOfCommand.Muster);

            CanAccessModule("Main")
                .CanReturn(PropertySelector.SelectPropertiesFrom<Entities.Person>(
                    x => x.SSN,
                    x => x.DateOfBirth,
                    x => x.Ethnicity,
                    x => x.ReligiousPreference,
                    x => x.PrimaryNEC,
                    x => x.SecondaryNECs,
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
                    x => x.PRD,
                    x => x.WatchQualifications,
                    x => x.GTCTrainingDate,
                    x => x.HasCompletedAWARE,
                    x => x.ADAMSTrainingDate,
                    x => x.DoDId))
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
                    x => x.CurrentMusterRecord,
                    x => x.EmailAddresses,
                    x => x.PhoneNumbers,
                    x => x.PhysicalAddresses,
                    x => x.EmergencyContactInstructions,
                    x => x.ContactRemarks,
                    x => x.Username,
                    x => x.PermissionGroupNames,
                    x => x.PRD,
                    x => x.WatchQualifications,
                    x => x.GTCTrainingDate,
                    x => x.HasCompletedAWARE,
                    x => x.ADAMSTrainingDate,
                    x => x.DoDId));

            CanAccessModule("Muster");
        }
    }
}
