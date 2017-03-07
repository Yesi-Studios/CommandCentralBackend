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

namespace CCServ
{
    static class Scripts
    {
        [ServiceManagement.StartMethod(Priority = 1)]
        private static void TestWatchbill(CLI.Options.LaunchOptions launchOptions)
        {
            Logging.Log.Debug("starting watchbill test");
            using (var session = DataAccess.NHibernateHelper.CreateStatefulSession())
            {
                using (var transaction = session.BeginTransaction())
                {

                    var command = session.QueryOver<Entities.ReferenceLists.Command>().List().ToList().FirstOrDefault();
                    var user0 = session.QueryOver<Person>().Where(x => x.Username.IsInsensitiveLike("user0", MatchMode.Anywhere)).SingleOrDefault();
                    

                    Debug.Assert(command != null && user0 != null);

                    var watchbill = new Watchbill { Command = command, CreatedBy = user0, CurrentState = WatchbillStatuses.Initial, 
                        Id = Guid.NewGuid(), Title = "fuck you", LastStateChangedBy = user0, LastStateChange = DateTime.Now };
                    session.Save(watchbill);
                    session.Flush();

                    watchbill.ElligibilityGroup = WatchElligibilityGroups.Quarterdeck;
                    session.Update(watchbill);
                    session.Flush();

                    var results = new Watchbill.WatchbillValidator().Validate(watchbill);

                    if (results.Errors.Any())
                        throw new Exception("fuck");

                    transaction.Commit();

                }

                using (var transaction = session.BeginTransaction())
                {
                    var watchbill = session.QueryOver<Watchbill>().List().FirstOrDefault();

                    for (int x = 0; x < 30; x++)
                    {
                        watchbill.WatchDays.Add(new WatchDay
                        {
                            Date = DateTime.Now.Date.AddDays(watchbill.WatchDays.Count),
                            Id = Guid.NewGuid(),
                            Remarks = "test",
                            Watchbill = watchbill
                        });

                        watchbill.WatchDays.Last().WatchShifts.Add(new WatchShift
                        {
                            Range = new AtwoodUtils.TimeRange { Start = watchbill.WatchDays.Last().Date.Date.AddHours(1), End = watchbill.WatchDays.Last().Date.Date.AddHours(8) },
                            Id = Guid.NewGuid(),
                            ShiftType = WatchShiftTypes.JOOD,
                            Title = "jood watch",
                            WatchDays = new List<WatchDay> { watchbill.WatchDays.Last() }
                        });

                        watchbill.WatchDays.Last().WatchShifts.Add(new WatchShift
                        {
                            Range = new AtwoodUtils.TimeRange { Start = watchbill.WatchDays.Last().Date.Date.AddHours(9), End = watchbill.WatchDays.Last().Date.Date.AddHours(12) },
                            Id = Guid.NewGuid(),
                            ShiftType = WatchShiftTypes.JOOD,
                            Title = "jood watch",
                            WatchDays = new List<WatchDay> { watchbill.WatchDays.Last() }
                        });

                        watchbill.WatchDays.Last().WatchShifts.Add(new WatchShift
                        {
                            Range = new AtwoodUtils.TimeRange { Start = watchbill.WatchDays.Last().Date.Date.AddHours(4), End = watchbill.WatchDays.Last().Date.Date.AddHours(6) },
                            Id = Guid.NewGuid(),
                            ShiftType = WatchShiftTypes.OOD,
                            Title = "jood watch",
                            WatchDays = new List<WatchDay> { watchbill.WatchDays.Last() }
                        });

                    }

                    var results = new Watchbill.WatchbillValidator().Validate(watchbill);

                    if (results.Errors.Any())
                        throw new Exception("fuck");

                    session.Update(watchbill);

                    transaction.Commit();
                }

                using (var transaction = session.BeginTransaction())
                {

                    var allUsers = session.QueryOver<Person>().List().ToList();

                    var group = session.Get<WatchElligibilityGroup>(WatchElligibilityGroups.Quarterdeck.Id);
                    foreach (var person in allUsers.Take(AtwoodUtils.Utilities.GetRandomNumber(0, allUsers.Count)))
                    {
                        group.ElligiblePersons.Add(person);
                    }

                    var results = new WatchElligibilityGroup.WatchElligibilityGroupValidator().Validate(group);

                    if (results.Errors.Any())
                        throw new Exception("fuck");

                    session.Update(group);

                    transaction.Commit();

                }


            }

            using (var session = DataAccess.NHibernateHelper.CreateStatefulSession())
            {
                using (var transaction = session.BeginTransaction())
                {
                    var watchbill = session.QueryOver<Watchbill>()
                        .Fetch(x => x.ElligibilityGroup).Eager
                        .List().FirstOrDefault();

                    var persons = watchbill.ElligibilityGroup.ElligiblePersons.ToList();

                    if (!persons.Any())
                        throw new Exception("fuck");

                    transaction.Commit();
                }
            }

            using (var session = DataAccess.NHibernateHelper.CreateStatefulSession())
            {
                using (var transaction = session.BeginTransaction())
                {
                    var user0 = session.QueryOver<Person>().Where(x => x.Username.IsInsensitiveLike("user0", MatchMode.Anywhere)).SingleOrDefault();

                    var watchbill = session.QueryOver<Watchbill>()
                        .Fetch(x => x.ElligibilityGroup).Eager
                        .List().FirstOrDefault();

                    watchbill.SetState(WatchbillStatuses.OpenForInputs, DateTime.Now, user0);

                    session.Update(watchbill);

                    transaction.Commit();
                }
            }

            using (var session = DataAccess.NHibernateHelper.CreateStatefulSession())
            {
                using (var transaction = session.BeginTransaction())
                {
                    var user0 = session.QueryOver<Person>().Where(x => x.Username.IsInsensitiveLike("user0", MatchMode.Anywhere)).SingleOrDefault();

                    var watchbill = session.QueryOver<Watchbill>()
                        .Fetch(x => x.ElligibilityGroup).Eager
                        .List().FirstOrDefault();

                    watchbill.SetState(WatchbillStatuses.Initial, DateTime.Now, user0);

                    session.Update(watchbill);

                    transaction.Commit();
                }
            }


            int i = 0;

        }
    }
}
