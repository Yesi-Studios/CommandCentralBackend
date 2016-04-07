using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnifiedServiceFramework.Framework;
using AtwoodUtils;

namespace CDBServiceHost.Interfaces
{
    public static class ErrorsEditor
    {
        public static void EditErrors(List<Errors.Error> errors)
        {
            try
            {
                bool keepLooping = true;

                while (keepLooping)
                {
                    Console.Clear();
                    Console.WriteLine("Choose a number to view the error's details or enter an empty line to cancel...");
                    Console.WriteLine();

                    List<string[]> lines = new List<string[]>();
                    lines.Add(new[] { "#", "Time", "Message" });
                    for (int x = 0; x < errors.Count; x++)
                    {
                        lines.Add(new[] { x.ToString(), errors[x].Time.ToString(), errors[x].Message.Truncate(20) });
                    }
                    Console.WriteLine(Interfaces.GenericInterfaces.PadElementsInLines(lines, 3));

                    int option = -1;
                    string optionString = Console.ReadLine();

                    if (string.IsNullOrWhiteSpace(optionString))
                    {
                        keepLooping = false;
                    }
                    else
                        if (Int32.TryParse(optionString, out option) && option >= 0 && option <= errors.Count - 1)
                        {
                            EditError(errors[option]);
                        }

                }
            }
            catch
            {
                throw;
            }
        }

        public static void EditError(UnifiedServiceFramework.Framework.Errors.Error error)
        {
            try
            {
                bool keepLooping = true;

                while (keepLooping)
                {
                    Console.Clear();

                    Console.WriteLine(string.Format("Time: \t{0}", error.Time.ToString()));
                    Console.WriteLine(string.Format("Logged In User: \t{0}", error.LoggedInUserID));
                    Console.WriteLine(string.Format("Handled: \t{0}", error.IsHandled));
                    Console.WriteLine(string.Format("Message: \t{0}", error.Message));
                    Console.WriteLine(string.Format("Inner Exception: \t{0}", error.InnerException));
                    Console.WriteLine(string.Format("Stack Trace: \t{0}", error.StackTrace.Truncate(100)));
                    Console.WriteLine();

                    Console.WriteLine("1. View Full Stack Trace");
                    Console.WriteLine("2. Switch Handled State and Save");
                    Console.WriteLine("3. Cancel");

                    int option = 0;

                    if (Int32.TryParse(Console.ReadLine(), out option) && option >= 1 && option <= 3)
                    {
                        switch (option)
                        {
                            case 1:
                                {
                                    Console.Clear();
                                    Console.WriteLine(error.StackTrace);
                                    Console.WriteLine();
                                    Console.WriteLine("Press any key to return...");
                                    Console.ReadKey();
                                    break;
                                }
                            case 2:
                                {
                                    Console.Clear();
                                    error.IsHandled = !error.IsHandled;
                                    Console.WriteLine("Updating...");
                                    error.DBUpdate().Wait();
                                    Console.WriteLine("Success!");
                                    break;
                                }
                            case 3:
                                {
                                    keepLooping = false;
                                    break;
                                }
                            default:
                                {
                                    throw new NotImplementedException("In Edit Error");

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
