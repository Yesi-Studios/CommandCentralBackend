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
    public class UICTests
    {

        [TestMethod]
        public void CreateUICs()
        {

            List<UIC> expected = new List<UIC>();

            try
            {
                using (var session = DataAccess.DataProvider.CreateStatefulSession())
                {
                    using (var transaction = session.BeginTransaction())
                    {

                        for (int x = 0; x < Utilities.GetRandomNumber(5, 10); x++)
                        {
                            expected.Add(new UIC
                            {
                                Value = Utilities.RandomString(5),
                                Description = Utilities.RandomString(8),
                                Id = Guid.NewGuid()
                            });

                            session.Save(expected.Last());

                        }

                        transaction.Commit();
                    }
                }
            }
            catch (Exception e)
            {
                Assert.Fail(e.Message);
            }

            try
            {
                using (var session = DataAccess.DataProvider.CreateStatefulSession())
                {
                    var actual = session.QueryOver<UIC>().List();

                    Assert.IsTrue(expected.All(x => actual.Contains(x)));
                }
            }
            catch (Exception e)
            {
                Assert.Fail(e.Message);
            }
        }
    }
}
