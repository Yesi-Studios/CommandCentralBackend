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
using System.Net;
using Microsoft.Build.Construction;
using Microsoft.Build.Evaluation;
using Microsoft.Build.Execution;
using Microsoft.Build.Logging;

namespace CCServ.ServiceManagement
{
    public static class ServiceUpgrader
    {


        public static void UpgradeService(CLI.Options.UpgradeOptions options)
        {
            string prodServiceName = options.BaseServiceName + "_prod";
            string betaServiceName = options.BaseServiceName + "_beta";

            UninstallService(prodServiceName);
            UninstallService(betaServiceName);

            //At this point, we can be sure there is no service installed with the desired name.
            //Now let's shuffle the file system.  We need four folders: Source, Beta, Production, Old.  These will all be in a folder called Command Central.
            string rootPath = @"C:/commandcentral";
            string sourcePath = Path.Combine(rootPath, "source");
            string betaPath = Path.Combine(rootPath, "beta");
            string prodPath = Path.Combine(rootPath, "prod");
            string oldPath = Path.Combine(rootPath, "old_" + string.Format("{0:yyyy-MM-dd_hh-mm-ss-fff}", DateTime.UtcNow));

            PrepareDirectories(rootPath, sourcePath, prodPath, betaPath, oldPath);

            Tuple<Branch, Branch> branches = BuildBranches(options, sourcePath, prodPath, betaPath);
            var prodBranch = branches.Item1;
            var betaBranch = branches.Item2;

            InstallService(Path.Combine(prodPath, AppDomain.CurrentDomain.FriendlyName), prodServiceName, "[Production] Command Central Backend Service on commit '{0}' by '{1}'.".FormatS(prodBranch.Tip.Id, prodBranch.Tip.Author));
            InstallService(Path.Combine(betaPath, AppDomain.CurrentDomain.FriendlyName), betaServiceName, "[Beta] Command Central Backend Service on commit '{0}' by '{1}'.".FormatS(betaBranch.Tip.Id, betaBranch.Tip.Author));

            //Ok, services are installed!



        }

        

        private static void LaunchServices(CLI.Options.UpgradeOptions options, string prodPath, string betaPath)
        {
            //First, let's make sure that we have reservations for the required ports.
            ReserveURL(options.BetaPort, options.SecurityMode == CLI.SecurityModes.Both || options.SecurityMode == CLI.SecurityModes.HTTPSOnly);
            ReserveURL(options.ProdPort, options.SecurityMode == CLI.SecurityModes.Both || options.SecurityMode == CLI.SecurityModes.HTTPSOnly);

            //TODO: Find out how to check that we actually reserved those ports.
            
            //Now, we want to install both services.
        }

        private static void ReserveURL(int port, bool useHTTPS)
        {
            System.Diagnostics.Process process = new System.Diagnostics.Process();
            System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
            startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Normal;
            startInfo.FileName = "netsh";
            startInfo.Arguments = @"http add urlacl url=http{0}://+:{1}/ user=Everyone".FormatS(useHTTPS ? "s" : "", port);
            startInfo.UseShellExecute = false;
            startInfo.RedirectStandardOutput = true;
            startInfo.RedirectStandardError = true;
            process.StartInfo = startInfo;
            process.OutputDataReceived += (sender, args) => { if (args.Data != null) args.Data.WriteLine(); };
            process.ErrorDataReceived += (sender, args) => { if (args.Data != null) args.Data.WriteLine(); };

            "Executing: {0} {1}".WriteLine(startInfo.FileName, startInfo.Arguments);

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            process.WaitForExit();
        }

        private static void PrepareDirectories(string rootPath, string sourcePath, string prodPath, string betaPath, string oldPath)
        {
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

            //Now we remove source, prod and beta if they exist.
            if (Directory.Exists(prodPath))
            {
                Utilities.DeleteDirectory(prodPath, true);
                "Removed {0}".WriteLine(prodPath);
            }

            if (Directory.Exists(sourcePath))
            {
                Utilities.DeleteDirectory(sourcePath, true);
                "Removed {0}".WriteLine(sourcePath);
            }

            if (Directory.Exists(betaPath))
            {
                Utilities.DeleteDirectory(betaPath, true);
                "Removed {0}".WriteLine(betaPath);
            }

            //Now we need to download the production branch into source, and build it into production.
            Directory.CreateDirectory(sourcePath);
            "Created {0}".WriteLine(sourcePath);
        }

