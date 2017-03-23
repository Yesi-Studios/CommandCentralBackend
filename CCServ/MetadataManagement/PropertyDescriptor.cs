using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using CCServ.MetadataManagement.Permissions;

namespace CCServ.MetadataManagement
{
    /// <summary>
    /// Describes the metadata relating to a single property.
    /// </summary>
    public class PropertyDescriptor<T>
    {

        #region Properties

        /// <summary>
        /// The property selector.
        /// </summary>
        public Expression<Func<T, object>> PropertyExpression { get; private set; }

        /// <summary>
        /// This contains all of the permissions related to this property: who can edit it and who can return it.
        /// </summary>
        public PermissionsPropertyDescriptor PermissionsDescriptor { get; private set; }

        #endregion

        #region ctors

        /// <summary>
        /// Creates a new property.
        /// </summary>
        /// <returns></returns>
        public PropertyDescriptor(Expression<Func<T, object>> expression)
        {
            this.PropertyExpression = expression;
            PermissionsDescriptor = new PermissionsPropertyDescriptor();
        }

        #endregion

        #region FluentMethods

        /// <summary>
        /// Starts the permissions declaration chain.
        /// </summary>
        /// <param name="permissionsDeclarer"></param>
        /// <returns></returns>
        public PropertyDescriptor<T> Permissions(Action<PermissionsPropertyDescriptor> permissionsDeclarer)
        {
            permissionsDeclarer(PermissionsDescriptor);
            return this;
        }

        #endregion

    }
}
