using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AtwoodUtils;

namespace CommandCentral.Authorization.Groups
{
    public class ModulePermission
    {
        public string ModuleName { get; set; }

        public ConcurrentBag<PropertyPermission> PropertyPermissions { get; private set; }

        public void AddPropertyPermission(Type type, string propertyName)
        {
            if (!type.GetProperties().Select(x => x.Name).Contains(propertyName))
                throw new ArgumentException("The type, '{0}', has no property named '{1}'.".FormatS(type.Name, propertyName));

            if (PropertyPermissions.Any(x => x.PropertyName == propertyName && x.Type == type))
                throw new ArgumentException("For the module, '{0}', you have already declared access to the property, '{1}', in the type, '{2}'.".FormatS(ModuleName, propertyName, type.Name));

            PropertyPermissions.Add(new PropertyPermission { ParentModulePermission = this, PropertyName = propertyName, Type = type });
        }

        public ModulePermission()
        {
            ModuleName = "";
            PropertyPermissions = new ConcurrentBag<PropertyPermission>();
        }
    }
}
