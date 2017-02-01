using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace CCServ
{
    static class Scripts
    {
        /// <summary>
        /// Registers the roll over method to run at a certain time.
        /// </summary>
        //[ServiceManagement.StartMethod(Priority = 1)]
        private static void ImportWatchQuals(CLI.Options.LaunchOptions launchOptions)
        {

            List<string> updatedNames = new List<string>();

            File.ReadAllLines(@"C:\Users\dkatwoo\Source\Repos\CommandCentralBackend4\CCServ\TextFile1.txt")
                .ToList().Select(x =>
                    {
                        var elements = x.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries).ToList();

                        List<string> quals = new List<string>();

                        if (elements.Count == 1)
                        {

                        }
                        else
                            if (elements.Count == 2)
                            {
                                quals = elements[1].Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries).Select(y => y.Trim()).ToList();
                            }
                            else
                            {
                                throw new Exception("oh fuck");
                            }

                        var id = Guid.Parse(elements[0].Trim());

                        return new
                        {
                            Id = id,
                            Quals = quals
                        };
                    }).ToList()
                    .ForEach(x =>
                        {
                            if (x.Quals.Contains("jood", StringComparer.CurrentCultureIgnoreCase))
                            {
                                using (var session = DataAccess.NHibernateHelper.CreateStatefulSession())
                                using (var transaction = session.BeginTransaction())
                                {
                                    try
                                    {
                                        var person = session.Get<Entities.Person>(x.Id);

                                        if (person == null)
                                            throw new Exception("oh shit");

                                        if (!person.WatchQualifications.Contains(Entities.ReferenceLists.WatchQualifications.JOOD))
                                        {
                                            person.WatchQualifications.Add(Entities.ReferenceLists.WatchQualifications.JOOD);

                                            session.Save(person);

                                            updatedNames.Add(person.ToString());
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
                        });

            Logging.Log.Critical(String.Join(Environment.NewLine, updatedNames));

        }
    }
}
