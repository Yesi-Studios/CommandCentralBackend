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
using UnifiedServiceFramework.Framework;
using AtwoodUtils;

namespace CommandDB_Plugin.CustomAuthorization
{
    public static class CustomPermissions
    {

        /// <summary>
        /// This method returns a boolean indicating whether or not the client associated with a message token's session is considered an "admin" from the POV of a given person.
        /// <para />
        /// Returns a boolean indicating whether or not the client is in the chain of command of a given person.
        /// </summary>
        /// <param name="token"></param>
        /// <param name="personID"></param>
        /// <returns></returns>
        public static async Task<bool> IsClientInChainOfCommandOfPerson(List<UnifiedServiceFramework.Authorization.Permissions.PermissionGroup> clientPerms, string clientID, string personID)
        {
            try
            {

                //First, we can check if the person and client are the same.  If so, the answer is false, persons are not in their own chains of command.
                if (personID == clientID)
                    return false;


                var clientCustomPermissions = clientPerms.Select(x => x.CustomPermissions).SelectMany(x => x).Distinct().Select(x =>
                    {
                        CustomPermissionTypes customPermission;
                        if (!Enum.TryParse(x, false, out customPermission))
                            throw new Exception(string.Format("While attempting to parse the permission '{0}' into the custom permissions list, an error occurred.", x));

                        return customPermission;

                    }).ToList();

                Dictionary<string, Dictionary<string, string>> persons = new Dictionary<string, Dictionary<string, string>>();

                //First, we need to go get the division, department and command of the person.  
                using (MySqlConnection connection = new MySqlConnection(Properties.ConnectionString))
                {
                    await connection.OpenAsync();

                    using (MySqlCommand command = new MySqlCommand("", connection))
                    {
                        command.CommandText = "SELECT `ID`,`Division`,`Department`,`Command` FROM `persons_main` WHERE `ID` = @PersonID OR `ID` = @ClientID";

                        command.Parameters.AddWithValue("@PersonID", personID);
                        command.Parameters.AddWithValue("@ClientID", clientID);

                        using (MySqlDataReader reader = (MySqlDataReader)await command.ExecuteReaderAsync())
                        {
                            if (reader.HasRows)
                            {
                                while (await reader.ReadAsync())
                                {
                                    persons.Add(reader["ID"] as string, new Dictionary<string, string>()
                                    {
                                        { "Division", reader["Division"] as string },
                                        { "Department", reader["Department"] as string },
                                        { "Command", reader["Command"] as string }
                                    });
                                }
                            }
                            else
                            {
                                throw new Exception(string.Format("In the chain of command permissions check, we tried to load permissions for the users with the id '{0}' and '{1}' but no users were found.", personID, clientID));
                            }
                        }
                    }
                }

                //Make sure we only get two persons
                if (persons.Count != 2)
                    throw new Exception(string.Format("While loading the chain of command for the two users whose IDs are '{0}' and '{1}', we expected to get two users; however, we only get one.", personID, clientID));

                //Now that we have everything we need, let's start comparing some shit.

                //If the client is command leadership and the client and the person are in the same command, true
                if (clientCustomPermissions.Contains(CustomPermissionTypes.Command_Leadership)
                    && persons[clientID]["Command"].SafeEquals(persons[personID]["Command"]))
                    return true;

                if (clientCustomPermissions.Contains(CustomPermissionTypes.Department_Leadership)
                    && persons[clientID]["Command"].SafeEquals(persons[personID]["Command"])
                    && persons[clientID]["Department"].SafeEquals(persons[personID]["Department"]))
                    return true;

                if (clientCustomPermissions.Contains(CustomPermissionTypes.Division_Leadership)
                    && persons[clientID]["Command"].SafeEquals(persons[personID]["Command"])
                    && persons[clientID]["Department"].SafeEquals(persons[personID]["Department"])
                    && persons[clientID]["Division"].SafeEquals(persons[personID]["Division"]))
                    return true;

                return false;

            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Updates a person's profile with a new list of group permissions.  No authorization or validation occurs on this method and it should never be exposed directly to the client.
        /// </summary>
        /// <param name="personID"></param>
        /// <param name="permissionGroupIDs"></param>
        public static async Task SetUserPermissionGroups(string personID, List<string> permissionGroupIDs)
        {
            try
            {
                using (MySqlConnection connection = new MySqlConnection(Properties.ConnectionString))
                {
                    await connection.OpenAsync();

                    using (MySqlTransaction transaction = await connection.BeginTransactionAsync())
                    {
                        try
                        {

                            using (MySqlCommand command = new MySqlCommand("", connection, transaction))
                            {
                                command.CommandText = "UPDATE `persons_accounts` SET `PermissionGroupIDs` = @PermissionGroupIDs WHERE `ID` = @ID";

                                command.Parameters.AddWithValue("@PermissionGroupIDs", permissionGroupIDs.Serialize());
                                command.Parameters.AddWithValue("@ID", personID);

                                await command.ExecuteNonQueryAsync();
                            }

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
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Updates a person's profile with a new list of group permissions.  No authorization or validation occurs on this method and it should never be exposed directly to the client.
        /// <para />
        /// This version uses the passed transaction for DB interaction
        /// </summary>
        /// <param name="personID"></param>
        /// <param name="permissionGroupIDs"></param>
        public static async Task SetUserPermissionGroups(string personID, List<string> permissionGroupIDs, MySqlTransaction transaction)
        {
            try
            {
                using (MySqlCommand command = new MySqlCommand("", transaction.Connection, transaction))
                {
                    command.CommandText = "UPDATE `persons_accounts` SET `PermissionGroupIDs` = @PermissionGroupIDs WHERE `ID` = @ID";

                    command.Parameters.AddWithValue("@PermissionGroupIDs", permissionGroupIDs.Serialize());
                    command.Parameters.AddWithValue("@ID", personID);

                    await command.ExecuteNonQueryAsync();
                }
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Loads the user's permission groups from the database.  
        /// </summary>
        /// <param name="personID"></param>
        /// <returns></returns>
        public static async Task<List<UnifiedServiceFramework.Authorization.Permissions.PermissionGroup>> GetPermissionGroupsForUser(string personID)
        {
            try
            {

                using (MySqlConnection connection = new MySqlConnection(Properties.ConnectionString))
                {
                    await connection.OpenAsync();

                    MySqlCommand command = connection.CreateCommand();
                    command.CommandType = CommandType.Text;
                    command.CommandText = "SELECT `PermissionGroupIDs` FROM `persons_accounts` WHERE `ID` = @ID";

                    command.Parameters.AddWithValue("@ID", personID);

                    using (MySqlDataReader reader = (MySqlDataReader)await command.ExecuteReaderAsync())
                    {
                        if (reader.HasRows)
                        {
                            await reader.ReadAsync();

                            return UnifiedServiceFramework.Authorization.Permissions.TranslatePermissionGroupIDs((reader["PermissionGroupIDs"] as string).Deserialize<List<string>>());
                        }
                        else
                        {
                            throw new Exception(string.Format("While loading the permission IDs for a user ('{0}'), no permission IDs were found.", personID));
                        }
                    }
                }
            }
            catch
            {
                throw;
            }
        }

        
    }
}
