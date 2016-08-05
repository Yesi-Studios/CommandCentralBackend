using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommandCentral.Authorization.Groups.Definitions
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

            CanAccessSubModules(new[] { SubModules.EditNews }.Select(x => x.ToString()).ToArray());
            CanEditMembershipOf();

            CanAccessModule("Main")
                .AtLevel(PermissionGroupLevels.None)
                .CanReturn(PropertySelector.SelectPropertiesFrom<Entities.Person>(
                    x => x.Id,
                    x => x.LastName,
                    x => x.FirstName,
                    x => x.MiddleName,
                    x => x.Suffix,
                    x => x.Remarks,
                    x => x.Supervisor,
                    x => x.WorkCenter,
                    x => x.WorkRoom))
                .And.CanReturn(PropertySelector.SelectPropertiesFrom<Entities.Person>(
                    x => x.SSN))
                    .IfSelf()
                .And.CanEdit(PropertySelector.SelectPropertiesFrom<Entities.Person>(
                    x => x.FirstName,
                    x => x.LastName,
                    x => x.MiddleName))
                    .IfSelf();
        }
    }
}
