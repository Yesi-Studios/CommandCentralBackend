using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AtwoodUtils;
using CommandCentral.ServiceManagement;

namespace CommandCentral.Authorization
{
    /// <summary>
    /// Utilities to help consumers interact with permissions.
    /// </summary>
    public static class AuthorizationUtilities
    {
        /// <summary>
        /// Returns a list of permission groups derived from the given list of permission group names.  Throws an exception if a group name does not resolve to a group.
        /// <para />
        /// Names are not case sensitive.
        /// <para />
        /// Optionally includes default groups onto the resulting list.
        /// </summary>
        /// <param name="groupNames"></param>
        /// <param name="includeDefaults"></param>
        /// <returns></returns>
        public static List<Groups.PermissionGroup> GetPermissionGroupsFromNames(IEnumerable<string> groupNames, bool includeDefaults)
        {
            List<Groups.PermissionGroup> groups = new List<Groups.PermissionGroup>();

            foreach (var groupName in groupNames)
            {
                var group = Groups.PermissionGroup.AllPermissionGroups.FirstOrDefault(x => x.GroupName.SafeEquals(groupName));

                if (group == null)
                    throw new Exception("The group name, '{0}', was not valid.".FormatS(groupName));

                groups.Add(group);
            }

            if (includeDefaults)
                groups.AddRange(Groups.PermissionGroup.AllPermissionGroups.Where(x => x.IsDefault));

            return groups;
        }
    }
}
