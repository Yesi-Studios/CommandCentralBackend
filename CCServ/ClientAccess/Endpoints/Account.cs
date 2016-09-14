﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CCServ.Authorization;
using CCServ.DataAccess;
using CCServ.Entities;
using NHibernate.Transform;

namespace CCServ.ClientAccess.Endpoints
{
    static class Account
    {
        #region Login/Logout

        /// <summary>
        /// WARNING!  THIS METHOD IS EXPOSED TO THE CLIENT AND IS NOT INTENDED FOR INTERNAL USE.  AUTHENTICATION, AUTHORIZATION AND VALIDATION MUST BE HANDLED PRIOR TO DB INTERACTION.
        /// <para />
        /// Logs in the user by searching for the user's username and then checking to ensure that the password matches the password given.  
        /// Passwords are compared using a slow equals to defend against server timing attacks.
        /// <para />
        /// If the username or password are not given or not correct, exceptions are thrown.
        /// <para />
        /// Additionally, if the password is incorrect then we also send an email to the user informing them that a failed login attempt occurred.
        /// <para />
        /// If the username/password combo is good, we create a new session, insert it into the database and add it to the cache.
        /// <para />
        /// Finally, we return the session id to be used as the authentication token for further requests along with some other information.
        /// <para/>
        /// Client Parameters: <para />
        ///     Username - The username of the account for which to attempt to login as. <para />
        ///     Password - The plain text password related to the same account as the username.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        [EndpointMethod(EndpointName = "Login", AllowArgumentLogging = false, AllowResponseLogging = true, RequiresAuthentication = false)]
        private static void EndpointMethod_Login(MessageToken token)
        {
            //Let's see if the parameters are here.
            if (!token.Args.ContainsKey("username"))
                token.AddErrorMessage("You didn't send a 'username' parameter.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);

            if (!token.Args.ContainsKey("password"))
                token.AddErrorMessage("You didn't send a 'password' parameter.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);

            if (!token.HasError)
            {

                //If the token has no error then we need a session and a transaction
                using (var session = NHibernateHelper.CreateStatefulSession())
                using (var transaction = session.BeginTransaction())
                {
                    try
                    {
                        string username = token.Args["username"] as string;
                        string password = token.Args["password"] as string;

                        //The query itself.  Note that SingleOrDefault will throw an exception if more than one person comes back.
                        //This is ok because the username field is marked unique so this shouldn't happen and if it does then we want an exception.
                        var person = session.QueryOver<Person>()
                            .Where(x => x.Username == username)
                            .SingleOrDefault<Person>();

                        if (person == null)
                        {
                            token.AddErrorMessage("Either the username or password is wrong.", ErrorTypes.Authentication, System.Net.HttpStatusCode.Forbidden);
                            return;
                        }

                        if (!ClientAccess.PasswordHash.ValidatePassword(password, person.PasswordHash))
                        {
                            //A login to the client's account failed.  We need to send an email.
                            if (!person.EmailAddresses.Any())
                                throw new Exception(string.Format("Login failed to the person's account whose Id is '{0}'; however, we could find no email to send this person a warning.", person.Id));

                            var model = new Email.Models.FailedAccountLoginEmailModel
                            {
                                FriendlyName = person.ToString()
                            };

                            //Ok, so we have an email we can use to contact the person!
                            Email.EmailInterface.CCEmailMessage
                                .CreateDefault()
                                .To(person.EmailAddresses.Select(x => new System.Net.Mail.MailAddress(x.Address, person.ToString())))
                                .Subject("Security Alert : Failed Login")
                                .HTMLAlternateViewUsingTemplateFromEmbedded("CCServ.Email.Templates.FailedAccountLogin_HTML.html", model)
                                .SendWithRetryAndFailure(TimeSpan.FromSeconds(1));

                            //Put the error in the token and shake it all up.
                            token.AddErrorMessage("Either the username or password is wrong.", ErrorTypes.Authentication, System.Net.HttpStatusCode.Forbidden);

                            //Now we also need to add the event to client's account history.
                            person.AccountHistory.Add(new AccountHistoryEvent
                            {
                                AccountHistoryEventType = Entities.ReferenceLists.AccountHistoryTypes.FailedLogin,
                                EventTime = token.CallTime
                            });

                            session.Save(person);

                        }
                        else
                        {
                            //Cool then we can make a new authentication session.
                            AuthenticationSession ses = new AuthenticationSession
                            {
                                IsActive = true,
                                LastUsedTime = token.CallTime,
                                LoginTime = token.CallTime,
                                Person = person
                            };

                            //Now insert it
                            session.Save(ses);

                            //Also put the account history on the client.
                            person.AccountHistory.Add(new AccountHistoryEvent
                            {
                                AccountHistoryEventType = Entities.ReferenceLists.AccountHistoryTypes.Login,
                                EventTime = token.CallTime
                            });

                            //We need to get the client's permission groups, add the defaults, and then tell the client their permissions.
                            token.SetResult(new { PersonId = person.Id, ResolvedPermissions = AuthorizationUtilities.GetPermissionGroupsFromNames(person.PermissionGroupNames, true).Resolve(person, null), AuthenticationToken = ses.Id, FriendlyName = person.ToString() });
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

        /// <summary>
        /// WARNING!  THIS METHOD IS EXPOSED TO THE CLIENT AND IS NOT INTENDED FOR INTERNAL USE.  AUTHENTICATION, AUTHORIZATION AND VALIDATION MUST BE HANDLED PRIOR TO DB INTERACTION.
        /// <para />
        /// Logs out the user by invalidating the session/deleted it from the database.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        [EndpointMethod(EndpointName = "Logout", AllowArgumentLogging = true, AllowResponseLogging = true, RequiresAuthentication = true)]
        private static void EndpointMethod_Logout(MessageToken token)
        {
            //Just make sure the client is logged in.  The endpoint's description should've handled this but you never know.
            if (token.AuthenticationSession == null)
            {
                token.AddErrorMessage("You must be logged in to update the news.", ErrorTypes.Authentication, System.Net.HttpStatusCode.Unauthorized);
                return;
            }

            token.AuthenticationSession.IsActive = false;

            //Cool, we also need to update the client.
            token.AuthenticationSession.Person.AccountHistory.Add(new AccountHistoryEvent
            {
                AccountHistoryEventType = Entities.ReferenceLists.AccountHistoryTypes.Logout,
                EventTime = token.CallTime
            });
        }

        #endregion

        #region Registration

        /// <summary>
        /// WARNING!  THIS METHOD IS EXPOSED TO THE CLIENT AND IS NOT INTENDED FOR INTERNAL USE.  AUTHENTICATION, AUTHORIZATION AND VALIDATION MUST BE HANDLED PRIOR TO DB INTERACTION.
        /// <para />
        /// <para />
        /// Begins the registration process by sending an email to the account to whom the SSN belongs.  
        /// This email contains a confirmation key that has also been inserted into the database.
        /// <para />
        /// If the account doesn't have an email address associated to it yet, throw an error.
        /// <para /> 
        /// If the account is already claimed, then trying to start the registration process for this account looks pretty bad and we send an email to a bunch of people to inform them that this happened.
        /// <para />
        /// Client Parameters: <para />
        ///     SSN - The ssn that belongs to the account for which the client wants to start the registration process.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        [EndpointMethod(EndpointName = "BeginRegistration", AllowArgumentLogging = true, AllowResponseLogging = true, RequiresAuthentication = false)]
        private static void EndpointMethod_BeginRegistration(MessageToken token)
        {

            //Let's do our work in a new session so that we don't affect the authentication information.
            using (var session = NHibernateHelper.CreateStatefulSession())
            using (var transaction = session.BeginTransaction())
            {
                try
                {

                    session.FlushMode = NHibernate.FlushMode.Always;

                    //First off, we need the link that the client wants us to use to finish registration.
                    if (!token.Args.ContainsKey("continuelink"))
                    {
                        token.AddErrorMessage("You failed to send a 'continuelink' parameter!", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                        return;
                    }
                    string continueLink = token.Args["continueLink"] as string;

                    //Let's just do some basic validation and make sure it's a real URI.
                    if (!Uri.IsWellFormedUriString(continueLink, UriKind.Absolute))
                    {
                        token.AddErrorMessage("The continue link you sent was not a valid URI.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                        return;
                    }

                    //We also need the client's ssn.  This is the account they want to claim.
                    if (!token.Args.ContainsKey("ssn"))
                    {
                        token.AddErrorMessage("You didn't send a 'ssn' parameter.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                        return;
                    }

                    string ssn = token.Args["ssn"] as string;

                    //The query itself.  Note that SingleOrDefault will throw an exception if more than one person comes back.
                    //This is ok because the ssn field is marked unique so this shouldn't happen and if it does then we want an exception.
                    var person = session.QueryOver<Person>()
                        .Where(x => x.SSN == ssn)
                        .SingleOrDefault<Person>();

                    //If no result came back, this will be null
                    if (person == null)
                    {
                        token.AddErrorMessage("That ssn belongs to no profile.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                        return;
                    }

                    //Ok, so we have a single profile.  Let's see if it's already been claimed.
                    if (person.IsClaimed)
                    {
                        //If the profile is already claimed that's a big issue.  That means someone is trying to reclaim it.  It's send some emails.

                        if (!person.EmailAddresses.Any())
                            throw new Exception(string.Format("Another user tried to claim the profile whose Id is '{0}'; however, we could find no email to send this person a warning.", person.Id));

                        var beginRegModel = new Email.Models.BeginRegistrationErrorEmailModel
                        {
                            FriendlyName = person.ToString()
                        };

                        Email.EmailInterface.CCEmailMessage
                            .CreateDefault()
                            .CC(Config.Email.DeveloperDistroAddress)
                            .To(person.EmailAddresses.Select(x => new System.Net.Mail.MailAddress(x.Address, person.ToString())))
                            .Subject("Security Alert : Reregistration Attempt")
                            .HTMLAlternateViewUsingTemplateFromEmbedded("CCServ.Email.Templates.ReregistrationError_HTML.html", beginRegModel)
                            .SendWithRetryAndFailure(TimeSpan.FromSeconds(1));

                        token.AddErrorMessage("A user has already claimed that account.  That user has been notified of your attempt to claim the account." +
                                              "  If you believe this is in error or if you are the rightful owner of this account, please call the development team immediately.", ErrorTypes.Validation, System.Net.HttpStatusCode.Forbidden);
                        return;
                    }

                    //If we get here, then the account isn't claimed.  Now we need a DOD email address to send the account verification email to.
                    var dodEmailAddress = person.EmailAddresses.FirstOrDefault(x => x.IsDodEmailAddress);
                    if (dodEmailAddress == null)
                    {
                        token.AddErrorMessage("We were unable to start the registration process because it appears your profile has no DOD email address (@mail.mil) assigned to it." +
                                              "  Please make sure that Admin or IMO has updated your account with your email address.", ErrorTypes.Validation, System.Net.HttpStatusCode.Forbidden);
                        return;
                    }

                    //Well, looks like we have a DOD email address and there are no old pending account confirmations sitting in the database.  Let's make an account confirmation... thing.
                    var pendingAccountConfirmation = new PendingAccountConfirmation
                    {
                        Person = person,
                        Time = token.CallTime,
                        Id = Guid.NewGuid()
                    };

                    //Let's see if there is already a pending account confirmation.
                    var existingPendingAccountConfirmation = session.QueryOver<PendingAccountConfirmation>()
                        .Where(x => x.Person.Id == person.Id)
                        .SingleOrDefault();

                    //if one already exists, update it, else make a new one.
                    if (existingPendingAccountConfirmation != null)
                    {
                        session.Delete(existingPendingAccountConfirmation);
                        session.Flush();
                    }

                    //Then, persist it if there isn't one yet.
                    session.Save(pendingAccountConfirmation);

                    //Let's also add the account history object here.
                    person.AccountHistory.Add(new AccountHistoryEvent
                    {
                        AccountHistoryEventType = Entities.ReferenceLists.AccountHistoryTypes.RegistrationStarted,
                        EventTime = token.CallTime
                    });

                    //And then persist that by updating the person.
                    session.Update(person);

                    //Wait!  we're not even done yet.  Let's send the client the registration email now.
                    var model = new Email.Models.AccountConfirmationEmailModel
                    {
                        ConfirmationId = pendingAccountConfirmation.Id,
                        ConfirmEmailAddressLink = continueLink,
                        FriendlyName = person.ToString().Trim()
                    };

                    Email.EmailInterface.CCEmailMessage
                            .CreateDefault()
                            .To(person.EmailAddresses.Select(x => new System.Net.Mail.MailAddress(x.Address, person.ToString())))
                            .Subject("Confirm Command Central Account")
                            .HTMLAlternateViewUsingTemplateFromEmbedded("CCServ.Email.Templates.AccountConfirmation_HTML.html", model)
                            .SendWithRetryAndFailure(TimeSpan.FromSeconds(1));

                    //Ok, Jesus Christ.  I think we're finally done.
                    token.SetResult("Success");

                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        /// <summary>
        /// WARNING!  THIS METHOD IS EXPOSED TO THE CLIENT AND IS NOT INTENDED FOR INTERNAL USE.  AUTHENTICATION, AUTHORIZATION AND VALIDATION MUST BE HANDLED PRIOR TO DB INTERACTION.
        /// <para />
        /// Completes the registration process by assigning a username and password to a user's account, thus allowing the user to claim the account.  If the account confirmation id isn't valid,
        /// <para />
        /// an error is thrown.  The could happen if begin registration hasn't happened yet.
        /// <para />
        /// Client Parameters: <para />
        ///     Username - The username the client wants to assign to the account. <para />
        ///     Password - The password the client wants to assign to the account. <para />
        ///     AccountConfirmationId - The unique Id that was sent to the user's email address by the begin registration endpoint.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        [EndpointMethod(EndpointName = "CompleteRegistration", AllowArgumentLogging = true, AllowResponseLogging = true, RequiresAuthentication = false)]
        private static void EndpointMethod_CompleteRegistration(MessageToken token)
        {
            //First, let's make sure the args are present.
            if (!token.Args.ContainsKey("username"))
                token.AddErrorMessage("You didn't send a 'username' parameter.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
            if (!token.Args.ContainsKey("password"))
                token.AddErrorMessage("You didn't send a 'password' parameter.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
            if (!token.Args.ContainsKey("accountconfirmationid"))
                token.AddErrorMessage("You didn't send a 'accountconfirmationid' parameter.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);

            //If there were any errors from the above checks, then stop now.
            if (token.HasError)
                return;

            string username = token.Args["username"] as string;
            string password = token.Args["password"] as string;
            Guid accountConfirmationId;
            if (!Guid.TryParse(token.Args["accountconfirmationid"] as string, out accountConfirmationId))
            {
                token.AddErrorMessage("The account confirmation ID you sent was not in the right format.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                return;
            }

            //Now we're going to try to find a pending account confirmation for the Id the client gave us.  
            //If we find one, we're going to look at the time on it and make sure it is still valid.
            //If it is, then we'll look up the person and make sure the person hasn't already been claimed. (that shouldn't be possible given the order of events but we'll throw an error anyways)
            //If the client isn't claimed, we'll set the username and password on the profile to what the client wants, switch IsClaimed to true, and delete the pending account confirmation.
            //Finally, we'll update the profile with an account history event.
            //Then return. ... fuck, ok, here we go.

            using (var session = NHibernateHelper.CreateStatefulSession())
            using (var transaction = session.BeginTransaction())
            {
                try
                {
                    var pendingAccountConfirmation = session.Get<PendingAccountConfirmation>(accountConfirmationId);
                    if (pendingAccountConfirmation == null)
                    {
                        token.AddErrorMessage("For the account confirmation Id that you provided, no account registration process has been started.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                        return;
                    }

                    //Is the record valid?
                    if (!pendingAccountConfirmation.IsValid())
                    {
                        //If not we need to delete the record and then tell the client to start over.
                        session.Delete(pendingAccountConfirmation);

                        token.AddErrorMessage("It appears you waited too long to register your account and it has become inactive!  Please restart the registration process.", ErrorTypes.Validation, System.Net.HttpStatusCode.Forbidden);
                        return;
                    }

                    //Ok now that we know the record is valid, let's see if the person is already claimed.  This is exceptional.
                    if (pendingAccountConfirmation.Person.IsClaimed)
                        throw new Exception("During complete registration, a valid pending account registration object was created for an already claimed account.");

                    //Alright, we're ready to update the person then!
                    pendingAccountConfirmation.Person.Username = username;
                    pendingAccountConfirmation.Person.PasswordHash = ClientAccess.PasswordHash.CreateHash(password);
                    pendingAccountConfirmation.Person.IsClaimed = true;

                    //Also put the account history object on the person.
                    pendingAccountConfirmation.Person.AccountHistory.Add(new AccountHistoryEvent
                    {
                        AccountHistoryEventType = Entities.ReferenceLists.AccountHistoryTypes.RegistrationCompleted,
                        EventTime = token.CallTime
                    });

                    //Cool, so now just update the person object.
                    session.Update(pendingAccountConfirmation.Person);

                    //Send the email to the client telling them we're done.
                    var model = new Email.Models.CompletedAccountRegistrationEmailModel
                    {
                        FriendlyName = pendingAccountConfirmation.Person.ToString()
                    };

                    Email.EmailInterface.CCEmailMessage
                        .CreateDefault()
                        .To(pendingAccountConfirmation.Person.EmailAddresses.Select(x => new System.Net.Mail.MailAddress(x.Address, pendingAccountConfirmation.Person.ToString())))
                        .Subject("Account Registered!")
                        .HTMLAlternateViewUsingTemplateFromEmbedded("CCServ.Email.Templates.CompletedAccountRegistration_HTML.html", model)
                        .SendWithRetryAndFailure(TimeSpan.FromSeconds(1));

                    //Now delete the pending account confirmation.  We don't need it anymore.
                    session.Delete(pendingAccountConfirmation);

                    token.SetResult("Success");

                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        #endregion

        #region Password Reset

        /// <summary>
        /// WARNING!  THIS METHOD IS EXPOSED TO THE CLIENT AND IS NOT INTENDED FOR INTERNAL USE.  AUTHENTICATION, AUTHORIZATION AND VALIDATION MUST BE HANDLED PRIOR TO DB INTERACTION.
        /// <para />
        /// In order to start a password reset, we're going to use the given email address and ssn to load the person's is claimed field.  If is claimed is false, 
        /// then the account hasn't been claimed yet and you can't reset the password.  
        /// <para />
        /// Client Parameters: <para />
        ///     email : The email address of the account we want to reset <para/>
        ///     ssn : The SSN of the account we want to reset.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        [EndpointMethod(EndpointName = "BeginPasswordReset", AllowArgumentLogging = true, AllowResponseLogging = true, RequiresAuthentication = false)]
        private static void EndpointMethod_BeginPasswordReset(MessageToken token)
        {
            //First, let's make sure the args are present.
            if (!token.Args.ContainsKey("email"))
                token.AddErrorMessage("You didn't send a 'email' parameter.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
            if (!token.Args.ContainsKey("ssn"))
                token.AddErrorMessage("You didn't send a 'ssn' parameter.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);

            //If there were any errors from the above checks, then stop now.
            if (token.HasError)
                return;

            string email = token.Args["email"] as string;
            string ssn = token.Args["ssn"] as string;

            //Let's validate the email.  
            System.Net.Mail.MailAddress mailAddress = null;
            try
            {
                mailAddress = new System.Net.Mail.MailAddress(email);
            }
            catch
            {
                token.AddErrorMessage("The mail parameter you sent was not valid.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                return;
            }

            if (mailAddress.Host != Config.Email.DODEmailHost)
            {
                token.AddErrorMessage("The email you sent was not a valid DoD email.  We require that you use your military email to do the password reset.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                return;
            }

            //Let's get the continue link
            if (!token.Args.ContainsKey("continuelink"))
            {
                token.AddErrorMessage("You failed to send a 'continuelink' parameter.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                return;
            }

            var continueLink = token.Args["continuelink"] as string;

            //Let's just do some basic validation and make sure it's a real URI.
            if (!Uri.IsWellFormedUriString(continueLink, UriKind.Absolute))
            {
                token.AddErrorMessage("The continue link you sent was not a valid URI.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                return;
            }

            //Now we need to go load the profile that matches this email address/ssn combination.
            //Then we need to ensure that there is only one profile and that the profile we get is claimed (you can't reset a password that doesn't exist.)
            //If that all is good, then we'll create the pending password reset, log the event on the profile, and then send the client an email.

            //Here we go
            using (var session = NHibernateHelper.CreateStatefulSession())
            using (var transaction = session.BeginTransaction())
            {
                try
                {
                    //Find the user who has the given email address and has the given ssn.
                    var person = session.QueryOver<Person>()
                        .Where(x => x.SSN == ssn)
                        .Fetch(x => x.EmailAddresses).Eager
                        .JoinQueryOver<EmailAddress>(x => x.EmailAddresses)
                        .Where(x => x.Address == email)
                        .TransformUsing(Transformers.DistinctRootEntity)
                        .SingleOrDefault<Person>();

                    if (person == null)
                    {
                        token.AddErrorMessage("That ssn/email address combination belongs to no profile.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                        return;
                    }

                    //Ok so the ssn and email address gave us a single profile back.  Now we just need to make sure it's claimed.
                    if (!person.IsClaimed)
                    {
                        token.AddErrorMessage("That profile has not yet been claimed and therefore can not have its password reset.  Please consider trying to register first.", ErrorTypes.Validation, System.Net.HttpStatusCode.Forbidden);
                        return;
                    }

                    //Let's also make sure the client has a dod email address.

                    person.AccountHistory.Add(new AccountHistoryEvent
                    {
                        AccountHistoryEventType = Entities.ReferenceLists.AccountHistoryTypes.PasswordResetInitiated,
                        EventTime = token.CallTime
                    });

                    //Save the event
                    session.Update(person);

                    //Create the pending password reset thing.
                    var pendingPasswordReset = new PendingPasswordReset
                    {
                        Person = person,
                        Time = token.CallTime
                    };

                    //Save that.
                    session.Save(pendingPasswordReset);

                    //And then send the email.
                    var model = new Email.Models.BeginPasswordResetEmailModel
                    {
                        FriendlyName = person.ToString(),
                        PasswordResetId = pendingPasswordReset.Id,
                        PasswordResetLink = continueLink
                    };

                    Email.EmailInterface.CCEmailMessage
                        .CreateDefault()
                        .To(person.EmailAddresses.Select(x => new System.Net.Mail.MailAddress(x.Address, person.ToString())))
                        .Subject("Password Reset")
                        .HTMLAlternateViewUsingTemplateFromEmbedded("CCServ.Email.Templates.BeginPasswordReset_HTML.html", model)
                        .SendWithRetryAndFailure(TimeSpan.FromSeconds(1));

                    token.SetResult("Success");

                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }



        }

        /// <summary>
        /// WARNING!  THIS METHOD IS EXPOSED TO THE CLIENT AND IS NOT INTENDED FOR INTERNAL USE.  AUTHENTICATION, AUTHORIZATION AND VALIDATION MUST BE HANDLED PRIOR TO DB INTERACTION.
        /// <para />
        /// Completes a password reset by updating the password to the given password for the given reset password id.
        /// <para />
        /// Client Parameters: <para />
        ///     passwordResetID : The reset password id that was emailed to the client during the start password reset endpoint. <para />
        ///     password : The password the client wants the account to have.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        [EndpointMethod(EndpointName = "CompletePasswordReset", AllowArgumentLogging = false, AllowResponseLogging = true, RequiresAuthentication = false)]
        private static void EndpointMethod_CompletePasswordReset(MessageToken token)
        {

            //First, let's make sure the args are present.
            if (!token.Args.ContainsKey("passwordresetid"))
                token.AddErrorMessage("You didn't send a 'passwordresetid' parameter.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
            if (!token.Args.ContainsKey("password"))
                token.AddErrorMessage("You didn't send a 'password' parameter.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);

            //If there were any errors from the above checks, then stop now.
            if (token.HasError)
                return;

            string password = token.Args["password"] as string;
            Guid passwordResetId;
            if (!Guid.TryParse(token.Args["passwordresetid"] as string, out passwordResetId))
            {
                token.AddErrorMessage("The password reset ID you sent was not in the right format.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                return;
            }
            //Create the hash.
            string passwordHash = ClientAccess.PasswordHash.CreateHash(password);

            //Ok, we're going to use the password reset Id to load the pending password reset.
            //If we get one, we'll make sure it's still valid.
            //If it is, we'll set the password and then send the user an email telling them that the password was reset.
            //We'll also log the event on the user's profile.

            using (var session = NHibernateHelper.CreateStatefulSession())
            using (var transaction = session.BeginTransaction())
            {
                try
                {
                    var pendingPasswordReset = session.Get<PendingPasswordReset>(passwordResetId);

                    if (pendingPasswordReset == null)
                    {
                        token.AddErrorMessage("That password reset Id does not correspond to an actual password reset event.  Try initiating a password reset first.", ErrorTypes.Validation, System.Net.HttpStatusCode.Forbidden);
                        return;
                    }

                    //Is the record still valid?
                    if (!pendingPasswordReset.IsValid())
                    {
                        //If not we need to delete the record and then tell the client to start over.
                        session.Delete(pendingPasswordReset);

                        token.AddErrorMessage("It appears you waited too long to reset your password!  Please restart the password reset process.", ErrorTypes.Validation, System.Net.HttpStatusCode.Forbidden);
                        return;
                    }

                    //Well, now we're ready!  All we have to do now is change the password and then log the event and delete the pending password reset.
                    pendingPasswordReset.Person.PasswordHash = passwordHash;

                    //Let's also add the account history. 
                    pendingPasswordReset.Person.AccountHistory.Add(new AccountHistoryEvent
                    {
                        AccountHistoryEventType = Entities.ReferenceLists.AccountHistoryTypes.PasswordResetCompleted,
                        EventTime = token.CallTime
                    });

                    //Update/save the person.
                    session.Update(pendingPasswordReset.Person);

                    //Finally we need to send an email before we delete the object.
                    var model = new Email.Models.FinishPasswordResetEmailModel
                    {
                        FriendlyName = pendingPasswordReset.Person.ToString(),
                    };

                    Email.EmailInterface.CCEmailMessage
                        .CreateDefault()
                        .To(pendingPasswordReset.Person.EmailAddresses.Select(x => new System.Net.Mail.MailAddress(x.Address, pendingPasswordReset.Person.ToString())))
                        .Subject("Password Reset")
                        .HTMLAlternateViewUsingTemplateFromEmbedded("CCServ.Email.Templates.FinishPasswordReset_HTML.html", model)
                        .SendWithRetryAndFailure(TimeSpan.FromSeconds(1));

                    session.Delete(pendingPasswordReset);

                    token.SetResult("Success");

                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        #endregion
    }
}