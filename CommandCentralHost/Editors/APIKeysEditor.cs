using System;
using System.Collections.Generic;
using System.Linq;
using AtwoodUtils;
using CommandCentral.ClientAccess;
using CommandCentral.DataAccess;

namespace CommandCentralHost.Editors
{
    internal static class ApiKeysEditor
    {
        internal static void EditAPIKeys()
        {
            bool keepLooping = true;

            using (var session = NHibernateHelper.CreateSession())
            using (var transaction = session.BeginTransaction())
            {
                try
                {
                    while (keepLooping)
                    {

                        Console.Clear();
                        "Welcome to the API Keys editor.".WriteLine();
                        "Enter the number of an API Key to edit, the number followed by '-' to delete it, a new application name to create a new Id, or a blank line to cancel.".WriteLine();
                        "".WriteLine();

                        //Let's go get all the API Keys.
                        IList<ApiKey> apiKeys = session.CreateCriteria<ApiKey>().List<ApiKey>();

                        //And then print them out.
                        List<string[]> lines = new List<string[]> { new[] { "#", "ID", "Application Name" } };
                        for (int x = 0; x < apiKeys.Count; x++)
                            lines.Add(new[] { x.ToString(), apiKeys[x].Id.ToString(), apiKeys[x].ApplicationName });
                        DisplayUtilities.PadElementsInLines(lines, 3).WriteLine();

                        int option;
                        string input = Console.ReadLine();

                        if (string.IsNullOrWhiteSpace(input))
                            keepLooping = false;
                        else if (input.Last() == '-' && input.Length > 1 && int.TryParse(input.Substring(0, input.Length - 1), out option) && option >= 0 && option <= apiKeys.Count -1 && apiKeys.Any())
                        {
                            session.Delete(apiKeys[option]);
                        }
                        else if (int.TryParse(input, out option) && option >= 0 && option <= apiKeys.Count - 1 && apiKeys.Any())
                        {
                            //Client wants to edit an item.
                            EditAPIKey(apiKeys[option]);
                        }
                        else
                        {
                            var item = new ApiKey { ApplicationName = input };
                            session.Save(item);
                        }

                    }
                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }

            }
        }

        private static void EditAPIKey(ApiKey key)
        {
            bool keepLooping = true;

            while (keepLooping)
            {
                Console.Clear();

                "Editing API Key for application '{0}'".FormatS(key.ApplicationName).WriteLine();
                "".WriteLine();

                "Application Name:\n\t{0}".FormatS(key.ApplicationName).WriteLine();
                "".WriteLine();
                "API Key:\n\t{0}".FormatS(key.ApplicationName).WriteLine();
                "".WriteLine();

                "1. Edit Applicaion Name".WriteLine();
                "2. Copy API Key to clipboard".WriteLine();
                "3. Return".WriteLine();

                int option;
                if (int.TryParse(Console.ReadLine(), out option))
                {
                    switch (option)
                    {
                        case 1:
                            {
                                Console.Clear();

                                "Enter a new application name...".WriteLine();
                                key.ApplicationName = Console.ReadLine();
                                break;
                            }
                        case 2:
                            {
                                System.Windows.Forms.Clipboard.SetText(key.Id.ToString());
                                break;
                            }
                        case 3:
                            {
                                keepLooping = false;
                                break;
                            }
                        default:
                            {
                                throw new NotImplementedException("In the api key editor switch.");
                            }

                    }
                }
            }
        }
    }
}
