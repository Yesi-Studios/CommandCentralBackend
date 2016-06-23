using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AtwoodUtils;

namespace CommandCentral.Authorization.Rules
{
    public class PermissionGroupSpecialRule : AuthorizationRuleBase
    {
        public PermissionGroupSpecialRule(AuthorizationRuleCategoryEnum category, List<string> propertyNames)
            : base(category, propertyNames)
        {
        }

        public override bool AuthorizationOperation(AuthorizationToken authToken)
        {
            List<PermissionGroup> addedGroups = new List<PermissionGroup>();
            List<PermissionGroup> removedGroups = new List<PermissionGroup>();

            //First we need the differences in the permission groups between the old DB person and the new person from the client.
            //Let's loop through all permission groups in the old person.  Any that don't exist in the new person were deleted.
            foreach (var group in authToken.OldPersonFromDB.PermissionGroups)
            {
                if (!authToken.NewPersonFromClient.PermissionGroups.Contains(group))
                    removedGroups.Add(group);
            }

            //Now go in the opposite direction.  These are the added groups.
            foreach (var group in authToken.NewPersonFromClient.PermissionGroups)
            {
                if (!authToken.OldPersonFromDB.PermissionGroups.Contains(group))
                    addedGroups.Add(group);
            }

            //Let's make sure we don't somehow have some weird duplicate permission group.
            if (addedGroups.Intersect(removedGroups).Any())
            {
                //I'm choosing to throw an error here because I can't imagine a valid situation in which this would occur and I'd like to be alerted if it does.
                throw new Exception("Somehow a person attempted to change their permissions such that they added and removed the same group.  The added groups were '{0}' and the removed groups were '{1}'."
                    .FormatS(String.Join(",", addedGroups.Select(x => x.Id.ToString())), String.Join(",", removedGroups.Select(x => x.Id.ToString()))));
            }

            //These are all the groups the client is allowed to edit the membership of.  Dat danglin' preposition though.
            var subordinateGroups = authToken.Client.PermissionGroups.SelectMany(x => x.SubordinatePermissionGroups).ToList();

            //Cool now we have the additions and the removals.  Now let's make sure that the client has admin over all of these changed groups.
            if (addedGroups.Concat(removedGroups).Any(x => !subordinateGroups.Contains(x)))
                return false;

            return true;
        }
    }
}
