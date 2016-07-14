using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AtwoodUtils;

namespace CommandCentral.Config
{
    /// <summary>
    /// Encapsulates methods for determining the current version.
    /// </summary>
    public static class Version
    {
        private const string RELEASE_VERSION = "0.9.2.0";

        /// <summary>
        /// Gets the current version of the application.
        /// </summary>
        /// <returns></returns>
        public static string GetVersion()
        {
            bool isDebug = System.Diagnostics.Debugger.IsAttached;

            if (System.Deployment.Application.ApplicationDeployment.IsNetworkDeployed)
            {
                return RELEASE_VERSION;
            }
            else
            {
                string repoPath = LibGit2Sharp.Repository.Discover(System.IO.Directory.GetCurrentDirectory());

                try
                {
                    using (var repo = new LibGit2Sharp.Repository(repoPath))
                    {
                        var currentBranch = repo.Head;
                        return "{0} @ {1}".FormatS(currentBranch.FriendlyName, String.Concat(currentBranch.Tip.Id.ToString().Take(7)));
                    }
                }
                catch
                {
                    return "Unspecified Repo - Debug Mode";
                }
            }
        }
    }
}
