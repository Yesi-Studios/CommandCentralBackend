using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Expressions;
using AtwoodUtils;
using System.Reflection;

namespace CommandCentral.Authorization.Groups
{
    /// <summary>
    /// Builds a module permission.
    /// </summary>
    public class ModulePart
    {
        /// <summary>
        /// The name of the module.
        /// </summary>
        public string ModuleName { get; set; }

        /// <summary>
        /// The level of access this module grants.
        /// </summary>
        public PermissionGroupLevels Level { get; set; }

        private bool wasLevelSet = false;

        /// <summary>
        /// Creates a new module with the given name.
        /// </summary>
        /// <param name="moduleName"></param>
        public ModulePart(string moduleName)
        {
            ModuleName = moduleName;
        }

        /// <summary>
        /// Sets the level of this module's permissions.
        /// </summary>
        /// <param name="level"></param>
        /// <returns></returns>
        public ModulePart AtLevel(PermissionGroupLevels level)
        {
            if (wasLevelSet)
                throw new Exception("You may not set the level of a module more than once.");

            Level = level;
            
            wasLevelSet = true;

            return this;
        }

        /// <summary>
        /// Creates a new property group with the given properties and with the access category set to edit.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="PropertyT"></typeparam>
        /// <param name="properties"></param>
        /// <returns></returns>
        public PropertyGroupPart CanEdit(params List<MemberInfo>[] members)
        {
            return new PropertyGroupPart(this) 
            { 
                AccessCategory = AccessCategories.Edit, 
                Properties = members.SelectMany(x => x).ToList()
            };
        }

        /// <summary>
        /// Creates a new property group with the given properties and with the access category set to return.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="PropertyT"></typeparam>
        /// <param name="properties"></param>
        /// <returns></returns>
        public PropertyGroupPart CanReturn(params List<MemberInfo>[] members)
        {
            return new PropertyGroupPart(this)
            {
                AccessCategory = AccessCategories.Return,
                Properties = members.SelectMany(x => x).ToList()
            };
        }

    }
}
