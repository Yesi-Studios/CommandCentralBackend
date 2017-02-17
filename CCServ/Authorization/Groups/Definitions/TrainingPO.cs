using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CCServ.Authorization.Groups.Definitions
{
    /// <summary>
    /// Any other training POs at the command who can do training events.
    /// </summary>
    public class TrainingPO : PermissionGroup
    {
        /// <summary>
        /// Any other training POs at the command who can do training events.
        /// </summary>
        public TrainingPO()
        {
            CanAccessSubModules(SubModules.ManageTrainingEvents);

            CanEditMembershipOf(typeof(TrainingCoordinator));

            HasAccessLevel(ChainOfCommandLevels.Command);

            InChainsOfCommand(ChainsOfCommand.Training);

            CanAccessModule("Training");
        }
    }
}
