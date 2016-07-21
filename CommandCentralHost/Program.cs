using System;
using AtwoodUtils;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using CommandCentralHost.Editors;
using System.Runtime.InteropServices;

namespace CommandCentralHost
{
    /// <summary>
    /// Main Class
    /// </summary>
    class Program
    {

        private static readonly List<DialogueOption> _dialogueOptions = new List<DialogueOption>
        {
            new DialogueOption
            {
                OptionText = "Launch Service",
                Method = ServiceManager.LaunchService,
                DisplayCriteria = () => ServiceManager.Host == null
            },
            new DialogueOption
            {
                OptionText = "Shutdown Service",
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
                OptionText = "Manage Reference Lists",
                Method = ReferenceListEditor.EditAllReferenceLists,
                DisplayCriteria = () => true
            },
            new DialogueOption
            {
                OptionText = "Manage API Keys",
                Method = ApiKeysEditor.EditAPIKeys,
                DisplayCriteria = () => true
            },
            new DialogueOption
            {
                OptionText = "Manage Commands",
                Method = CommandsEditor.EditCommands,
                DisplayCriteria = () => true
            },
            new DialogueOption
            {
                OptionText = "Manage Permissions",
                Method = PermissionsEditor.PermissionEditorEntry,
                DisplayCriteria = () => true
            },
            new DialogueOption
            {
                OptionText = "Manage Endpoints",
                Method = EndpointEditor.EditEndpoints,
                DisplayCriteria = () => true
            },
            new DialogueOption
            {
                OptionText = "View Entity Metadata",
                Method = MetadataViewer.ViewAllEntityMetadata,
                DisplayCriteria = () => true
            },
            new DialogueOption
            {
                OptionText = "Manage Database Connection Settings",
                Method = ConnectionEditor.EditConnection,
                DisplayCriteria = () => true
            },
            new DialogueOption
            {
                OptionText = "Create the McLean",
                Method = PersonsEditor.CreateMcLean,
                DisplayCriteria = () => true
            },
            new DialogueOption
            {
                OptionText = "Create the Atwood",
                Method = PersonsEditor.CreateAtwood,
                DisplayCriteria = () => true
            },
            new DialogueOption
            {
                OptionText = "Finalize Muster",
                Method = () =>
                    {
                        CommandCentral.Entities.Muster.MusterRecord.FinalizeMuster(null);
                    },
                DisplayCriteria = () => true
            },
            new DialogueOption
            {
                OptionText = "Rollover Muster",
                Method = () =>
                    {
                        CommandCentral.Entities.Muster.MusterRecord.RolloverMuster();
                    },
                DisplayCriteria = () => true
            }


        };

        private const int SWP_NOSIZE = 0x0001;


        [DllImport("kernel32.dll", ExactSpelling = true)]
        private static extern IntPtr GetConsoleWindow();

        private static readonly IntPtr _myConsole = GetConsoleWindow();

        [DllImport("user32.dll", EntryPoint = "SetWindowPos")]
        private static extern IntPtr SetWindowPos(IntPtr hWnd, int hWndInsertAfter, int x, int y, int cx, int cy, int wFlags);



        [STAThread]
        private static void Main()
        {

            try
            {
                string version = CommandCentral.Config.Version.RELEASE_VERSION;

                Console.Title = "Command Central Backend Service | version {0}".FormatS(version);

                Console.WindowWidth = 200;
                Console.WindowHeight = Console.LargestWindowHeight;
                SetWindowPos(_myConsole, 0, 0, 0, 0, 0, SWP_NOSIZE);

                Console.ForegroundColor = ConsoleColor.Green;

                //In order to exit the application a call to Application.Exit is made from one of the dialogue options.
                // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                while (true)
                {
                    try
                    {

                        Console.Clear();

                        "Command Central Backend Service | version {0}".FormatS(version).WriteLine();
                        "".WriteLine();
                        //Determine which options to show the client.
                        var displayOptions = _dialogueOptions.Where(x => x.DisplayCriteria()).ToList();
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
                            Console.Clear();
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
