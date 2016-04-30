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
        /// Begins the registration process by sending an email to the account to whom the SSN belongs.  
        /// This email contains a confirmation key that has also been inserted into the database on the user's account.
        /// <para />
        /// If the account doesn't have an email address associated to it yet, throw an error.
        /// <para /> 
        /// If the account already has a username then we can assume that the account has already been claimed.  
        /// Therefore, trying to start the registration process for this account looks pretty bad and we send an email to a bunch of people to inform them that this happened.
        /// <para />
        /// Options: 
        /// <para />
        /// ssn : The SSN of the account that we are going to try to claim.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public static async Task<MessageTokens.MessageToken> BeginRegistration(MessageTokens.MessageToken token)
        {
            try
            {
                //Did the user send us an SSN?
                if (!token.Args.ContainsKey("ssn"))
                    throw new ServiceException("You must send an SSN.", ErrorTypes.Validation);
                string ssn = token.Args["ssn"].ToString();

                string username = "";
                string personID = "";

                //Ok, we need to go grab the ID and Username for the user account with the given SSN
                using (MySqlConnection connection = new MySqlConnection(Settings.ConnectionString))
                {
                    await connection.OpenAsync();

                    using (MySqlCommand command = new MySqlCommand("", connection))
                    {
                        command.CommandText = "SELECT `ID`,`Username` FROM `persons_accounts` JOIN `persons_main` USING (`ID`) WHERE `SSN` = @SSN";

                        command.Parameters.AddWithValue("SSN", ssn);

                        using (MySqlDataReader reader = (MySqlDataReader)await command.ExecuteReaderAsync())
                        {
                            if (reader.HasRows)
                            {
                                DataTable table = new DataTable();
                                table.Load(reader);

                                if (table.Rows.Count > 1)
                                    throw new Exception(string.Format("While beginning registration of the user account, multiple records were found to have the same SSN: '{0}'", ssn));

                                username = table.Rows[0]["Username"] as string;
                                personID = table.Rows[0]["ID"] as string;
                            }
                            else
                            {
                                throw new ServiceException("That SSN did not correspond to any user records.", ErrorTypes.Validation);
                            }
                        }
                    }
                }

                //Cast our values
                var person = new
                {
                    ID = personID,
                    Username = username,
                    DODEmailAddress = (await EmailAddresses.DBLoadAll(personID)).FirstOrDefault(x => x.IsDODEmailAddress)
                };

                //If the username isn't blank, it's because the account has already been claimed.  If that's the case, why is someone else trying to claim it?  That's really not good.
                if (!string.IsNullOrWhiteSpace(person.Username))
                {
                    await EmailHelper.SendBeginRegistrationErrorEmail(person.DODEmailAddress.Address, person.ID);

                    throw new ServiceException("A user has already claimed that account.  Your attempt to claim it appears suspicious and has been reported.", ErrorTypes.Authentication);
                }

                //Ok, so now we know that this account hasn't already been claimed, so that's good.  Now we need to validate the email address and then send the registration email.
                //We also need to insert into the Persons model the account confirmation information.
                if (person.DODEmailAddress == null)
                    throw new ServiceException("We found the account that is tied to that SSN; however, it appears that the DOD email address assigned to it is invalid.  Please make sure that Admin or IMO has updated your account with its email address.", ErrorTypes.Validation);

                //Ok the email address is legit.  Now let's input the information into the database.
                //This is the account confirmation id that's going to be sent to the client.
                string confirmationID = Guid.NewGuid().ToString();

                //Do the insert
                using (MySqlConnection connection = new MySqlConnection(Properties.ConnectionString))
                {
                    await connection.OpenAsync();

                    using (MySqlTransaction transaction = await connection.BeginTransactionAsync())
                    {
                        try
                        {

                            using (MySqlCommand command = new MySqlCommand("", connection, transaction))
                            {
                                command.CommandText = "UPDATE `persons_accounts` SET `AccountConfirmationID` = @AccountConfirmationID WHERE `ID` = @ID";

                                command.Parameters.AddWithValue("@AccountConfirmationID", confirmationID);
                                command.Parameters.AddWithValue("@ID", person.ID);

                                await command.ExecuteNonQueryAsync();
                            }

                            //And now we need to log this account history event.  We're going to do it here so we can use this transaction.
                            await new AccountHistoryEvents.AccountHistoryEvent()
                            {
                                AccountHistoryEventType = AccountHistoryEventTypes.Registration_Started,
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

                //Now that the database knows what the account confirmation ID is, we just need to inform the user!
                await EmailHelper.SendConfirmAccountEmail(person.DODEmailAddress.Address, confirmationID, ssn);

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
        /// Completes the registration process by assigning a username and password to a user's account, thus allowing the user to claim the account.  If the account confirmation id isn't valid,
        /// <para />
        /// an error is thrown.  The could happen if begin registration hasn't happened yet.
        /// <para />
        /// Options: 
        /// <para />
        /// username : the username the client wants to assign to the account.  Usernames must be longer than 6 characters.
        /// <para />
        /// password : the password the client wants to assign to the account.  Passwords must be longer than 6 characters.
        /// <para />
        /// accountconfirmationid : The unique ID that was sent to the user's email address by the begin registration endpoint.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public static async Task<MessageTokens.MessageToken> CompleteRegistration_Client(MessageTokens.MessageToken token)
        {
            try
            {
                //Did we get all our required arguments?
                if (!token.Args.ContainsKey("username"))
                    throw new ServiceException("You must send a username.", ErrorTypes.Validation);

                if (!token.Args.ContainsKey("password"))
                    throw new ServiceException("You must send a password.", ErrorTypes.Validation);

                if (!token.Args.ContainsKey("accountconfirmationid"))
                    throw new ServiceException("You must send an account confirmation id!", ErrorTypes.Validation);

                //Put them in variables for easier use.
                var clientPerson = new
                {
                    Username = token.Args["username"] as string,
                    Password = token.Args["password"] as string,
                    AccountConfirmationID = token.Args["accountconfirmationid"] as string
                };

                //Validate them
                if (!ValidationMethods.IsValidUsername(clientPerson.Username))
                    throw new ServiceException("Usernames must be length = [6,20] and be only letters or digits.", ErrorTypes.Validation);

                if (!ValidationMethods.IsValidPassword(clientPerson.Password))
                    throw new ServiceException("Passwords must be length = [6,40].", ErrorTypes.Validation);

                if (await Persons.DoesUsernameExist(clientPerson.Username))
                    throw new ServiceException("That username already exists!", ErrorTypes.Validation);

                //Alright, now we can do the update.  We're going to set the desired password onto the profile and the username and set the account to claimed.
                //We're also going to delete the confirmation ID, making the profile unclaimable in the future - hopefully.
                //We also can't use the Persons class because it doesn't expose access to secure fields, such as SecurePassword.
                //If this doesn't update anything, then the account confirmation ID isn't legit.
                using (MySqlConnection connection = new MySqlConnection(Properties.ConnectionString))
                {
                    await connection.OpenAsync();

                    using (MySqlTransaction transaction = await connection.BeginTransactionAsync())
                    {
                        try
                        {

                            //Let's go get the person's ID that owns this account confirmation ID.  This will serve to validate the account confirmation ID and give us the user's ID so we can log it later.
                            //During this load, we're also going to get the other information we need to validate that registration can be completed on this account.
                            string personID = null;
                            using (MySqlCommand command = new MySqlCommand("", connection, transaction))
                            {
                                command.CommandText = "SELECT `ID`, `Username`, `SecurePassword` FROM `persons_account` WHERE `AccountConfirmationID` = @AccountConfirmationID";

                                command.Parameters.AddWithValue("@AccountConfirmationID", clientPerson.AccountConfirmationID);

                                using (MySqlDataReader reader = (MySqlDataReader)await command.ExecuteReaderAsync())
                                {
                                    if (reader.HasRows)
                                    {
                                        DataTable table = new DataTable();
                                        table.Load(reader);
                                        
                                        //Make sure this account confirmation ID isn't on multiple profiles.
                                        if (table.Rows.Count > 1)
                                            throw new Exception(string.Format("While completing registration for the account confirmation ID, '{0}', this ID was found on multiple accounts.", clientPerson.AccountConfirmationID));

                                        //Now we know that we have a single row.  Now let's make sure the Username is blank and the password is blank.  If they're not blank, it's because the account is already claimed by someone.
                                        if (!string.IsNullOrWhiteSpace(table.Rows[0]["Username"] as string) || !string.IsNullOrWhiteSpace(table.Rows[0]["SecurePassword"] as string))
                                            throw new Exception(string.Format("During complete registration, the account confirmation ID, '{0}', was used to try to claim an account that appears to already be claimed.", clientPerson.AccountConfirmationID));

                                        //Now that everything looks good, let's get this person ID.
                                        personID = table.Rows[0]["ID"] as string;
                                    }
                                    else
                                    {
                                        throw new ServiceException(string.Format("The account confirmation ID, '{0}', was not valid.", clientPerson.AccountConfirmationID), ErrorTypes.Validation);
                                    }
                                }
                            }

                            //No we're going to set the username and secure password to what the client wants.
                            //We're also going to set IsClaimed to true.
                            //Based on the validaiton we did above, we're going to use the person ID to do this set.
                            using (MySqlCommand command = new MySqlCommand("", connection, transaction))
                            {
                                command.CommandText = "UPDATE `persons_accounts` SET `Username` = @Username, `SecurePassword` = @SecurePassword, " +
                                                      "`AccountConfirmationID` = NULL, `IsClaimed` = @IsClaimed WHERE `ID` = @ID";

                                command.Parameters.AddWithValue("@Username", clientPerson.Username);
                                command.Parameters.AddWithValue("@SecurePassword", PasswordHash.CreateHash(clientPerson.Password));
                                command.Parameters.AddWithValue("@IsClaimed", true);

                                command.Parameters.AddWithValue("@ID", personID);
                            }

                            //Now log this account history event.
                            await new AccountHistoryEvents.AccountHistoryEvent()
                            {
                                AccountHistoryEventType = AccountHistoryEventTypes.Registration_Completed,
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
