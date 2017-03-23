using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using AtwoodUtils;

namespace CCServ.MetadataManagement
{
    public abstract class AbstractPropertiesDescriptor<T>
    {

        #region Properties

        protected List<PropertyDescriptor> Properties { get; private set; }

        protected List<GlobalPermissionsDescriptor> GlobalPermissionsDescriptors { get; set; }

        #endregion

        #region ctors

        public AbstractPropertiesDescriptor()
        {
            Properties = new List<PropertyDescriptor>();
        }

        #endregion

        #region Fluent Methods

        public PropertyDescriptor Property(Expression<Func<T, object>> expression)
        {
            Properties.Add(new PropertyDescriptor(expression.GetProperty()));
            return Properties.Last();
        }

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
