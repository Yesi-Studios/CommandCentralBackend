using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CDBServiceHost.Interfaces
{
    public static class EndpointsEditor
    {
        public static void ManageEndpoint(KeyValuePair<string, UnifiedServiceFramework.Framework.EndpointDescription> endpoint)
        {
            try
            {
                bool keepLooping = true;

                while (keepLooping)
                {
                    Console.Clear();

                    Console.WriteLine(string.Format("Managing endpoint '{0}'...", endpoint.Key));
                    Console.WriteLine();
                    Console.WriteLine("Description:");
                    Console.WriteLine(string.Format("\t{0}", endpoint.Value.Description));
                    Console.WriteLine();
                    Console.WriteLine("Required Parameters:");
                    endpoint.Value.RequiredParameters.ForEach(x =>
                        {
                            Console.WriteLine(string.Format("\t{0}", x));
                        });
                    Console.WriteLine();
                    
                    Console.WriteLine(string.Format("Allows Argument Logging: \n\t{0}", endpoint.Value.AllowArgumentLogging.ToString()));
                    Console.WriteLine(string.Format("Allows Response Logging: \n\t{0}", endpoint.Value.AllowResponseLogging.ToString()));
                    Console.WriteLine(string.Format("Requires Authentication: \n\t{0}", endpoint.Value.RequiresAuthentication.ToString()));
                    Console.WriteLine(string.Format("Is Active: \n\t{0}", endpoint.Value.IsActive.ToString()));
                    Console.WriteLine();
                    Console.WriteLine(string.Format("1. {0} Endpoint", endpoint.Value.IsActive ? "Disable" : "Enable"));
                    Console.WriteLine("2. Cancel");

                    int option;
                    if (Int32.TryParse(Console.ReadLine(), out option) && option >= 1 && option <= 2)
                    {
                        switch (option)
                        {
                            case 1:
                                {
                                    UnifiedServiceFramework.Framework.ServiceManager.EndpointDescriptions[endpoint.Key].IsActive = !UnifiedServiceFramework.Framework.ServiceManager.EndpointDescriptions[endpoint.Key].IsActive;
                                    break;
                                }
                            case 2:
                                {
                                    keepLooping = false;
                                    break;
                                }
                            default:
                                {
                                    throw new NotImplementedException("In the manage endpoint method.");
                                }
                        }
                    }
                    

                }

            }
            catch
            {
                throw;
            }
        }
    }
}
