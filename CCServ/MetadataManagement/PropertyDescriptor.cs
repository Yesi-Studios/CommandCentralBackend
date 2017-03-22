using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CCServ.MetadataManagement
{
    class PropertyDescriptor
    {

        #region Properties

        public MemberInfo Property { get; private set; }

        public PermissionsPropertyDescriptor PermissionsDescriptor { get; private set; }

        public bool IsRequired { get; private set; }

        #endregion

        #region ctors

        public PropertyDescriptor(MemberInfo property)
        {
            this.Property = property;
            PermissionsDescriptor = new PermissionsPropertyDescriptor();
        }

        #endregion

        #region FluentMethods

        public PropertyDescriptor Permissions(params Action<PermissionsPropertyDescriptor>[] permissionsDeclarers)
        {
            foreach (var declarer in permissionsDeclarers)
            {
                declarer(PermissionsDescriptor);
            }
            return this;
        }

        public PropertyDescriptor Required()
        {
            IsRequired = true;
            return this;
        }



        #endregion

    }
}
