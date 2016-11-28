using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AtwoodUtils;

namespace CCServ.Authorization.Groups.Definitions
{
    public class CommandLeadership : PermissionGroup
    {
        /// <summary>
        /// The Command Leadership permission group. For the top leadership in the command.
        /// </summary>
        public CommandLeadership()
        {
            CanAccessSubModules(SubModules.AdminTools, SubModules.CreatePerson);

            CanEditMembershipOf(typeof(Users), typeof(DivisionLeadership), typeof(DepartmentLeadership), typeof(Admin), typeof(CommandLeadership),
                typeof(DivisionMuster), typeof(DepartmentMuster), typeof(CommandMuster));

            InChainsOfCommand(ChainsOfCommand.Main, ChainsOfCommand.Muster);

            HasAccessLevel(ChainOfCommandLevels.Command);

            CanAccessModule("Main")
                .CanReturn(PropertySelector.SelectPropertiesFrom<Entities.Person>(
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
                    x => x.PRD,
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
                    x => x.SecondaryNECs,
                    x => x.EAOS,
                    x => x.PRD,
                    x => x.WatchQualifications))
                    .IfInChainOfCommand();

            CanAccessModule("Muster");
        }
    }
}
