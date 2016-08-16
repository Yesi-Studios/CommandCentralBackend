using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CCServ.Authorization.Groups.Definitions
{
    class LPOs : PermissionGroup
    {
        /// <summary>
        /// The users permission group.  Default, and assigned to all persons.
        /// </summary>
        public LPOs()
        {
            CanEditMembershipOf();

            HasAccessLevel(PermissionGroupLevels.Division);

            CanAccessModule("Main")
                .CanReturn(PropertySelector.SelectPropertiesFrom<Entities.Person>(
                    x => x.Id,
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
                    x => x.Command,
                    x => x.NECs,
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
                    x => x.ContactRemarks,
                    x => x.IsClaimed,
                    x => x.PermissionGroupNames));

            CanAccessModule("Muster");
        }
    }
}
