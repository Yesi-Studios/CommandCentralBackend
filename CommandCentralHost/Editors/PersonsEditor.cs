using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandCentral.DataAccess;
using CommandCentral.Entities;
using AtwoodUtils;
using CommandCentral.Entities.ReferenceLists;

namespace CommandCentralHost.Editors
{
    internal static class PersonsEditor
    {

        internal static void CreateAtwood()
        {
            using (var session = NHibernateHelper.CreateStatefulSession())
            using (var transaction = session.BeginTransaction())
            {
                try
                {

                    var person = new Person()
                    {
                        Id = Guid.NewGuid(),
                        LastName = "Atwood",
                        FirstName = "Daniel",
                        SSN = "525956681",
                        IsClaimed = false,
                        EmailAddresses = new List<EmailAddress>()
                        {
                            new EmailAddress
                            {
                                Address = "daniel.k.atwood.mil@mail.mil",
                                IsContactable = true,
                                IsPreferred = true
                            }
                        },
                        DateOfBirth = new DateTime(1992, 04, 24),
                        DateOfArrival = new DateTime(2013, 08, 23),
                        EAOS = new DateTime(2018, 1, 27),
                        Paygrade = CommandCentral.Paygrades.E5,
                        Designation = session.QueryOver<Designation>().Where(x => x.Value == "CTI").SingleOrDefault<Designation>(),
                        UIC = session.QueryOver<UIC>().Where(x => x.Value == "40533").SingleOrDefault<UIC>(),
                        DutyStatus = CommandCentral.DutyStatuses.Active,
                        Command = session.QueryOver<Command>().Where(x => x.Value == "NIOC Georgia").SingleOrDefault<Command>(),
                        Department = session.QueryOver<Command>().Where(x => x.Value == "NIOC Georgia").SingleOrDefault<Command>()
                                        .Departments.First(x => x.Value == "N0"),
                        Division = session.QueryOver<Command>().Where(x => x.Value == "NIOC Georgia").SingleOrDefault<Command>()
                                        .Departments.First(x => x.Value == "N0").Divisions.First(x => x.Value == "N0"),
                    };

                    person.CurrentMusterStatus = MusterRecord.CreateDefaultMusterRecordForPerson(person, DateTime.Now);

                    session.SaveOrUpdate(person);

                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        internal static void CreateMcLean()
        {
            using (var session = NHibernateHelper.CreateStatefulSession())
            using (var transaction = session.BeginTransaction())
            {
                try
                {

                    var person = new Person()
                    {
                        Id = Guid.NewGuid(),
                        LastName = "McLean",
                        FirstName = "Angus",
                        SSN = "888888888",
                        IsClaimed = false,
                        EmailAddresses = new List<EmailAddress>()
                        {
                            new EmailAddress
                            {
                                Address = "angus.l.mclean5.mil@mail.mil",
                                IsContactable = true,
                                IsPreferred = true
                            }
                        },
                        DateOfBirth = new DateTime(1992, 04, 24),
                        DateOfArrival = new DateTime(2013, 08, 23),
                        EAOS = new DateTime(2018, 1, 27),
                        Paygrade = CommandCentral.Paygrades.E5,
                        Designation = session.QueryOver<Designation>().Where(x => x.Value == "CTI").SingleOrDefault<Designation>(),
                        UIC = session.QueryOver<UIC>().Where(x => x.Value == "40533").SingleOrDefault<UIC>(),
                        DutyStatus = CommandCentral.DutyStatuses.Active,
                        Command = session.QueryOver<Command>().Where(x => x.Value == "NIOC Georgia").SingleOrDefault<Command>(),
                        Department = session.QueryOver<Command>().Where(x => x.Value == "NIOC Georgia").SingleOrDefault<Command>()
                                        .Departments.First(x => x.Value == "N0"),
                        Division = session.QueryOver<Command>().Where(x => x.Value == "NIOC Georgia").SingleOrDefault<Command>()
                                        .Departments.First(x => x.Value == "N0").Divisions.First(x => x.Value == "N0")
                    };



                    person.CurrentMusterStatus = MusterRecord.CreateDefaultMusterRecordForPerson(person, DateTime.Now);

                    session.SaveOrUpdate(person);

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
