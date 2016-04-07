using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Collections.Concurrent;
using System.Reflection;
using MySql.Data.MySqlClient;
using MySql.Data.Common;

namespace UnifiedServiceFramework.Validation
{
    /// <summary>
    /// Provides methods and members that allow for dynamic validation against the database schematic without having to load the whole thing from the database.
    /// </summary>
    public static class SchemaValidation
    {

        private static ConcurrentDictionary<string, ConcurrentBag<string>> _databaseSchema = new ConcurrentDictionary<string, ConcurrentBag<string>>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Gets the database schematic.
        /// </summary>
        public static ConcurrentDictionary<string, ConcurrentBag<string>> DatabaseSchema
        {
            get
            {
                return _databaseSchema;
            }
            private set
            {
                _databaseSchema = value;
            }
        }

        /// <summary>
        /// Loads the entire database schematic into a dictionary where the key is the table name and the value is a list of of hte column names. Also updates the cache with the database schema.
        /// </summary>
        /// <param name="connectionString"></param>
        /// <returns></returns>
        public static async Task<ConcurrentDictionary<string, ConcurrentBag<string>>> LoadDatabaseSchema(string connectionString)
        {
            try
            {
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    DataTable table = connection.GetSchema("COLUMNS");
                    ConcurrentDictionary<string, ConcurrentBag<string>> result = new ConcurrentDictionary<string, ConcurrentBag<string>>();
                    table.AsEnumerable().ToList().ForEach(x =>
                    {
                        string tableName = x["TABLE_Name"].ToString();
                        string columnName = x["COLUMN_NAME"].ToString();

                        result.AddOrUpdate(tableName, new ConcurrentBag<string>() { columnName }, (key, value) =>
                        {
                            List<string> newValue = value.ToList();
                            newValue.Add(columnName);
                            return new ConcurrentBag<string>(newValue);
                        });
                    });

                    DatabaseSchema = result;

                    return result;
                }
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Returns a bool indicating whether or not a given column name is the primary key of that column.
        /// </summary>
        /// <param name="columnName"></param>
        /// <param name="tableNames"></param>
        /// <param name="connectionString"></param>
        /// <returns></returns>
        internal static async Task<bool> IsColumnNamePrimaryKeyOfTables(string columnName, List<string> tableNames, string connectionString)
        {
            try
            {
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    foreach (string tableName in tableNames)
                    {
                        string query = string.Format("SHOW INDEX FROM `{0}` WHERE `Key_name` = 'PRIMARY'", tableName);
                        DataTable table = new DataTable();
                        using (MySqlCommand command = new MySqlCommand(query, connection))
                        {
                            using (MySqlDataReader reader = (MySqlDataReader)await command.ExecuteReaderAsync())
                            {
                                if (reader.HasRows)
                                {
                                    table.Load(reader);
                                }
                                else
                                    throw new Exception(string.Format("The table '{0}' contains no primary key!", tableName));
                            }
                        }
                        if (!table.AsEnumerable().Any(x => x["Column_name"].ToString() == columnName))
                            return false;
                    }
                }
                return true;
            }
            catch
            {
                throw;
            }
        }

    }
}
