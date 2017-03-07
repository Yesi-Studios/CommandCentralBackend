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
                    var allUsers = session.QueryOver<Person>().List().ToList();

                    Debug.Assert(command != null && user0 != null);

                    var watchbill = new Watchbill { Command = command, CreatedBy = user0, CurrentState = WatchbillStatuses.Initial, Id = Guid.NewGuid(), Title = "fuck you" };
                    session.Save(watchbill);
                    session.Flush();

                    var ellGroup = new WatchbillElligibilityGroup { ElligiblePersons = allUsers.Take(new Random(DateTime.Now.Millisecond).Next(0, allUsers.Count)).ToList(), Id = Guid.NewGuid(), Name = "quarterdeck" };
                    session.Save(ellGroup);
                    session.Flush();

                    watchbill.ElligibilityGroup = ellGroup;
                    session.Update(watchbill);
                    session.Flush();

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
                            From = watchbill.WatchDays.Last().Date.Date,
                            Id = Guid.NewGuid(),
                            ShiftType = WatchShiftTypes.JOOD,
                            Title = "jood watch",
                            To = watchbill.WatchDays.Last().Date.Date.AddHours(8),
                            WatchDays = new List<WatchDay> { watchbill.WatchDays.Last() }
                        });
                    }

                    session.Update(watchbill);

                    transaction.Commit();
                }

                using (var transaction = session.BeginTransaction())
                {
                }

            }



        }
    }
}
