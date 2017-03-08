using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AtwoodUtils;

namespace CCServ.Authorization.Groups.Definitions
{
    public class DepartmentLeadership : PermissionGroup
    {
        /// <summary>
        /// The department leadership group. Allows department LPOs, Chiefs, and Department Heads to manage their sailors.
        /// </summary>
        public DepartmentLeadership()
        {
            CanEditMembershipOf(typeof(Users), typeof(DivisionLeadership), typeof(DepartmentLeadership), typeof(DivisionMuster), typeof(DepartmentMuster));

            HasAccessLevel(ChainOfCommandLevels.Department);

            InChainsOfCommand(ChainsOfCommand.Main, ChainsOfCommand.Muster, ChainsOfCommand.QuarterdeckWatchbill);

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
                    x => x.SSN,
                    x => x.DoDId,
                    x => x.PRD))
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
                    x => x.CurrentMusterRecord,
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
                    x => x.PRD,
                    x => x.EAOS,
                    x => x.WatchQualifications,
                    x => x.GTCTrainingDate,
                    x => x.HasCompletedAWARE,
                    x => x.ADAMSTrainingDate))
                    .IfInChainOfCommand();

            CanAccessModule("Muster");
        }
    }
}
