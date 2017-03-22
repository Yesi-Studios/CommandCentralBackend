using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CCServ.MetadataManagement
{
    class PermissionsPropertyDescriptor
    {

        #region Properties

        public List<PermissionsGroupPropertyDefinition> PermissionGroups { get; private set; }

        #endregion

        public PermissionsGroupPropertyDefinition EditableBy(params Authorization.ChainOfCommand[] chainsOfCommand)
        {
            PermissionGroups.Add(new PermissionsGroupPropertyDefinition(CRUDMethods.Update));
            return PermissionGroups.Last();
        }

        public PermissionsGroupPropertyDefinition ReturnableBy(params Authorization.ChainOfCommand[] chainsOfCommand)
        {
            PermissionGroups.Add(new PermissionsGroupPropertyDefinition(CRUDMethods.Retrieve));
            return PermissionGroups.Last();
        }

        public PermissionsGroupPropertyDefinition ReturnableByEveryone()
        {
            PermissionGroups.Add(new PermissionsGroupPropertyDefinition(CRUDMethods.Retrieve));
            return PermissionGroups.Last();
        }
        
    }
}
