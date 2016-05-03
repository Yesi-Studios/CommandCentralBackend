using System;
using AtwoodUtils;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using CommandCentralHost.Editors;

namespace CommandCentralHost
{
    class Program
    {

        private static readonly List<DialogueOption> dialogueOptions = new List<DialogueOption>
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
                DisplayCriteria = () => ServiceManager.Host != null && ServiceManager.Host.State != CommunicationState.Opened
            },
            new DialogueOption
            {
                OptionText = "Start Service",
                Method = ServiceManager.StartService,
                DisplayCriteria = () => ServiceManager.Host != null && ServiceManager.Host.State != CommunicationState.Opened
            },
            new DialogueOption
            {
                OptionText = "Stop Service",
                Method = ServiceManager.StopService,
                DisplayCriteria = () => ServiceManager.Host != null && ServiceManager.Host.State == CommunicationState.Opened
            },
            new DialogueOption
            {
                OptionText = "Shutdown Application",
                Method = () => Environment.Exit(0),
                DisplayCriteria = () => ServiceManager.Host == null
            },
            new DialogueOption
            {
                OptionText = "Freeze Communicator",
                Method = () => CommandCentral.Communicator.Freeze(),
                DisplayCriteria = () => CommandCentral.Communicator.IsCommunicatorInitialized && !CommandCentral.Communicator.IsFrozen
            },
            new DialogueOption
            {
                OptionText = "Unfreeze Communicator",
                Method = () => CommandCentral.Communicator.Unfreeze(),
                DisplayCriteria = () => CommandCentral.Communicator.IsCommunicatorInitialized && CommandCentral.Communicator.IsFrozen
            },
            new DialogueOption
            {
                OptionText = "Create Schema",
                Method = SchemaEditor.CreateSchema,
                DisplayCriteria = () => ServiceManager.Host == null
            },
            new DialogueOption
            {
                OptionText = "Edit Reference Lists",
                Method = ReferenceListEditor.EditAllReferenceLists,
                DisplayCriteria = () => true
            },
            new DialogueOption
            {
                OptionText = "Edit API Keys",
                Method = ApiKeysEditor.EditAPIKeys,
                DisplayCriteria = () => true
            },
            new DialogueOption
            {
                OptionText = "Edit Commands",
                Method = CommandsEditor.EditCommands,
                DisplayCriteria = () => true
            },
            new DialogueOption
            {
                OptionText = "Edit Permissions",
                Method = PermissionsEditor.PermissionEditorEntry,
                DisplayCriteria = () => true
            }
        };

        [STAThread]
        private static void Main()
        {
            try
            {
                Console.ForegroundColor = ConsoleColor.Green;

                bool keepLooping = true;

                //In order to exit the application a call to Application.Exit is made from one of the dialogue options.
                // ReSharper disable once LoopVariableIsNeverChangedInsideLoop
                // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                while (keepLooping)
                {
                    try
                    {
                        Console.Clear();

                        "Welcome to Command Central's Backend Host Application!".WriteLine();
                        "".WriteLine();
                        //Determine which options to show the client.
                        var displayOptions = dialogueOptions.Where(x => x.DisplayCriteria()).ToList();
                        for (int x = 0; x < displayOptions.Count; x++)
                        {
                            "{0}. {1}".FormatS(x, displayOptions[x].OptionText).WriteLine();
                        }

                        //Get the client's option and then check to make sure it's an integer and that integer is within range.
                        int option;
                        if (int.TryParse(Console.ReadLine(), out option) && option >= 0 && option <= displayOptions.Count - 1)
                        {
                            //Before we call the method, let's clear the console.
                            Console.Clear();

                            //The input was good so just call the method we're supposed to and it'll handle the rest.
                            displayOptions[option].Method();

                            //Regardless of where that method left us, let's give the client some breathing time.
                            "".WriteLine();
                            "Please press any key to continue...".WriteLine();
                            Console.ReadKey();
                        }
                        else
                        {
                            //The input was bad :(
                            Console.Clear();
                            "That input was not valid. Press any key to try again...".WriteLine();
                            Console.ReadKey();
                        }
                    }
                    catch (Exception e)
                    {
                        Console.Clear();
                        "A fatal error occurred in the host application.  Grab your pitchforks!  Find Atwood and RAAAAGE!\n\t{0}".FormatS(e.Message).WriteLine();
                        "".WriteLine();
                        "Press any key to continue...".WriteLine();
                        Console.ReadKey();
                    }
                }
            }
            catch (Exception e)
            {
                "Something terrible has happened that has caused the application to completely fail :(\n\t{0}".FormatS(e.Message).WriteLine();
                "The application will not show down.  Press any key to continue...".WriteLine();
                Console.ReadKey();
            }
        }
    }
}
