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
            using (var session = NHibernateHelper.CreateSession())
            using (var transaction = session.BeginTransaction())
            {
                try
                {

                    var person = new Person()
                    {
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
                        Rank = session.QueryOver<Rank>().Where(x => x.Value == "E5").SingleOrDefault<Rank>(),
                        Rate = session.QueryOver<Rate>().Where(x => x.Value == "CTI2").SingleOrDefault<Rate>(),
                        UIC = session.QueryOver<UIC>().Where(x => x.Value == "40533").SingleOrDefault<UIC>(),
                        DutyStatus = session.QueryOver<DutyStatus>().Where(x => x.Value == "Active").SingleOrDefault<DutyStatus>(),
                        Command = session.QueryOver<Command>().Where(x => x.Value == "NIOC Georgia").SingleOrDefault<Command>(),
                        Department = session.QueryOver<Command>().Where(x => x.Value == "NIOC Georgia").SingleOrDefault<Command>()
                                        .Departments.First(x => x.Value == "N0"),
                        Division = session.QueryOver<Command>().Where(x => x.Value == "NIOC Georgia").SingleOrDefault<Command>()
                                        .Departments.First(x => x.Value == "N0").Divisions.First(x => x.Value == "N0"),


                    };
                    person.EmailAddresses.First().Owner = person;

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
