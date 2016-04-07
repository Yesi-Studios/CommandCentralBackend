using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using MySql.Data.MySqlClient;
using MySql.Data.Common;
using AtwoodUtils;

namespace UnifiedServiceFramework.Administration
{
    /// <summary>
    /// Provides methods that allow arbitray execution of SQL code against the database.
    /// <para />
    /// WARNING!  THIS CLASS OR ITS METHODS SHOULD NEVER BE EXPOSED TO THE CLIENT THROUGH THE SERVICE INTERFACE.
    /// </summary>
    public static class Executer
    {
        /// <summary>
        /// Executes a SQL string, without parameters, that is not expected to return results.
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public static string ExecuteNonQuery(string query)
        {
            try
            {
                using (MySqlConnection connection = new MySqlConnection(Framework.Settings.ConnectionString))
                {
                    connection.Open();

                    MySqlCommand command = connection.CreateCommand();
                    command.CommandType = CommandType.Text;
                    command.CommandText = query;

                    command.ExecuteNonQuery();

                    return "Success!";
                }
            }
            catch (Exception e)
            {
                return e.Message;
            }
        }

        /// <summary>
        /// Executes a query against the database, without parameters, and returns the result as a JSON reprsentation of a table.
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public static string ExecuteQuery(string query)
        {
            try
            {
                using (MySqlConnection connection = new MySqlConnection(Framework.Settings.ConnectionString))
                {
                    connection.Open();

                    MySqlCommand command = connection.CreateCommand();
                    command.CommandType = CommandType.Text;
                    command.CommandText = query;

                    DataTable table = new DataTable();

                    using (MySqlDataReader reader = (MySqlDataReader)command.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            table.Load(reader);
                        }
                    }

                    return table.Serialize();
                }
            }
            catch (Exception e)
            {
                return e.Message;
            }
        }
    }
}
