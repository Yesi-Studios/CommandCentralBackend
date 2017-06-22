using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using CCServ.Entities.Watchbill;
using CCServ.Entities.ReferenceLists.Watchbill;
using CCServ.Entities;
using System.Diagnostics;
using NHibernate.Criterion;
using AtwoodUtils;
using CCServ.Entities.ReferenceLists;

namespace CCServ
{
    static class Scripts
    {

        //[ServiceManagement.StartMethod(Priority = 1)]
        private static void AssignWatchQuals(CLI.Options.LaunchOptions launchOptions)
        {
            using (var session = DataAccess.NHibernateHelper.CreateStatefulSession())
            {
                using (var transaction = session.BeginTransaction())
                {
                    try
                    {

                        var persons = session.QueryOver<Person>().List();

                        var ood = session.Merge(WatchQualifications.OOD);
                        var jood = session.Merge(WatchQualifications.JOOD);
                        var cdo = session.Merge(WatchQualifications.CDO);

                        foreach (var person in persons)
                        {
                            if (person.Paygrade == Paygrades.E1 || person.Paygrade == Paygrades.E2 || 
                                person.Paygrade == Paygrades.E3 || person.Paygrade == Paygrades.E4 ||
                                person.Paygrade == Paygrades.E5)
                            {
                                if (!person.WatchQualifications.Contains(WatchQualifications.JOOD))
                                {
                                    person.WatchQualifications.Add(jood);
                                    "{0} given JOOD.".WriteLine(person);
                                }
                            }

                            if (person.Paygrade == Paygrades.E5 || person.Paygrade == Paygrades.E6)
                            {
                                if (!person.WatchQualifications.Contains(WatchQualifications.OOD))
                                {
                                    person.WatchQualifications.Add(ood);
                                    "{0} given OOD.".WriteLine(person);
                                }
                            }

                            if (person.Paygrade == Paygrades.E7 || person.Paygrade == Paygrades.E8 ||
                                person.Paygrade == Paygrades.E9 || person.Paygrade == Paygrades.O1 ||
                                person.Paygrade == Paygrades.O2 || person.Paygrade == Paygrades.O2E ||
                                person.Paygrade == Paygrades.O3 || person.Paygrade == Paygrades.O3E)
                            {
                                if (!person.WatchQualifications.Contains(WatchQualifications.CDO))
                                {
                                    person.WatchQualifications.Add(cdo);
                                    "{0} given CDO.".WriteLine(person);
                                }
                            }

                            session.Update(person);
                        }

                        "Submit transaction? (y)".WriteLine();
                        if (Console.ReadLine().ToLower() == "y")
                        {
                            transaction.Commit();
                        }
                        else
                        {
                            transaction.Rollback();
                        }

                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        //[ServiceManagement.StartMethod(Priority = 1)]
        private static void WatchbillStats(CLI.Options.LaunchOptions launchOptions)
        {
            using (var session = DataAccess.NHibernateHelper.CreateStatefulSession())
            {
                var watchbill = session.QueryOver<Watchbill>().List().First();

                var data = watchbill.WatchShifts.Select(x => x.WatchAssignment).GroupBy(x => x.PersonAssigned.Department);

                string text = "";
                foreach (var group in data)
                {

                    int totalDep = session.QueryOver<Person>().Where(x => x.Department.Id == group.Key.Id).RowCount();
                    int total = session.QueryOver<Person>().RowCount();

                    text += "{0} : {1}% ({2}/{3}) vs {4}% ({5}/{6})"
                        .FormatS(group.Key,
                        Math.Round(((double)group.ToList().Count / (double)watchbill.WatchShifts.Select(x => x.WatchAssignment).Count()) * 100, 2),
                        group.ToList().Count,
                        watchbill.WatchShifts.Select(x => x.WatchAssignment).Count(),
                        Math.Round(((double)totalDep / (double)total) * 100, 2),
                        totalDep,
                        total);
                    text += Environment.NewLine;
                }

                //File.WriteAllText(@"C:\Users\dkatwoo\Source\Repos\CommandCentralBackend4\CCServ\data.txt", text);
            }
        }

        [ServiceManagement.StartMethod(Priority = 1)]
        private static void TestWatchbill(CLI.Options.LaunchOptions launchOptions)
        {

            if (launchOptions.Rebuild)
            {
                Logging.Log.Debug("starting watchbill test");
                using (var session = DataAccess.NHibernateHelper.CreateStatefulSession())
                {
                    using (var transaction = session.BeginTransaction())
                    {

                        var allUsers = session.QueryOver<Person>().List().ToList();

                        var group = session.Get<WatchEligibilityGroup>(WatchEligibilityGroups.Quarterdeck.Id);
                        foreach (var person in allUsers.Where(x => x.DutyStatus != Entities.ReferenceLists.DutyStatuses.Loss))
                        {
                            group.EligiblePersons.Add(person);
                        }

                        var results = new WatchEligibilityGroup.WatchEligibilityGroupValidator().Validate(group);

                        if (results.Errors.Any())
                            throw new Exception("fuck");

                        session.Update(group);

                        transaction.Commit();
                    }
                }
            }
        }
    }
}
