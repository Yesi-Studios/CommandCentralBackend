using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using AtwoodUtils;

namespace CCServ.MetadataManagement
{
    abstract class AbstractPropertiesDescriptor<T>
    {

        #region Properties

        protected List<PropertyDescriptor> Properties { get; private set; }

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

        #endregion

    }
}
