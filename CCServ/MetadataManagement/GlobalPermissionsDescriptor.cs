using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CCServ.Authorization;

namespace CCServ.MetadataManagement
{
    public class GlobalPermissionsDescriptor
    {

        #region Properties

        public CRUDMethods Method { get; private set; }

        public List<ChainOfCommand> RequiredChainsOfCommand { get; private set; }

        public ChainOfCommandLevels RequiredLevel { get; private set; }

        public bool IsImpossible { get; private set; }

        #endregion

        #region ctors

        public GlobalPermissionsDescriptor()
        {
        }

        #endregion

        #region Fluent Methods

        public GlobalPermissionsDescriptor Create
        {
            get
            {
                Method = CRUDMethods.Create;
                return this;
            }
        }

        public GlobalPermissionsDescriptor Delete
        {
            get
            {
                Method = CRUDMethods.Delete;
                return this;
            }
        }

        public GlobalPermissionsDescriptor MustBeIn(params ChainOfCommand[] chains)
        {
            foreach (var chain in chains)
            {
                RequiredChainsOfCommand.Add(chain);
            }
            return this;
        }

        public GlobalPermissionsDescriptor AtLevel(ChainOfCommandLevels level)
        {
            RequiredLevel = level;
            return this;
        }

        public GlobalPermissionsDescriptor Impossible()
        {
            IsImpossible = true;
            return this;
        }

        #endregion


    }
}
