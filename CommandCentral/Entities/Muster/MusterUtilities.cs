using CommandCentral.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommandCentral.Entities.Muster
{
    public static class MusterUtilities
    {

        public static bool CanMusterPerson(this Person person, Person other)
        {
            var resolvedPermissions = person.ResolvePermissions(other);

            return resolvedPermissions.IsInChainOfCommand[ChainsOfCommand.Muster];
        }



    }
}