        private static Tuple<Branch, Branch> BuildBranches(CLI.Options.UpgradeOptions options, string sourcePath, string prodPath, string betaPath)
        {
            "Beginning clone from repository {0}".WriteLine(options.GitURL);
            var co = new CloneOptions();
            co.CredentialsProvider = (_url, _user, _cred) => new UsernamePasswordCredentials { Username = options.GitUsername, Password = options.GitPassword };
            Repository.Clone(options.GitURL, sourcePath, co);

            //We should now have the repo in the source directory.
            //Let's make sure of that.
            if (!Repository.IsValid(sourcePath))
                throw new Exception("An error occurred while cloning the repository!");

            "Clone completed.".WriteLine();

            //Now let's ensure that the branches we need actually exist.
            using (var repo = new Repository(sourcePath))
            {
                var prodBranch = repo.Branches.FirstOrDefault(x => x.FriendlyName == @"origin/" + options.ProductionBranchName);
                var betaBranch = repo.Branches.FirstOrDefault(x => x.FriendlyName == @"origin/" + options.BetaBranchName);

                if (prodBranch == null)
                {
                    throw new Exception("No branch was found for '{0}'".FormatS(@"origin/" + options.ProductionBranchName));
                }

                if (betaBranch == null)
                {
                    throw new Exception("No branch was found for '{0}'".FormatS(@"origin/" + options.BetaBranchName));
                }

                //Cool, so we know that we have the two branches that we need.
                //Now we need to build them into their respective directories.
                Commands.Checkout(repo, prodBranch);

                //Now let's build the project.
                //First we need nuget as well!

                string pathToNuget = Path.Combine(sourcePath, "nuget.exe");
                WebClient client = new WebClient();
                client.DownloadFile(new Uri(options.NugetURL), pathToNuget);

                //Let's make sure that we got it!
                if (!File.Exists(pathToNuget))
                    throw new Exception("An error occurred while acquiring nuget!");

                RestoreNugetPackages(sourcePath, pathToNuget);

                BuildSolution(sourcePath, prodPath);

                //Once the solution builds, let's also make a version comment file.
                File.WriteAllText(Path.Combine(prodPath, "version"), "{0} by {1}\n-----------\n{2}".FormatS(prodBranch.Tip.Id, prodBranch.Tip.Author, prodBranch.Tip.Message));

                //Now for beta
                Commands.Checkout(repo, betaBranch);
                RestoreNugetPackages(sourcePath, pathToNuget);
                BuildSolution(sourcePath, betaPath);

                File.WriteAllText(Path.Combine(betaPath, "version"), "{0} by {1}\n-----------\n{2}".FormatS(betaBranch.Tip.Id, betaBranch.Tip.Author, betaBranch.Tip.Message));

                return new Tuple<Branch, Branch>(prodBranch, betaBranch);
            }
        }

        private static void RestoreNugetPackages(string rootpath, string pathToNuget)
        {
            //These are all the directories that need to have packages rebuilt.
            var dirs = Directory.GetDirectories(rootpath).Where(x => Directory.GetFiles(x).Any(y => y == Path.Combine(x, "packages.config")));

            foreach (var dir in dirs)
            {
                System.Diagnostics.Process process = new System.Diagnostics.Process();
                System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
                startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Normal;
                startInfo.FileName = "cmd.exe";
                startInfo.Arguments = @"/c {0} install {1}\packages.config -o {2}".FormatS(pathToNuget, dir, Path.Combine(rootpath, "packages"));
                startInfo.UseShellExecute = false;
                startInfo.RedirectStandardOutput = true;
                startInfo.RedirectStandardError = true;
                process.StartInfo = startInfo;
                process.OutputDataReceived += (sender, args) => { if (args.Data != null) args.Data.WriteLine(); };
                process.ErrorDataReceived += (sender, args) => { if (args.Data != null) args.Data.WriteLine(); };

                "Executing: {0} {1}".WriteLine(startInfo.FileName, startInfo.Arguments);

                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                process.WaitForExit();
            }
        }

        private static void BuildSolution(string rootPath, string outputPath)
        {
            //First let's see if we have a .sln in here.
            var slnFileName = Directory.GetFiles(rootPath).SingleOrDefault(x => Path.GetExtension(x) == ".sln");

            if (slnFileName == null)
                throw new Exception("The directory contains no .sln file.");

            ProjectCollection pc = new ProjectCollection();
            Dictionary<string, string> properties = new Dictionary<string, string>();
            properties.Add("Configuration", "Release");
            properties.Add("Platform", "Any CPU");
            properties.Add("OutputPath", outputPath);

            BuildRequestData buildRequest = new BuildRequestData(slnFileName, properties, null, new string[] { "Build" }, null);

            BuildResult buildResult = BuildManager.DefaultBuildManager.Build(new BuildParameters(pc), buildRequest);
        }

        private static void InstallService(string assemblyPath, string serviceName, string description)
        {
            ServiceProcessInstaller ProcesServiceInstaller = new ServiceProcessInstaller();
            ProcesServiceInstaller.Account = ServiceAccount.NetworkService;

            ServiceInstaller ServiceInstallerObj = new ServiceInstaller();
            InstallContext Context = new InstallContext();
            String path = String.Format("/assemblypath={0}", assemblyPath);
            String[] cmdline = { path };

            Context = new InstallContext("", cmdline);
            ServiceInstallerObj.Context = Context;
            ServiceInstallerObj.DisplayName = serviceName;
            ServiceInstallerObj.Description = description;
            ServiceInstallerObj.ServiceName = serviceName;
            ServiceInstallerObj.StartType = ServiceStartMode.Automatic;
            ServiceInstallerObj.Parent = ProcesServiceInstaller;

            System.Collections.Specialized.ListDictionary state = new System.Collections.Specialized.ListDictionary();
            ServiceInstallerObj.Install(state);
        }

        private static void UninstallService(string serviceName)
        {
            //Select the service.
            //We do it like this to allow for case insensitivity in the service name.
            var service = ServiceController.GetServices().FirstOrDefault(x => x.ServiceName.Equals(serviceName, StringComparison.CurrentCultureIgnoreCase));

            //If there is a service with the desired service name.
            if (service != null)
            {
                var liveService = new ServiceController(serviceName);

                //If the live service is running, we need to shut it down.
                if (liveService.Status == ServiceControllerStatus.Running)
                {
                    "A service named '{0}' is currently running, do you wish to stop it? (y/n)".WriteLine(serviceName);
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
                installer.ServiceName = serviceName;
                installer.Uninstall(null);

                //Now, let's confirm that the service is actually gone.
                if (ServiceController.GetServices().Any(x => x.ServiceName.Equals(serviceName, StringComparison.CurrentCultureIgnoreCase)))
                    throw new Exception("An error occurred!  We failed to uninstall the service.");
            }
        }

    }
}
