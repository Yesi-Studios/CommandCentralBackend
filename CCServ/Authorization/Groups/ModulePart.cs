using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Expressions;
using AtwoodUtils;
using System.Reflection;

namespace CCServ.Authorization.Groups
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
        /// The list of property groups in this module.
        /// </summary>
        public List<PropertyGroupPart> PropertyGroups { get; set; }

        /// <summary>
        /// The parent permission group.
        /// </summary>
        public PermissionGroup ParentPermissionGroup { get; set; }

        private bool wasLevelSet = false;

        /// <summary>
        /// Creates a new module with the given name.
        /// </summary>
        /// <param name="moduleName"></param>
        public ModulePart(string moduleName)
        {
            ModuleName = moduleName;
            PropertyGroups = new List<PropertyGroupPart>();
        }
        
        /// <summary>
        /// Creates a new property group with the given properties and with the access category set to edit.
        /// </summary>
        /// <param name="members"></param>
        /// <returns></returns>
        public PropertyGroupPart CanEdit(params List<MemberInfo>[] members)
        {
            PropertyGroups.Add(new PropertyGroupPart(this)
            {
                AccessCategory = AccessCategories.Edit,
                Properties = members.SelectMany(x => x).ToList()
            });
            return PropertyGroups.Last();
        }

        /// <summary>
        /// Creates a new property group with the given properties and with the access category set to return.
        /// </summary>
        /// <param name="members"></param>
        /// <returns></returns>
        public PropertyGroupPart CanReturn(params List<MemberInfo>[] members)
        {
            PropertyGroups.Add(new PropertyGroupPart(this)
            {
                AccessCategory = AccessCategories.Return,
                Properties = members.SelectMany(x => x).ToList()
            });
            return PropertyGroups.Last();
        }

    }
}
