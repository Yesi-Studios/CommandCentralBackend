using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CCServ.Authorization;

namespace CCServ.MetadataManagement.Permissions
{
    /// <summary>
    /// TODO
    /// </summary>
    /// <returns></returns>
    public class GlobalPermissionsDescriptor
    {

        #region Properties

        /// <summary>
        /// The type of crud operation that this permissions descriptor applies to.
        /// </summary>
        /// <returns></returns>
        public CRUDMethods Method { get; private set; }

        /// <summary>
        /// The chains of command that are allowed to take this action.
        /// </summary>
        /// <returns></returns>
        public List<ChainOfCommand> RequiredChainsOfCommand { get; private set; }

        /// <summary>
        /// The required level within the afforementioned chains of command in order to take this action.
        /// </summary>
        /// <returns></returns>
        public ChainOfCommandLevels RequiredLevel { get; private set; }

        /// <summary>
        /// Indicates that this operation is impossible.  Overrides all other rules.
        /// </summary>
        /// <returns></returns>
        public bool IsImpossible { get; private set; }

        #endregion

        #region ctors

        /// <summary>
        /// Creates a new global permissions description.
        /// </summary>
        /// <returns></returns>
        public GlobalPermissionsDescriptor()
        {
            RequiredChainsOfCommand = new List<ChainOfCommand>();
        }

        #endregion

        #region Fluent Methods

        /// <summary>
        /// Sets the method to Create.
        /// </summary>
        /// <returns></returns>
        public GlobalPermissionsDescriptor Create
        {
            get
            {
                Method = CRUDMethods.Create;
                return this;
            }
        }

        /// <summary>
        /// Sets the method to delete.
        /// </summary>
        /// <returns></returns>
        public GlobalPermissionsDescriptor Delete
        {
            get
            {
                Method = CRUDMethods.Delete;
                return this;
            }
        }

        /// <summary>
        /// Sets the required chains of command.
        /// </summary>
        /// <returns></returns>
        public GlobalPermissionsDescriptor MustBeIn(params ChainOfCommand[] chains)
        {
            foreach (var chain in chains)
            {
                RequiredChainsOfCommand.Add(chain);
            }
            return this;
        }

        /// <summary>
        /// Sets the required level.
        /// </summary>
        /// <returns></returns>
        public GlobalPermissionsDescriptor AtLevel(ChainOfCommandLevels level)
        {
            RequiredLevel = level;
            return this;
        }

        /// <summary>
        /// Sets the impossible flag to true.
        /// </summary>
        /// <returns></returns>
        public GlobalPermissionsDescriptor Impossible()
        {
            IsImpossible = true;
            return this;
        }

        #endregion


    }
}
