using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommandCentral.Authorization.Groups.Definitions
{
    public class Users : PermissionGroup
    {
        public Users()
        {
            Name("Users");
            Default();

            CanAccessSubModules("CreatePerson", "SomethingElse");
            CanEditMembershipOf();

            CanAccessModule("Main")
                .AtLevel(PermissionGroupLevels.Division)
                .CanEdit(PropertySelector.Properties<Entities.Person, string>(
                    x => x.SSN,
                    x => x.FirstName), PropertySelector.Properties<Entities.Person, DateTime?>(
                    x => x.DateOfBirth))
                    .IfInChainOfCommand()
                .And.


        }
    }
}
