using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace CCServ.ChangeHandling
{
    /// <summary>
    /// Builds a property group for a change tracking declaration.
    /// </summary>
    public class PropertyGroupPart<T>
    {
        #region Properties

        /// <summary>
        /// The properties used in this group.
        /// </summary>
        public List<MemberInfo> Properties { get; set; }

        /// <summary>
        /// The parent that owns this property group.
        /// </summary>
        public ChangeHandlerBase<T> ParentChangeHandler { get; set; }

        /// <summary>
        /// Given a variance, calculates a list of changes for this list of properties.
        /// </summary>
        public Func<Variance, IEnumerable<Entities.Change>> CalculateChanges { get; set; }

        #endregion

        #region ctors

        /// <summary>
        /// Creates a new property group.
        /// </summary>
        /// <param name="parent"></param>
        public PropertyGroupPart(ChangeHandlerBase<T> parent, List<MemberInfo> properties)
        {
            Properties = properties;
            ParentChangeHandler = parent;
        }

        #endregion

        #region Fluent Methods

        /// <summary>
        /// Declares a function to use to determine the changes when a variance is detected.
        /// </summary>
        /// <param name="method"></param>
        /// <returns></returns>
        public PropertyGroupPart<T> UsingChangeHandler(Func<Variance, IEnumerable<Entities.Change>> method)
        {
            CalculateChanges = method;
            return this;
        }

        #endregion

    }
}
