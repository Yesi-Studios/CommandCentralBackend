using CommandCentral.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommandCentral.Entities.Muster
{
    public static class MusterUtility
    {

        public static bool CanMusterPerson(this Person person, Person otherPerson)
        {
            var resolvedPermissions = person.ResolvePermissions(otherPerson);

            return resolvedPermissions.IsInChainOfCommand[ChainsOfCommand.Muster];
        }

    }
}
