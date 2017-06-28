using AtwoodUtils;
using CommandCentral.Entities;
using CommandCentral.Entities.ReferenceLists;
using Microsoft.VisualStudio.TestTools.UnitTesting;
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
        public Person CreatePerson(Command command, Department department, Division division, 
            UIC uic, string lastName, IEnumerable<Authorization.Groups.PermissionGroup> permissionGroups,
            IEnumerable<WatchQualification> watchQuals)
        {
            var person = new Person()
            {
                Id = Guid.NewGuid(),
                LastName = lastName,
                FirstName = String.Join(".", permissionGroups.Select(x => x.GroupName.First().ToString())),
                MiddleName = "{0} {1} {2}".With(command.Value, department.Value, division.Value),
                Command = command,
                Department = department,
                Division = division,
                UIC = uic,
                SSN = Utilities.GenerateSSN(),
                DoDId = Utilities.GenerateDoDId(),
                IsClaimed = true,
                Username = String.Join(".", permissionGroups.Select(x => x.GroupName.First().ToString())),
                PasswordHash = ClientAccess.PasswordHash.CreateHash("a"),
                Sex = ReferenceListHelper<Sex>.Random(1).First(),
                DateOfBirth = new DateTime(Utilities.GetRandomNumber(1970, 2000), Utilities.GetRandomNumber(1, 12), Utilities.GetRandomNumber(1, 28)),
                DateOfArrival = new DateTime(Utilities.GetRandomNumber(1970, 2000), Utilities.GetRandomNumber(1, 12), Utilities.GetRandomNumber(1, 28)),
                EAOS = new DateTime(Utilities.GetRandomNumber(1970, 2000), Utilities.GetRandomNumber(1, 12), Utilities.GetRandomNumber(1, 28)),
                PRD = new DateTime(Utilities.GetRandomNumber(1970, 2000), Utilities.GetRandomNumber(1, 12), Utilities.GetRandomNumber(1, 28)),
                Paygrade = ReferenceListHelper<Paygrade>.Random(1).First(),
                DutyStatus = ReferenceListHelper<DutyStatus>.Random(1).First(),
                PermissionGroupNames = permissionGroups.Select(x => x.GroupName).ToList(),
                WatchQualifications = watchQuals.ToList()
            };

            person.EmailAddresses = new List<EmailAddress> { new EmailAddress
                {
                    Address = "{0}.{1}.{2}.mil@mail.mil".With(person.FirstName, person.MiddleName[0], person.LastName),
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

                    var person = CreatePerson(command, department, division, uic, "developer",
                        new[] { new Authorization.Groups.Definitions.Developers() },
                        ReferenceListHelper<WatchQualification>.All());

                    session.Save(person);

                    transaction.Commit();

                    Logging.Log.Info("Created developer: {0}.".With(person));
                }
            }
        }
    }
}
