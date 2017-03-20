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
        //[ServiceManagement.StartMethod(Priority = 1)]
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
                        Id = Guid.NewGuid(), Title = "Quarterdeck Watchbill", LastStateChangedBy = user0, LastStateChange = DateTime.Now };
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
                            Title = "ood watch",
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
                    foreach (var person in allUsers.Take(AtwoodUtils.Utilities.GetRandomNumber(5, allUsers.Count)))
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
                        .List().FirstOrDefault();

                    var count = watchbill.WatchDays.Count;

                    watchbill.SetState(WatchbillStatuses.OpenForInputs, DateTime.Now, user0);

                    session.Update(watchbill);

                    transaction.Commit();
                }
            }

            using (var session = DataAccess.NHibernateHelper.CreateStatefulSession())
            {
                using (var transaction = session.BeginTransaction())
                {
                    WatchInputReason reason = new WatchInputReason { Description = "test", Id = Guid.NewGuid(), Value = "because I don't wanna" };
                    session.Save(reason);

                    transaction.Commit();
                }
            }

            using (var session = DataAccess.NHibernateHelper.CreateStatefulSession())
            {
                using (var transaction = session.BeginTransaction())
                {
                    var user0 = session.QueryOver<Person>().Where(x => x.Username.IsInsensitiveLike("user0", MatchMode.Anywhere)).SingleOrDefault();

                    var allUsers = session.QueryOver<Person>().List().ToList();

                    var inputReason = session.QueryOver<WatchInputReason>().List().FirstOrDefault();

                    if (inputReason == null)
                        throw new Exception("uh oh");

                    var watchbill = session.QueryOver<Watchbill>()
                        .List().FirstOrDefault();

                    foreach (var shift in watchbill.WatchDays.SelectMany(x => x.WatchShifts))
                    {
                        foreach (var requirement in watchbill.InputRequirements)
                        {
                            if (Utilities.GetRandomNumber(1, 10000) > 9950)
                            {
                                shift.WatchInputs.Add(new WatchInput
                                {
                                    DateSubmitted = DateTime.Now,
                                    Id = Guid.NewGuid(),
                                    InputReason = inputReason,
                                    Person = requirement.Person,
                                    SubmittedBy = user0
                                });

                                requirement.IsAnswered = true;
                                requirement.AnsweredBy = user0;
                                requirement.DateAnswered = DateTime.Now;
                            }

                        }
                    }

                    session.Update(watchbill);

                    transaction.Commit();
                }
            }

            using (var session = DataAccess.NHibernateHelper.CreateStatefulSession())
            {
                using (var transaction = session.BeginTransaction())
                {
                    var user0 = session.QueryOver<Person>().Where(x => x.Username.IsInsensitiveLike("user0", MatchMode.Anywhere)).SingleOrDefault();

                    var allUsers = session.QueryOver<Person>().List().ToList();

                    var watchbill = session.QueryOver<Watchbill>()
                        .List().FirstOrDefault();

                    var allInputs = watchbill.WatchDays.SelectMany(x => x.WatchShifts.SelectMany(y => y.WatchInputs));

                    foreach (var input in allInputs)
                    {
                        if (Utilities.GetRandomNumber(1, 100) < 90)
                        {
                            input.ConfirmedBy = user0;
                            input.DateConfirmed = DateTime.Now;
                            input.IsConfirmed = true;
                        }
                    }

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
                        .List().FirstOrDefault();

                    watchbill.SetState(WatchbillStatuses.ClosedForInputs, DateTime.Now, user0);

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
                        .List().FirstOrDefault();

                    watchbill.PopulateWatchbill(user0, DateTime.Now);

                    session.Update(watchbill);

                    transaction.Commit();
                }
            }
        }
    }
}
