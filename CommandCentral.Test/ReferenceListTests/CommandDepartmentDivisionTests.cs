using AtwoodUtils;
using CommandCentral.Entities.ReferenceLists;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommandCentral.Test.ReferenceListTests
{
    [TestClass]
    public class CommandDepartmentDivisionTests
    {
        [TestMethod]
        public void CreateCommands()
        {
            var expected = new List<Command>();

            using (var session = DataAccess.DataProvider.CreateStatefulSession())
            {
                using (var transaction = session.BeginTransaction())
                {
                    for (int x = 0; x < Utilities.GetRandomNumber(2, 4); x++)
                    {
                        expected.Add(new Command
                        {
                            Departments = new List<Department>(),
                            Description = Utilities.RandomString(8),
                            Value = Utilities.RandomString(8),
                            Id = Guid.NewGuid()
                        });

                        session.Save(expected.Last());
                    }

                    transaction.Commit();
                }
            }

            using (var session = DataAccess.DataProvider.CreateStatefulSession())
            {
                var actual = session.QueryOver<Command>().List();

                Assert.IsTrue(expected.All(x => actual.Contains(x)));
            }
        }
    }
}
