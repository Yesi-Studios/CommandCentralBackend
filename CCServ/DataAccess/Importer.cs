using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using System.Data;
using AtwoodUtils;
using CCServ.Logging;

namespace CCServ.DataAccess
{
    /// <summary>
    /// Contains the ingest old database script.
    /// </summary>
    public static class Importer
    {
        /// <summary>
        /// Ingests the old database into the new database.  NHibernate must be configured prior to calling this method.
        /// </summary>
        public static void IngestOldDatabase()
        {

            using (var session = NHibernateHelper.CreateStatefulSession())
            using (var transaction = session.BeginTransaction())
            {
                try
                {
                    var connectionString = String.Format("server={0};uid={1};pwd={2};database={3}", "gord14ec204", "dkatwoo", "dkatwoo987", "sullitest");

                    using (MySqlConnection connection = new MySqlConnection(connectionString))
                    {
                        connection.Open();

                        //Let's start at the top and load in all of the references.

                        //Commands first
                        Log.Info("Importing commands...");
                        using (MySqlCommand command = new MySqlCommand("SELECT * FROM `commands`", connection))
                        {
                            using (MySqlDataReader reader = command.ExecuteReader())
                            {
                                if (reader.HasRows)
                                {
                                    while (reader.Read())
                                    {
                                        var newCommand = new Entities.ReferenceLists.Command
                                        {
                                            Departments = new List<Entities.ReferenceLists.Department>(),
                                            Description = reader["CMD_name"] as string,
                                            Value = reader["CMD_pla"] as string
                                        };

                                        session.Save(newCommand);

                                        var results = newCommand.Validate();
                                        if (!results.IsValid)
                                        {
                                            foreach (var error in results.Errors)
                                            {
                                                session.Save(new ImportError
                                                {
                                                    ErrorMessage = error.ErrorMessage,
                                                    ObjectId = newCommand.Id.ToString(),
                                                    ObjectName = "Command",
                                                    PropertyName = error.PropertyName,
                                                    AttemptedValue = error.AttemptedValue.ToString()
                                                });
                                            }
                                        }
                                    }

                                    Log.Info("Imported {0} commands.".FormatS(session.QueryOver<Entities.ReferenceLists.Command>().RowCount()));
                                }
                                else
                                {
                                    Log.Warning("Import from old database failed to read from the commands table.");
                                }
                            }
                        }

                        //Now departments
                        Log.Info("Importing departments...");
                        //Select only those departments that are active. That's what the where clause does.
                        using (MySqlCommand command = new MySqlCommand("SELECT * FROM `dept` WHERE `DEPT_actv` = 1", connection))
                        {
                            using (MySqlDataReader reader = command.ExecuteReader())
                            {
                                if (reader.HasRows)
                                {
                                    //We need the command that we're going to add everyone to.
                                    //Based on the old database, there is no relationship from command -> department.  
                                    //Brandy says to just assume that all departments are in NIOC GA.

                                    var niocGA = session.QueryOver<Entities.ReferenceLists.Command>().Where(x => x.Value == "NAVIOCOM GA").SingleOrDefault();

                                    if (niocGA == null)
                                    {
                                        Log.Warning("Could not read departments because the command, nioc ga, could not be found.");
                                    }
                                    else
                                    {
                                        while (reader.Read())
                                        {
                                            var department = new Entities.ReferenceLists.Department
                                            {
                                                Command = niocGA,
                                                Description = null,
                                                Divisions = new List<Entities.ReferenceLists.Division>(),
                                                Value = reader["DEPT_dept"] as string
                                            };

                                            session.Save(department);

                                            var results = department.Validate();
                                            if (!results.IsValid)
                                            {
                                                foreach (var error in results.Errors)
                                                {
                                                    session.Save(new ImportError
                                                    {
                                                        ErrorMessage = error.ErrorMessage,
                                                        ObjectId = department.Id.ToString(),
                                                        ObjectName = "Department",
                                                        PropertyName = error.PropertyName,
                                                        AttemptedValue = error.AttemptedValue.ToString()
                                                    });
                                                }
                                            }
                                        }

                                        Log.Info("Imported {0} departments.".FormatS(session.QueryOver<Entities.ReferenceLists.Department>().RowCount()));
                                    }
                                }
                                else
                                {
                                    Log.Warning("Import from old database failed to read from the departments table.");
                                }
                            }
                        }
                    }

                    transaction.Commit();
                }
                catch (MySqlException ex)
                {
                    transaction.Rollback();

                    switch (ex.Number)
                    {
                        case 0:
                            {
                                Log.Warning("Old database could not be contacted!");
                                break;
                            }
                        case 1045:
                            {
                                Log.Warning("The Username/password combination for the old database was invalid!");
                                break;
                            }
                        case 1042:
                            {
                                Log.Warning("The old database was either offline or otherwise non-contactable.");
                                break;
                            }
                        default:
                            {
                                Log.Exception(ex, "While attempting to connect to the old database an exception occurred.");
                                break;
                            }
                    }
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    throw;
                }
            }

            
        }
    }
}
