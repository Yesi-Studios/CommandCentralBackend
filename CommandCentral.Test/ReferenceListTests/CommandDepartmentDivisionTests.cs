﻿using AtwoodUtils;
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
    public static class CommandDepartmentDivisionTests
    {
        public static void CreateCommands()
        {
            Logging.Log.Info("Creating commands...");

            var expected = new List<Command>();

            using (var session = DataAccess.DataProvider.CreateStatefulSession())
            {
                using (var transaction = session.BeginTransaction())
                {
                    for (var x = 0; x < 1; x++)
                    {
                        expected.Add(new Command
                        {
                            Description = Utilities.RandomString(8),
                            Value = x.ToString(),
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
                Logging.Log.Info("Created {0} commands.".With(actual.Count));
            }
        }

        public static void CreateDepartments()
        {
            Logging.Log.Info("Creating departments...");

            var expected = new List<Department>();

            using (var session = DataAccess.DataProvider.CreateStatefulSession())
            {
                using (var transaction = session.BeginTransaction())
                {

                    var commands = session.QueryOver<Command>().List();

                    foreach (var command in commands)
                    {
                        for (var x = 0; x < Utilities.GetRandomNumber(2, 4); x++)
                        {
                            expected.Add(new Department
                            {
                                Command = command,
                                Description = Utilities.RandomString(8),
                                Value = "{0}.{1}".With(command.Value, x.ToString()),
                                Id = Guid.NewGuid()
                            });

                            session.Save(expected.Last());
                        }
                    }

                    transaction.Commit();
                }
            }

            using (var session = DataAccess.DataProvider.CreateStatefulSession())
            {
                var actual = session.QueryOver<Department>().List();

                Assert.IsTrue(expected.All(x => actual.Contains(x)));
                Logging.Log.Info("Created {0} departments.".With(actual.Count));
            }

        }

        public static void CreateDivisions()
        {
            Logging.Log.Info("Creating divisions...");

            var expected = new List<Division>();

            using (var session = DataAccess.DataProvider.CreateStatefulSession())
            {
                using (var transaction = session.BeginTransaction())
                {

                    var departments = session.QueryOver<Department>().List();

                    foreach (var department in departments)
                    {
                        for (var x = 0; x < Utilities.GetRandomNumber(2, 4); x++)
                        {
                           expected.Add(new Division
                            {
                                Department = department,
                                Description = Utilities.RandomString(8),
                                Value = "{0}.{1}".With(department.Value, x.ToString()),
                                Id = Guid.NewGuid()
                            });

                            session.Save(expected.Last());
                        }
                    }

                    transaction.Commit();
                }
            }

            using (var session = DataAccess.DataProvider.CreateStatefulSession())
            {
                var actual = session.QueryOver<Division>().List();

                Assert.IsTrue(expected.All(x => actual.Contains(x)));
                Logging.Log.Info("Created {0} divisions.".With(actual.Count));
            }

        }
    }
}
