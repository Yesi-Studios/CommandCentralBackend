using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using AtwoodUtils;
using FluentNHibernate.Mapping;
using CCServ.MetadataManagement.Permissions;

namespace CCServ.MetadataManagement
{
    /// <summary>
    /// Implement this class in order to declare metadata about your object.
    /// </summary>
    /// <returns></returns>
    public abstract class ClassMetadata<T>
    {

        #region Properties

        /// <summary>
        /// The list of properties contained in this class metadata.
        /// </summary>
        /// <returns></returns>
        protected List<PropertyDescriptor<T>> Properties { get; private set; }

        /// <summary>
        /// The global permissions which aren't tied to any one property.
        /// </summary>
        /// <returns></returns>
        protected List<GlobalPermissionsDescriptor> GlobalPermissionsDescriptors { get; set; }

        #endregion

        #region ctors

        /// <summary>
        /// Creates a new class metadata.
        /// </summary>
        /// <returns></returns>
        public ClassMetadata()
        {
            Properties = new List<PropertyDescriptor<T>>();
        }

        #endregion

        #region Fluent Methods

        /// <summary>
        /// Starts a property description.
        /// </summary>
        /// <returns></returns>
        public PropertyDescriptor<T> Property(Expression<Func<T, object>> expression)
        {
            Properties.Add(new PropertyDescriptor<T>(expression));
            return Properties.Last();
        }

        /// <summary>
        /// Starts a global permissions declaration.
        /// </summary>
        /// <returns></returns>
        public GlobalPermissionsDescriptor InOrderTo
        {
            get
            {
                GlobalPermissionsDescriptors.Add(new GlobalPermissionsDescriptor());
                return GlobalPermissionsDescriptors.Last();
            }
        }

        #endregion

    }
}
