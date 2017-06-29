using AtwoodUtils;
using CommandCentral.Authorization;
using CommandCentral.Authorization.Groups;
using CommandCentral.Entities;
using CommandCentral.Entities.ReferenceLists;
using CommandCentral.Entities.ReferenceLists.Watchbill;
using CommandCentral.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NHibernate.Criterion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommandCentral.Test
{
    [TestClass]
    public class PersonTests
    {
        static Dictionary<string, int> emailAddresses = new Dictionary<string, int>();

        public Person CreatePerson(Command command, Department department, Division division, 
            UIC uic, string lastName, string username, IEnumerable<PermissionGroup> permissionGroups,
            IEnumerable<WatchQualification> watchQuals, Paygrade paygrade)
        {
            var person = new Person()
            {
                Id = Guid.NewGuid(),
                LastName = lastName,
                MiddleName = division.Value,
                Command = command,
                Department = department,
                Division = division,
                UIC = uic,
                SSN = Utilities.GenerateSSN(),
                DoDId = Utilities.GenerateDoDId(),
                IsClaimed = true,
                Username = username,
                PasswordHash = ClientAccess.PasswordHash.CreateHash("a"),
                Sex = ReferenceListHelper<Sex>.Random(1).First(),
                DateOfBirth = new DateTime(Utilities.GetRandomNumber(1970, 2000), Utilities.GetRandomNumber(1, 12), Utilities.GetRandomNumber(1, 28)),
                DateOfArrival = new DateTime(Utilities.GetRandomNumber(1970, 2000), Utilities.GetRandomNumber(1, 12), Utilities.GetRandomNumber(1, 28)),
                EAOS = new DateTime(Utilities.GetRandomNumber(1970, 2000), Utilities.GetRandomNumber(1, 12), Utilities.GetRandomNumber(1, 28)),
                PRD = new DateTime(Utilities.GetRandomNumber(1970, 2000), Utilities.GetRandomNumber(1, 12), Utilities.GetRandomNumber(1, 28)),
                Paygrade = paygrade,
                DutyStatus = ReferenceListHelper<DutyStatus>.Random(1).First(),
                PermissionGroupNames = permissionGroups.Select(x => x.GroupName).ToList(),
                WatchQualifications = watchQuals.ToList()
            };

            var resolvedPermissions = person.ResolvePermissions(null);
            person.FirstName = String.Join("|", resolvedPermissions.HighestLevels.Select(x => "{0}:{1}".With(x.Key.ToString().Substring(0, 2), x.Value.ToString().Substring(0, 3))));

            var emailAddress = "{0}.{1}.{2}.mil@mail.mil".With(person.FirstName, person.MiddleName[0], person.LastName);

            if (emailAddresses.ContainsKey(emailAddress))
            {
                emailAddresses[emailAddress]++;
            }
            else
            {
                emailAddresses.Add(emailAddress, 1);
            }

            emailAddress = "{0}.{1}.{2}{3}.mil@mail.mil".With(person.FirstName, person.MiddleName[0], person.LastName, emailAddresses[emailAddress]);

            person.EmailAddresses = new List<EmailAddress> { new EmailAddress
                {
                    Address = emailAddress,
                    Id = Guid.NewGuid(),
                    IsContactable = true,
                    IsPreferred = true
                } };

            person.CurrentMusterRecord = MusterRecord.CreateDefaultMusterRecordForPerson(person, DateTime.UtcNow);

            person.AccountHistory = new List<AccountHistoryEvent>
            {
                new AccountHistoryEvent
                {
                    AccountHistoryEventType = ReferenceListHelper<AccountHistoryType>.Find("Creation"),
                    EventTime = DateTime.UtcNow
                }
            };

            return person;
        }

        [TestMethod]
        public void CreateDeveloper()
        {
            using (var session = DataAccess.DataProvider.CreateStatefulSession())
            {
                using (var transaction = session.BeginTransaction())
                {
                    var command = ReferenceListHelper<Command>.All().First();
                    var department = command.Departments.First();
                    var division = department.Divisions.First();
                    var uic = ReferenceListHelper<UIC>.Random(1).First();

                    var eligibilityGroup = session.Merge(ReferenceListHelper<WatchEligibilityGroup>.Find("Quarterdeck"));

                    var person = CreatePerson(command, department, division, uic, "developer", "dev",
                        new[] { new Authorization.Groups.Definitions.Developers() },
                        ReferenceListHelper<WatchQualification>.All(), ReferenceListHelper<Paygrade>.Find("E5"));

                    session.Save(person);

                    eligibilityGroup.EligiblePersons.Add(person);

                    session.Update(person);

                    transaction.Commit();

                    Logging.Log.Info("Created developer: {0}.".With(person));
                }
            }
        }

        [TestMethod]
        public void CreateUsers()
        {
            List<Guid> expected = new List<Guid>();

            using (var session = DataAccess.DataProvider.CreateStatefulSession())
            {
                using (var transaction = session.BeginTransaction())
                {
                    var paygrades = ReferenceListHelper<Paygrade>.All().Where(x => (x.Value.Contains("E") && !x.Value.Contains("O")) || (x.Value.Contains("O") && !x.Value.Contains("C")));

                    var eligibilityGroup = session.Merge(ReferenceListHelper<WatchEligibilityGroup>.Find("Quarterdeck"));

                    var divPermGroups = PermissionGroup.AllPermissionGroups.Where(y => y.AccessLevel == ChainOfCommandLevels.Division).ToList();
                    var depPermGroups = PermissionGroup.AllPermissionGroups.Where(y => y.AccessLevel == ChainOfCommandLevels.Department).ToList();
                    var comPermGroups = PermissionGroup.AllPermissionGroups.Where(y => y.AccessLevel == ChainOfCommandLevels.Command).ToList();

                    foreach (var command in ReferenceListHelper<Command>.All())
                    {
                        foreach (var department in command.Departments)
                        {
                            foreach (var division in department.Divisions)
                            {

                                //Add Sailors
                                for (int x = 0; x < 30; x++)
                                {
                                    var paygrade = paygrades.Shuffle().First();
                                    var uic = ReferenceListHelper<UIC>.Random(1).First();

                                    List<WatchQualification> quals = new List<WatchQualification>();
                                    List<PermissionGroup> permGroups = new List<PermissionGroup>();

                                    var permChance = Utilities.GetRandomNumber(0, 100);

                                    if (!paygrade.IsCivilianPaygrade())
                                    {
                                        if (permChance >= 0 && permChance < 60)
                                        {
                                            //Users
                                        }
                                        else if (permChance >= 60 && permChance < 80)
                                        {
                                            //Division leadership
                                            permGroups.AddRange(divPermGroups.Shuffle().Take(Utilities.GetRandomNumber(1, divPermGroups.Count)));
                                        }
                                        else if (permChance >= 80 && permChance < 90)
                                        {
                                            //Dep leadership
                                            permGroups.AddRange(depPermGroups.Shuffle().Take(Utilities.GetRandomNumber(1, depPermGroups.Count)));
                                        }
                                        else if (permChance >= 90 && permChance < 95)
                                        {
                                            //Com leadership
                                            permGroups.AddRange(comPermGroups.Shuffle().Take(Utilities.GetRandomNumber(1, comPermGroups.Count)));
                                        }
                                        else if (permChance >= 95 && permChance <= 100)
                                        {
                                            permGroups.Add(new Authorization.Groups.Definitions.Admin());
                                        }
                                    }

                                    if (paygrade.IsOfficerPaygrade())
                                    {
                                        quals.Add(ReferenceListHelper<WatchQualification>.Find("CDO"));
                                    }
                                    else if (paygrade.IsEnlistedPaygrade())
                                    {
                                        if (paygrade.IsChief())
                                        {
                                            quals.Add(ReferenceListHelper<WatchQualification>.Find("CDO"));
                                        }
                                        else
                                        {
                                            if (paygrade.IsPettyOfficer())
                                            {
                                                quals.AddRange(ReferenceListHelper<WatchQualification>.FindAll("OOD", "JOOD"));
                                            }
                                            else if (paygrade.IsSeaman())
                                            {
                                                quals.Add(ReferenceListHelper<WatchQualification>.Find("JOOD"));
                                            }
                                            else
                                            {
                                                throw new Exception("We shouldn't be here...");
                                            }
                                        }
                                    }
                                    else if (paygrade.IsCivilianPaygrade())
                                    {
                                        //Do nothing for now
                                    }
                                    else
                                    {
                                        throw new Exception("An unknown paygrade was found! {0}".With(paygrade));
                                    }

                                    var person = CreatePerson(command, department, division, uic, "user" + expected.Count.ToString(), "user" + expected.Count.ToString(), permGroups, quals, paygrade);

                                    session.Save(person);

                                    if (!paygrade.IsCivilianPaygrade())
                                    {
                                        eligibilityGroup.EligiblePersons.Add(person);
                                    }

                                    expected.Add(person.Id);

                                }
                            }
                        }
                    }

                    session.Update(eligibilityGroup);

                    transaction.Commit();

                }
            }

            using (var session = DataAccess.DataProvider.CreateStatefulSession())
            {
                Assert.AreEqual(session.QueryOver<Person>().Where(x => x.Id.IsIn(expected.ToArray())).RowCount(), expected.Count);

                Assert.AreEqual(session.QueryOver<Person>().List().Count(x => !x.Paygrade.IsCivilianPaygrade()), session.Merge(ReferenceListHelper<WatchEligibilityGroup>.Find("Quarterdeck")).EligiblePersons.Count);
            }
        }
    }
}
