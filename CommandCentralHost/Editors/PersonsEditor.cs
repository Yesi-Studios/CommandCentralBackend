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

        internal static void Runner()
        {
            using (var session = NHibernateHelper.CreateSession())
            using (var transaction = session.BeginTransaction())
            {
                try
                {

                    var person = session.QueryOver<Person>().Where(x => x.SSN == "525956681").SingleOrDefault<Person>();
                    person.EmailAddresses.Add(new EmailAddress
                    {
                        Address = "daniel.k.atwood.mil@mail.mil",
                        IsContactable = true,
                        IsPreferred = true,
                        Owner = person
                    });

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
