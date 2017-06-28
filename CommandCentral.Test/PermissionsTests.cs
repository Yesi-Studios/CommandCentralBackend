using CommandCentral.Authorization.Groups;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CommandCentral.Test
{
    [TestClass]
    public class PermissionsTests
    {

        [TestMethod]
        public void EnsureNoDuplicatePermissions()
        {
            var thereAreDupes = PermissionGroup.AllPermissionGroups.GroupBy(x => x.GroupName, StringComparer.CurrentCultureIgnoreCase).Any(x => x.Count() > 1);

            Assert.IsFalse(thereAreDupes);
        }

    }
}
