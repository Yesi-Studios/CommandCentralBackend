using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AtwoodUtils;

namespace CommandCentralHost.Editors
{

    /// <summary>
    /// Provides interface to manipulate the database schema.
    /// </summary>
    public static class SchemaEditor
    {
        /// <summary>
        /// Asks the user to create the schema and whether to drop the schema first.
        /// </summary>
        public static void CreateSchema()
        {
            Console.Clear();

            "Would you like to drop the current schema first? (y) (Note: creating the schema where tables already exist will cause unknown behavior.)".WriteLine();

            bool dropFirst = false;
            if (Console.ReadLine().ToLower() == "y")
                dropFirst = true;

            "Are you sure you want to run the schema generation script? (y)".WriteLine();

            if (Console.ReadLine().ToLower() == "y")
            {
                CommandCentral.DataAccess.NHibernateHelper.CreateSchema(dropFirst);
            }

        }
    }
}
