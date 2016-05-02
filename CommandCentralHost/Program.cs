using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AtwoodUtils;

namespace CommandCentralHost
{
    class Program
    {

        private static List<DialogueOption> DialogueOptions = new List<DialogueOption>
        {
            new DialogueOption
            {
                OptionText = "Initialize Service",
                Method = ServiceManager.InitializeService,
                DisplayCriteria = () => ServiceManager.Host == null
            },
            new DialogueOption
            {
                OptionText = "Release Service",
                Method = ServiceManager.ReleaseService,
                DisplayCriteria = () => ServiceManager.Host != null && ServiceManager.Host.State == System.ServiceModel.CommunicationState.Closed
            },
            new DialogueOption
            {
                OptionText = "Start Service",
                Method = ServiceManager.StartService,
                DisplayCriteria = () => ServiceManager.Host != null && ServiceManager.Host.State != System.ServiceModel.CommunicationState.Opened
            },
            new DialogueOption
            {
                OptionText = "Stop Service",
                Method = ServiceManager.StopService,
                DisplayCriteria = () => ServiceManager.Host != null && ServiceManager.Host.State == System.ServiceModel.CommunicationState.Opened
            },
            new DialogueOption
            {
                OptionText = "Shutdown Application",
                Method = () => Environment.Exit(0),
                DisplayCriteria = () => ServiceManager.Host == null
            },
            new DialogueOption
            {
                OptionText = "Edit Reference Lists",
                Method = ReferenceListEditor.EditAllReferenceLists,
                DisplayCriteria = () => true
            }
        };


        static void Main(string[] args)
        {
            try
            {
                Console.ForegroundColor = ConsoleColor.Green;

                bool keepLooping = true;

                while (keepLooping)
                {
                    try
                    {
                        Console.Clear();

                        "Welcome to Command Central's Backend Host Application!".WL();
                        "".WL();
                        //Determine which options to show the client.
                        var displayOptions = DialogueOptions.Where(x => x.DisplayCriteria()).ToList();
                        for (int x = 0; x < displayOptions.Count; x++)
                        {
                            "{0}. {1}".F(x, displayOptions[x].OptionText).WL();
                        }

                        //Get the client's option and then check to make sure it's an integer and that integer is within range.
                        int option;
                        if (Int32.TryParse(Console.ReadLine(), out option) && option >= 0 && option <= displayOptions.Count - 1)
                        {
                            //Before we call the method, let's clear the console.
                            Console.Clear();

                            //The input was good so just call the method we're supposed to and it'll handle the rest.
                            displayOptions[option].Method();

                            //Regardless of where that method left us, let's give the client some breathing time.
                            "".WL();
                            "Please press any key to continue...".WL();
                            Console.ReadKey();
                        }
                        else
                        {
                            //The input was bad :(
                            Console.Clear();
                            "That input was not valid. Press any key to try again...".WL();
                            Console.ReadKey();
                            continue;
                        }
                    }
                    catch (Exception e)
                    {
                        Console.Clear();
                        "A fatal error occurred in the host application.  Grab your pitchforks!  Find Atwood and RAAAAGE!\n\t{0}".F(e.Message).WL();
                        "".WL();
                        "Press any key to continue...".WL();
                        Console.ReadKey();
                    }
                }
            }
            catch (Exception e)
            {
                "Something terrible has happened that has caused the application to completely fail :(\n\t{0}".F(e.Message).WL();
                "The application will not show down.  Press any key to continue...".WL();
                Console.ReadKey();
            }
        }
    }
}
