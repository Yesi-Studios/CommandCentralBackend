using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AtwoodUtils;
using CommandCentral.Entities;
using CommandCentral.DataAccess;

namespace CommandCentralHost.Editors
{
    public static class VersionEditor
    {

        public static void EditVersions()
        {
            bool keepLooping = true;

            using (var session = NHibernateHelper.CreateStatefulSession())
            using (var transaction = session.BeginTransaction())
            {
                try
                {
                    while (keepLooping)
                    {

                        Console.Clear();
                        "Welcome to the versions editor.".WriteLine();
                        "Enter a new version to create it, or a blank line to cancel.".WriteLine();
                        "".WriteLine();

                        //Let's go get all the API Keys.
                        List<VersionInformation> versions = session.CreateCriteria<VersionInformation>().List<VersionInformation>().OrderBy(x => x.Time).ToList();

                        //And then print them out.
                        List<string[]> lines = new List<string[]> { new[] { "Version", "Time" } };
                        for (int x = 0; x < versions.Count; x++)
                            lines.Add(new[] { versions[x].Version, versions[x].Time.ToString() });
                        DisplayUtilities.PadElementsInLines(lines, 3).WriteLine();

                        string input = Console.ReadLine();

                        if (string.IsNullOrWhiteSpace(input))
                            keepLooping = false;
                        else
                        {
                            VersionInformation info = new VersionInformation
                            {
                                Time = DateTime.Now,
                                Version = input
                            };

                            session.Save(info);
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
