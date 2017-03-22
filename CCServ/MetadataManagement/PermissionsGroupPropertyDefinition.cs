using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CCServ.MetadataManagement
{
    class PermissionsGroupPropertyDefinition
    {

        #region Properties

        public CRUDMethods CRUDMethod { get; set; }

        #endregion

        #region ctors

        public PermissionsGroupPropertyDefinition(CRUDMethods method)
        {
            this.CRUDMethod = method;
        }

        #endregion

        #region FluentMethods

        public PermissionsGroupPropertyDefinition IfInChainOfCommand()
        {
        }

        public PermissionsGroupPropertyDefinition IfSelf()
        {
        }

        public PermissionsGroupPropertyDefinition Everyone()
        {
        }

        public PermissionsGroupPropertyDefinition Noone()
        {
        }

        public PermissionsGroupPropertyDefinition WithAtLeastLevel(Authorization.ChainOfCommandLevels level)
        {
        }

        #endregion

    }
}
