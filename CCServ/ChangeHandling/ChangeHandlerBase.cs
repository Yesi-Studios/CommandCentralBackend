using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CCServ.ChangeHandling
{
    /// <summary>
    /// The abstract class that encapsulates everything a consumer might need to track changes.
    /// </summary>
    public abstract class ChangeHandlerBase<T>
    {

        #region Properties

        /// <summary>
        /// The list of property groups in this change handler.
        /// </summary>
        public List<PropertyGroupPart<T>> PropertyGroups { get; set; }

        #endregion

        #region Fluent Methods

        /// <summary>
        /// Creates a new property group with the given properties.
        /// </summary>
        /// <param name="properties"></param>
        /// <returns></returns>
        public PropertyGroupPart<T> ForProperties(List<MemberInfo> properties)
        {
            //Make sure all the properties are for the correct type.
            if (!properties.All(x => x.DeclaringType == typeof(T)))
                throw new Exception("Not all members were from the correct type!");

            PropertyGroups.Add(new PropertyGroupPart<T>(this, properties));
            return PropertyGroups.Last();
        }

        #endregion
    }
}
