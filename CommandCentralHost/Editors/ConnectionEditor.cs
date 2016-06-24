using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AtwoodUtils;

namespace CommandCentralHost.Editors
{
    public static class ConnectionEditor
    {
        public static void EditConnection()
        {
            bool keepLooping = true;

            while (keepLooping)
            {
                Console.Clear();

                "Current Settings Key: {0}".FormatS(CommandCentral.DataAccess.ConnectionSettings.CurrentSettingsKey).WriteLine();

                "".WriteLine();

                "1. Change Settings".WriteLine();
                "2. Cancel".WriteLine();

                int option;
                if (Int32.TryParse(Console.ReadLine(), out option) && option > 0 && option <= 2)
                {
                    switch (option)
                    {
                        case 1:
                            {
                                Console.Clear();

                                for (int x = 0; x < CommandCentral.DataAccess.ConnectionSettings.PredefinedConnectionSettings.Count; x++)
                                {
                                    "{0}. {1}\n\tUsername: {2}\n\tPassword: {3}\n\tDatabase: {4}\n\tServer: {5}".FormatS(x, 
                                        CommandCentral.DataAccess.ConnectionSettings.PredefinedConnectionSettings.ElementAt(x).Key,
                                        CommandCentral.DataAccess.ConnectionSettings.PredefinedConnectionSettings.ElementAt(x).Value.Username,
                                        CommandCentral.DataAccess.ConnectionSettings.PredefinedConnectionSettings.ElementAt(x).Value.Password,
                                        CommandCentral.DataAccess.ConnectionSettings.PredefinedConnectionSettings.ElementAt(x).Value.Database,
                                        CommandCentral.DataAccess.ConnectionSettings.PredefinedConnectionSettings.ElementAt(x).Value.Server).WriteLine();
                                }

                                "".WriteLine();
                                "Select a settings number...".WriteLine();

                                int settingsOption;
                                if (Int32.TryParse(Console.ReadLine(), out settingsOption) && settingsOption >= 0 && settingsOption <= CommandCentral.DataAccess.ConnectionSettings.PredefinedConnectionSettings.Count - 1)
                                {
                                    CommandCentral.DataAccess.ConnectionSettings.CurrentSettingsKey = CommandCentral.DataAccess.ConnectionSettings.PredefinedConnectionSettings.ElementAt(settingsOption).Key;

                                    CommandCentral.DataAccess.NHibernateHelper.InitializeNHibernate(
                                        CommandCentral.DataAccess.ConnectionSettings.PredefinedConnectionSettings[CommandCentral.DataAccess.ConnectionSettings.CurrentSettingsKey]);

                                    "Settings have been reset and the session factory reconstructed.  Press any key to continue...".WriteLine();
                                    Console.ReadKey();
                                }
                                else
                                {
                                    "You suck.  Press any key to continue...".WriteLine();
                                    Console.ReadKey();
                                }

                                break;
                            }
                        case 2:
                            {
                                keepLooping = false;
                                break;
                            }
                        default:
                            {
                                throw new NotImplementedException("An option had no rule in the edit connection interface.  Option: {0}".FormatS(option));
                            }
                    }
                }
            }

            
        }
    }
}
