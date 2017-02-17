using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CCServ.Authorization.Groups.Definitions
{
    /// <summary>
    /// The command level training coordinators.
    /// </summary>
    public class TrainingCoordinator : PermissionGroup
    {
        /// <summary>
        /// The command level training coordinators.
        /// </summary>
        public TrainingCoordinator()
        {
            CanAccessSubModules(SubModules.TrainingAdmin, SubModules.ManageTrainingEvents);

            CanEditMembershipOf(typeof(TrainingCoordinator));

            HasAccessLevel(ChainOfCommandLevels.Command);

            InChainsOfCommand(ChainsOfCommand.Training);

            CanAccessModule("Training");
        }
    }
}
