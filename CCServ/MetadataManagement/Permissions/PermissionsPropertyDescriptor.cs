using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CCServ.MetadataManagement.Permissions
{
    /// <summary>
    /// The list of permissions for a given property.
    /// </summary>
    /// <returns></returns>
    public class PermissionsPropertyDescriptor
    {

        #region Properties

        /// <summary>
        /// TODO
        /// </summary>
        /// <returns></returns>
        public List<PermissionsGroupPropertyDefinition> PermissionGroups { get; private set; }

        #endregion

        #region ctors

        /// <summary>
        /// TODO
        /// </summary>
        /// <returns></returns>
        public PermissionsPropertyDescriptor()
        {
            PermissionGroups = new List<PermissionsGroupPropertyDefinition>();
        }

        #endregion

        #region Fluent Methods

        /// <summary>
        /// TODO
        /// </summary>
        /// <returns></returns>
        public PermissionsGroupPropertyDefinition EditableBy(params Authorization.ChainOfCommand[] chainsOfCommand)
        {
            PermissionGroups.Add(new PermissionsGroupPropertyDefinition(CRUDMethods.Update));
            return PermissionGroups.Last();
        }

        /// <summary>
        /// TODO
        /// </summary>
        /// <returns></returns>
        public PermissionsGroupPropertyDefinition EditableByEveryone()
        {
            PermissionGroups.Add(new PermissionsGroupPropertyDefinition(CRUDMethods.Update));
            return PermissionGroups.Last();
        }

        /// <summary>
        /// TODO
        /// </summary>
        /// <returns></returns>
        public PermissionsGroupPropertyDefinition EditableBySelf()
        {
            PermissionGroups.Add(new PermissionsGroupPropertyDefinition(CRUDMethods.Update));
            return PermissionGroups.Last();
        }

        /// <summary>
        /// TODO
        /// </summary>
        /// <returns></returns>
        public PermissionsGroupPropertyDefinition EditableByNoone()
        {
            PermissionGroups.Add(new PermissionsGroupPropertyDefinition(CRUDMethods.Update));
            return PermissionGroups.Last();
        }

        /// <summary>
        /// TODO
        /// </summary>
        /// <returns></returns>
        public PermissionsGroupPropertyDefinition ReturnableBy(params Authorization.ChainOfCommand[] chainsOfCommand)
        {
            PermissionGroups.Add(new PermissionsGroupPropertyDefinition(CRUDMethods.Retrieve));
            return PermissionGroups.Last();
        }

        /// <summary>
        /// TODO
        /// </summary>
        /// <returns></returns>
        public PermissionsGroupPropertyDefinition ReturnableByEveryone()
        {
            PermissionGroups.Add(new PermissionsGroupPropertyDefinition(CRUDMethods.Retrieve));
            return PermissionGroups.Last();
        }

        /// <summary>
        /// TODO
        /// </summary>
        /// <returns></returns>
        public PermissionsGroupPropertyDefinition ReturnableBySelf()
        {
            PermissionGroups.Add(new PermissionsGroupPropertyDefinition(CRUDMethods.Retrieve));
            return PermissionGroups.Last();
        }

        /// <summary>
        /// TODO
        /// </summary>
        /// <returns></returns>
        public PermissionsGroupPropertyDefinition ReturnableByNoone()
        {
            PermissionGroups.Add(new PermissionsGroupPropertyDefinition(CRUDMethods.Retrieve));
            return PermissionGroups.Last();
        }

        #endregion

    }
}
