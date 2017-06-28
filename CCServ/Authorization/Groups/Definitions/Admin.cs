using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AtwoodUtils;

namespace CommandCentral.Authorization.Groups.Definitions
{
    /// <summary>
    /// The administrative admin group which is pretty much the same as command leadership.
    /// <para/>
    /// If the application were ever used at multiple commands, we'd revisit this permission group.
    /// </summary>
    public class Admin : PermissionGroup
    {
        /// <summary>
        /// Declares the admin group's permissions.
        /// </summary>
        public Admin()
        {
            CanAccessSubModules(SubModules.AdminTools, SubModules.CreatePerson);

            CanEditMembershipOf(typeof(Users), typeof(DivisionLeadership), typeof(DepartmentLeadership), typeof(Admin),
                typeof(DivisionMuster), typeof(DepartmentMuster), typeof(CommandMuster), typeof(CommandLeadership));

            HasAccessLevel(ChainOfCommandLevels.Command);

            HasChainOfCommand(ChainsOfCommand.Main)
                .CanReturn(PropertySelector.SelectPropertiesFrom<Entities.Person>(
                    x => x.SSN,
                    x => x.DoDId,
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
                    x => x.BilletAssignment))
                    .IfInChainOfCommand()
                .And.CanEdit(PropertySelector.SelectPropertiesFrom<Entities.Person>(
                    x => x.LastName,
                    x => x.FirstName,
                    x => x.MiddleName,
                    x => x.SSN,
                    x => x.DoDId,
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
                    x => x.EAOS,
                    x => x.PRD,
                    x => x.WatchQualifications,
                    x => x.GTCTrainingDate,
                    x => x.HasCompletedAWARE,
                    x => x.ADAMSTrainingDate,
                    x => x.BilletAssignment,
                    x => x.SubscribedEvents))
                    .IfInChainOfCommand();

            HasChainOfCommand(ChainsOfCommand.Muster);
        }
    }
}
