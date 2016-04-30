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

namespace CommandCentral
{
    /// <summary>
    /// Provides mostly client methods for interacting with the user's account including Register and Login.
    /// </summary>
    public static class AccountServices
    {

        
        /// <summary>
        /// WARNING!  THIS IS A CLIENT METHOD.  AUTHENTICATION, AUTHORIZATION AND VALIDATION MUST BE HANDLED PRIOR TO DB INTERACTION.
        /// <para />
        /// In order to start a password reset, we're going to use the given email address and ssn to load the person's is claimed field.  If is claimed is false, 
        /// then the account hasn't been claimed yet and you can't reset the password.  If the account is ready to have its password reset, then we set the reset password id
        /// on the user's profile and then send the relevant email.
        /// <para />
        /// Options: 
        /// <para />
        /// email : The email address of the account we want to reset
        /// ssn : The SSN of the account we want to reset.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public static async Task<MessageTokens.MessageToken> InitiatePasswordReset(MessageTokens.MessageToken token)
        {
            try
            {
                //Make sure we got what we need.
                if (!token.Args.ContainsKey("email"))
                    throw new ServiceException("You must send a dod email address.", ErrorTypes.Validation);

                if (!token.Args.ContainsKey("ssn"))
                    throw new ServiceException("You must send an SSN.", ErrorTypes.Validation);

                string dodEmailAddress = token.Args["email"] as string;
                if (!ValidationMethods.IsValidDODEmailAddress(dodEmailAddress))
                    throw new ServiceException(string.Format("The email address you sent, '{0}', must be a valid DOD Email Address", dodEmailAddress), ErrorTypes.Validation);

                string ssn = token.Args["ssn"] as string;
                if (!ValidationMethods.IsValidSSN(ssn))
                    throw new ServiceException(string.Format("The SSN you sent, '{0}', was not a valid SSN.", ssn), ErrorTypes.Validation);

                string personID = "";
                bool isClaimed = false;

                //Alright, we need to go get the person's ID to whom the email address and SSN belong.  To do this we'll need to load from a couple different places at the same time.
                //First off, let's see if the SSN belongs to a profile
                using (MySqlConnection connection = new MySqlConnection(Properties.ConnectionString))
                {
                    await connection.OpenAsync();

                    using (MySqlCommand command = new MySqlCommand("", connection))
                    {
                        command.CommandText = "SELECT `ID`,`IsClaimed` FROM `persons_accounts` JOIN `persons_main` USING (`ID`) WHERE `SSN` = @SSN";

                        command.Parameters.AddWithValue("@SSN", ssn);

                        using (MySqlDataReader reader = (MySqlDataReader)await command.ExecuteReaderAsync())
                        {
                            if (reader.HasRows)
                            {
                                DataTable table = new DataTable();
                                table.Load(reader);

                                if (table.Rows.Count > 1)
                                    throw new Exception(string.Format("While beginning the password reset process, multiple records were loaded for the ssn '{0}'!", ssn));

                                personID = table.Rows[0]["ID"] as string;
                                isClaimed = Convert.ToBoolean(table.Rows[0]["IsClaimed"]);
                            }
                            else
                            {
                                throw new ServiceException("No user account exists for that ssn.", ErrorTypes.Validation);
                            }
                        }
                    }
                }

                //Ok since we know we only got one, let's make an object for this all.
                var person = new
                {
                    ID = personID,
                    IsClaimed = isClaimed,
                    SSN = ssn,
                    DODEmailAddress = (await EmailAddresses.DBLoadAll(personID)).FirstOrDefault(x => x.IsDODEmailAddress)
                };

                //Check to make sure we are ready.
                if (!person.IsClaimed || person.DODEmailAddress == null)
                    throw new ServiceException("The account can not have its password reset.  It either does not have a valid dod email address, or has yet to be claimed.", ErrorTypes.Validation);

                string passwordResetID = Guid.NewGuid().ToString();

                //Since the account is ready to have its password reset, we're going to "tag" the account with a password reset ID to be used in the complete password reset.
                using (MySqlConnection connection = new MySqlConnection(Properties.ConnectionString))
                {
                    await connection.OpenAsync();

                    using (MySqlTransaction transaction = await connection.BeginTransactionAsync())
                    {
                        try
                        {

                            using (MySqlCommand command = new MySqlCommand("", connection, transaction))
                            {
                                command.CommandText = "UPDATE `persons_accounts` SET `PasswordResetID` = @PasswordResetID WHERE `ID` = @ID";

                                command.Parameters.AddWithValue("@PasswordResetID", passwordResetID);
                                command.Parameters.AddWithValue("@ID", person.ID);

                                await command.ExecuteNonQueryAsync();
                            }

                            //Now log this account history event.
                            await new AccountHistoryEvents.AccountHistoryEvent()
                            {
                                AccountHistoryEventType = AccountHistoryEventTypes.Password_Reset_Initiated,
                                EventTime = token.CallTime,
                                ID = Guid.NewGuid().ToString(),
                                PersonID = person.ID
                            }.DBInsert(transaction);

                            transaction.Commit();
                        }
                        catch
                        {
                            transaction.Rollback();
                            throw;
                        }
                    }
                }

                await EmailHelper.SendInitiatePasswordResetEmail(passwordResetID, person.DODEmailAddress.Address);

                token.Result = "Success";

                return token;

            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// WARNING!  THIS IS A CLIENT METHOD.  AUTHENTICATION, AUTHORIZATION AND VALIDATION MUST BE HANDLED PRIOR TO DB INTERACTION.
        /// <para />
        /// Completes a password reset by updating the password to the given password for the given reset password id.
        /// <para />
        /// Options: 
        /// <para />
        /// passwordResetID : The reset password id that was emailed to the client during the start password reset endpoint.
        /// password : The password the client wants the account to have.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public static async Task<MessageTokens.MessageToken> FinishPasswordReset(MessageTokens.MessageToken token)
        {
            try
            {
                //Make sure we got all the arguments we need
                if (!token.Args.ContainsKey("passwordresetid"))
                    throw new ServiceException("You must send the password reset ID.", ErrorTypes.Validation);

                if (!token.Args.ContainsKey("password"))
                    throw new ServiceException("You must send a new password.", ErrorTypes.Validation);

                //Validate the password and create the hash
                string password = token.Args["password"].ToString();
                if (!ValidationMethods.IsValidPassword(password))
                    throw new ServiceException("The password you sent is not a valid password.", ErrorTypes.Validation);
                string passwordHash = PasswordHash.CreateHash(password);

                //Get and validate the password reset ID
                string passwordResetID = token.Args["passwordresetid"] as string;
                if (!ValidationMethods.IsValidGuid(passwordResetID))
                    throw new ServiceException(string.Format("The password reset ID that you sent ('{0}') was not in a valid format!", passwordResetID), ErrorTypes.Validation);

                //Set the password to the one the client wants.
                using (MySqlConnection connection = new MySqlConnection(Properties.ConnectionString))
                {
                    await connection.OpenAsync();

                    using (MySqlTransaction transaction = await connection.BeginTransactionAsync())
                    {
                        try
                        {

                            string personID = null;
                            //We're going to go load the profile that owns this password reset ID.
                            //This will let us validate the password reset ID and also give us the person's ID for logging purposes.
                            using (MySqlCommand command = new MySqlCommand("", connection, transaction))
                            {
                                command.CommandText = "SELECT `ID` FROM `persons_account` WHERE `PasswordResetID` = @PasswordResetID";

                                command.Parameters.AddWithValue("@PasswordResetID", passwordResetID);

                                using (MySqlDataReader reader = (MySqlDataReader)await command.ExecuteReaderAsync())
                                {
                                    if (reader.HasRows)
                                    {
                                        DataTable table = new DataTable();
                                        table.Load(reader);

                                        if (table.Rows.Count > 1)
                                            throw new Exception(string.Format("While attempting to complete password reset for the password reset ID, '{0}', the ID was found on more than one profile.", passwordResetID));

                                        //Now we know only one profile has this password reset ID, so let's save the person's ID.
                                        personID = table.Rows[0]["ID"] as string;
                                    }
                                    else
                                    {
                                        throw new ServiceException(string.Format("The password reset ID, '{0}', was not valid.", passwordResetID), ErrorTypes.Validation);
                                    }
                                }
                            }

                            //Now we can carry out the update.
                            using (MySqlCommand command = new MySqlCommand("", connection, transaction))
                            {
                                command.CommandText = "UPDATE `persons_accounts` SET `SecurePassword` = @SecurePassword WHERE `ID` = @ID";

                                command.Parameters.AddWithValue("@SecurePassword", passwordHash);
                                command.Parameters.AddWithValue("@ID", personID);

                                int rowsAffected = await command.ExecuteNonQueryAsync();
                            }

                            //Now log this account history event.
                            await new AccountHistoryEvents.AccountHistoryEvent()
                            {
                                AccountHistoryEventType = AccountHistoryEventTypes.Password_Reset_Initiated,
                                EventTime = token.CallTime,
                                ID = Guid.NewGuid().ToString(),
                                PersonID = personID
                            }.DBInsert(transaction);

                            transaction.Commit();
                        }
                        catch
                        {
                            transaction.Rollback();
                            throw;
                        }
                    }
                }

                token.Result = "Success";

                return token;
            
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Returns a boolean indicating whether or not a password is valid for a given account.
        /// </summary>
        /// <param name="personID"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public static async Task<bool> IsPasswordValidForAccount(string personID, string password)
        {
            try
            {
                using (MySqlConnection connection = new MySqlConnection(Properties.ConnectionString))
                {
                    await connection.OpenAsync();

                    using (MySqlCommand command = new MySqlCommand("", connection))
                    {
                        command.CommandText = "SELECT `SecurePassword` FROM `persons_accounts` WHERE `ID` = @ID";

                        command.Parameters.AddWithValue("@ID", personID);

                        using (MySqlDataReader reader = (MySqlDataReader)await command.ExecuteReaderAsync())
                        {
                            if (reader.HasRows)
                            {
                                await reader.ReadAsync();

                                return PasswordHash.ValidatePassword(password, reader["SecurePassword"] as string);
                            }
                            else
                            {
                                throw new ServiceException(string.Format("The person ID, '{0}', was not valid.", personID), ErrorTypes.Validation);
                            }
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
