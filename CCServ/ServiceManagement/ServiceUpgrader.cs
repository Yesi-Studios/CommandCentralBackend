using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using AtwoodUtils;
using LibGit2Sharp;
using System.Configuration.Install;
using System.ServiceProcess;

namespace CCServ.ServiceManagement
{
    public static class ServiceUpgrader
    {


        public static void UpgradeService(CLI.Options.UpgradeOptions options)
        {
            UninstallService(options);

            //At this point, we can be sure there is no service installed with the desired name.
            //Now let's shuffle the file system.  We need four folders: Staging, Beta, Production, Old.  THese will all be in a folder called Command Central.
            string rootPath = @"C:/commandcentral";
            string stagingPath = Path.Combine(rootPath, "staging");
            string betaPath = Path.Combine(rootPath, "beta");
            string prodPath = Path.Combine(rootPath, "prod");
            string oldPath = Path.Combine(rootPath, "old_" + string.Format("{0:yyyy-MM-dd_hh-mm-ss-tt}", DateTime.UtcNow));

            //Next, we need to make sure the root path exists.
            if (!Directory.Exists(rootPath))
            {
                Directory.CreateDirectory(rootPath);
                "Created root directory '{0}'".WriteLine(rootPath);
            }

            //Move production into old.
            if (Directory.Exists(prodPath))
            {
                Directory.Move(prodPath, oldPath);
                "Moved {0} -> {1}".WriteLine(prodPath, oldPath);
            }

            //Now we remove staging, prod and beta if they exist.
            if (Directory.Exists(prodPath))
            {
                Utilities.DeleteDirectory(prodPath, true);
                "Removed {0}".WriteLine(prodPath);
            }

            if (Directory.Exists(stagingPath))
            {
                Utilities.DeleteDirectory(stagingPath, true);
                "Removed {0}".WriteLine(stagingPath);
            }

            if (Directory.Exists(betaPath))
            {
                Utilities.DeleteDirectory(betaPath, true);
                "Removed {0}".WriteLine(betaPath);
            }

            //Now we need to download the production branch into staging, and build it into production.
            Directory.CreateDirectory(stagingPath);
            "Created {0}".WriteLine(stagingPath);

            "Beginning clone from repository {0}".WriteLine(options.GitURL);
            var co = new CloneOptions();
            co.CredentialsProvider = (_url, _user, _cred) => new UsernamePasswordCredentials { Username = options.GitUsername, Password = options.GitPassword };
            Repository.Clone(options.GitURL, stagingPath, co);

            //We should now have the repo in the staging directory.
            //Let's make sure of that.
            if (!Repository.IsValid(stagingPath))
                throw new Exception("An error occurred while cloning the repository!");

            "Clone completed.".WriteLine();

            //Now let's ensure that the branches we need actually exist.
            using (var repo = new Repository(stagingPath))
            {
                foreach (var branch in repo.Branches)
                {
                    branch.FriendlyName.WriteLine();
                }
            }
        }

        private static void UninstallService(CLI.Options.UpgradeOptions options)
        {
            //Select the service.
            //We do it like this to allow for case insensitivity in the service name.
            var service = System.ServiceProcess.ServiceController.GetServices().FirstOrDefault(x => x.ServiceName.Equals(options.ServiceName, StringComparison.CurrentCultureIgnoreCase));

            //If there is a service with the desired service name.
            if (service != null)
            {
                var liveService = new System.ServiceProcess.ServiceController(options.ServiceName);

                //If the live service is running, we need to shut it down.
                if (liveService.Status == ServiceControllerStatus.Running)
                {
                    "A service named '{0}' is currently running, do you wish to stop it? (y/n)".WriteLine(options.ServiceName);
                    string stopServiceOption = Console.ReadLine();

                    switch (stopServiceOption.ToLower())
                    {
                        case "y":
                            {
                                liveService.Stop();
                                liveService.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromSeconds(30));

                                break;
                            }
                        case "n":
                            {
                                "In order to upgrade the service you must either choose a different service name or agree to shut down the currently running service.".WriteLine();
                                return;
                            }
                        default:
                            {
                                "Your input, '{0}', was invalid.".WriteLine(stopServiceOption);
                                return;
                            }
                    }

                    //Let's just confirm that the service actually stopped.
                    if (liveService.Status != ServiceControllerStatus.Stopped)
                        throw new Exception("An error occurred!  The live service failed to stop.");
                }

                //At this point, we can be sure the service has shut down.
                //Now we need to uninstall the service.
                ServiceInstaller installer = new ServiceInstaller();
                InstallContext context = new InstallContext();
                installer.Context = context;
                installer.ServiceName = options.ServiceName;
                installer.Uninstall(null);

                //Now, let's confirm that the service is actually gone.
                if (ServiceController.GetServices().Any(x => x.ServiceName.Equals(options.ServiceName, StringComparison.CurrentCultureIgnoreCase)))
                    throw new Exception("An error occurred!  We failed to uninstall the service.");
            }
        }
    }
}
