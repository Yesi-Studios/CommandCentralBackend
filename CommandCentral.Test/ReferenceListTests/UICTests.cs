using AtwoodUtils;
using CommandCentral.Entities.ReferenceLists;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Assert = Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace CommandCentral.Test.ReferenceListTests
{
    public static class UICTests
    {
        public static void CreateUICs()
        {

            var expected = new List<UIC>();

            try
            {
                using (var session = DataAccess.DataProvider.CreateStatefulSession())
                {
                    using (var transaction = session.BeginTransaction())
                    {

                        for (var x = 0; x < Utilities.GetRandomNumber(5, 10); x++)
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
