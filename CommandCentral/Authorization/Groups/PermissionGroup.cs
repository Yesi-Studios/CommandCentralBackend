using System;
using System.Collections.Generic;
using FluentNHibernate.Mapping;
using System.Linq;
using CommandCentral.ClientAccess;
using AtwoodUtils;

namespace CommandCentral.Authorization.Groups
{
    /// <summary>
    /// Describes a single permission group.
    /// </summary>
    public class PermissionGroup
    {

        #region Properties

        /// <summary>
        /// The name of the permission group
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Indicates that this permission group is the default. Default : false
        /// </summary>
        public bool IsDefault { get; set; }

        /// <summary>
        /// The list of module permissions in this permission group.
        /// </summary>
        public List<ModulePermission> ModulePermissions { get; set; }
        
        #endregion

        #region ctors

        public PermissionGroup()
        {
            IsDefault = false;
        }

        #endregion

    }
}