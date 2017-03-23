using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CCServ.MetadataManagement.Permissions
{
    /// <summary>
    /// TODO
    /// </summary>
    /// <returns></returns>
    public class PermissionsGroupPropertyDefinition
    {

        #region Properties

        /// <summary>
        /// TODO
        /// </summary>
        /// <returns></returns>
        public CRUDMethods CRUDMethod { get; set; }

        /// <summary>
        /// TODO
        /// </summary>
        /// <returns></returns>
        public PermissionsPropertyDescriptor Parent { get; set; }

        #endregion

        #region ctors

        /// <summary>
        /// TODO
        /// </summary>
        /// <returns></returns>
        public PermissionsGroupPropertyDefinition(CRUDMethods method)
        {
            this.CRUDMethod = method;
        }

        #endregion

        #region FluentMethods

        public PermissionsPropertyDescriptor And
        {
            get
            {
                return Parent;
            }
        }

        /// <summary>
        /// TODO
        /// </summary>
        /// <returns></returns>
        public PermissionsGroupPropertyDefinition IfInChainOfCommand()
        {
            //TODO
            throw new NotImplementedException();
        }

        /// <summary>
        /// TODO
        /// </summary>
        /// <returns></returns>
        public PermissionsGroupPropertyDefinition WithAtLeastLevel(Authorization.ChainOfCommandLevels level)
        {
            //TODO
            throw new NotImplementedException();
        }

        #endregion

    }
}
