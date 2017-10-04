using CommandCentral.Authorization.Groups;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Assert = Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace CommandCentral.Test
{
    public static class PermissionsTests
    {
        public static void EnsureNoDuplicatePermissions()
        {
            var thereAreDupes = PermissionGroup.AllPermissionGroups.GroupBy(x => x.GroupName, StringComparer.CurrentCultureIgnoreCase).Any(x => x.Count() > 1);

            Assert.IsFalse(thereAreDupes);
        }

    }
}
