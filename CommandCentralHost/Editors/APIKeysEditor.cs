using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AtwoodUtils;
using CommandCentral.ClientAccess;
using CommandCentral.DataAccess;
using NHibernate;

namespace CommandCentralHost.Editors
{
    internal class ApiKeysEditor
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

                        string input = Console.ReadLine();

                        if (string.IsNullOrWhiteSpace(input))
                            keepLooping = false;
                        else
                        {
                            int option;
                            if (int.TryParse(input, out option) && option >= 0 && option <= apiKeys.Count - 1)
                            {

                            }
                            else
                            {
                                
                            }
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
    }
}
