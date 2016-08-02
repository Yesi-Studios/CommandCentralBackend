using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommandCentral.Authorization.Groups
{
    /// <summary>
    /// Builds a permission group.
    /// </summary>
    public class PermissionGroupPart
    {
        /// <summary>
        /// The name of the permission group.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The module included in this.
        /// </summary>
        public List<ModulePart> Modules { get; set; }

        /// <summary>
        /// Sets this to default, meaning all users should have it.
        /// </summary>
        public bool IsDefault { get; set; }

        /// <summary>
        /// Sets the resulting permission group to be default.
        /// </summary>
        /// <returns></returns>
        public PermissionGroupPart Default()
        {
            IsDefault = true;
            return this;
        }

        /// <summary>
        /// Creates a new module permission and sets its name.
        /// </summary>
        /// <param name="moduleName"></param>
        /// <returns></returns>
        public ModulePart CanAccessModule(string moduleName)
        {
            Modules.Add(new ModulePart(moduleName));
            return Modules.Last();
        }

    }
}
