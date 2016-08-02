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
    /// Builds a property group.
    /// </summary>
    public class PropertyGroupPart<T>
    {
        /// <summary>
        /// Indicates for which access category this property group was made.
        /// </summary>
        public AccessCategories AccessCategory { get; set; }

        /// <summary>
        /// The list of the properties referenced by this property group.
        /// </summary>
        public List<MemberInfo> Properties { get; set; }

        /// <summary>
        /// The list of disjunctions held in this property group.
        /// </summary>
        public List<Rules.RuleDisjunction<T>> Disjunctions { get; set; }

        /// <summary>
        /// Creates a new property group.
        /// </summary>
        public PropertyGroupPart()
        {
            Properties = new List<MemberInfo>();
            Disjunctions = new List<Rules.RuleDisjunction<T>>();
        }

    }
}
