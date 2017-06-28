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
    /// Builds a chain of command permission.
    /// </summary>
    public class ChainOfCommandPart
    {
        /// <summary>
        /// The name of the chain of command.
        /// </summary>
        public ChainsOfCommand ChainOfCommand { get; set; }

        /// <summary>
        /// The list of property groups in this chain of command.
        /// </summary>
        public List<PropertyGroupPart> PropertyGroups { get; set; }

        /// <summary>
        /// The parent permission group.
        /// </summary>
        public PermissionGroup ParentPermissionGroup { get; set; }

        /// <summary>
        /// Creates a new chain of command permission.
        /// </summary>
        /// <param name="chainOfCommand"></param>
        public ChainOfCommandPart(ChainsOfCommand chainOfCommand)
        {
            ChainOfCommand = chainOfCommand;
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
