using System;
using System.Collections.Generic;
using System.Linq;
using CommandCentral.Authorization;
using CommandCentral.ClientAccess;
using CommandCentral.Entities.ReferenceLists;
using FluentNHibernate.Mapping;
using FluentValidation;
using NHibernate.Transform;

namespace CommandCentral.Entities
{
    /// <summary>
    /// Describes a single person and all their properties and data access methods.
    /// </summary>
    public class Person
    {

        #region Properties

        /// <summary>
        /// The person's unique Id.
        /// </summary>
        public virtual Guid Id { get; set; }

        #region Main Properties

        /// <summary>
        /// The person's last name.
        /// </summary>
        public virtual string LastName { get; set; }

        /// <summary>
        /// The person's first name.
        /// </summary>
        public virtual string FirstName { get; set; }

        /// <summary>
        /// The person's middle name.
        /// </summary>
        public virtual string MiddleName { get; set; }

        /// <summary>
        /// The person's SSN.
        /// </summary>
        public virtual string SSN { get; set; }

        /// <summary>
        /// The person's date of birth.
        /// </summary>
        public virtual DateTime? DateOfBirth { get; set; }

        /// <summary>
        /// The person's sex.
        /// </summary>
        public virtual Sex Sex { get; set; }

        /// <summary>
        /// The person's remarks.  This is the primary comments section
        /// </summary>
        public virtual string Remarks { get; set; }

        /// <summary>
        /// Stores the person's ethnicity.
        /// </summary>
        public virtual Ethnicity Ethnicity { get; set; }

        /// <summary>
        /// The person's religious preference
        /// </summary>
        public virtual ReligiousPreference ReligiousPreference { get; set; }

        /// <summary>
        /// The person's suffix, sch as IV, Esquire, etc.
        /// </summary>
        public virtual Suffix Suffix { get; set; }

        /// <summary>
        /// The person's rank (e5, etc.)
        /// </summary>
        public virtual Rank Rank { get; set; }

        /// <summary>
        /// The person's rate (CTI2, CTR1)
        /// </summary>
        public virtual Rate Rate { get; set; }

        /// <summary>
        /// The person's division
        /// </summary>
        public virtual Division Division { get; set; }

        /// <summary>
        /// The person's department
        /// </summary>
        public virtual Department Department { get; set; }

        /// <summary>
        /// The person's command
        /// </summary>
        public virtual Command Command { get; set; }

        #endregion

        #region Work Properties

        /// <summary>
        ///The person's billet.
        /// </summary>
        public virtual Billet Billet { get; set; }

        /// <summary>
        /// The NECs of the person.
        /// </summary>
        public virtual IList<NEC> NECs { get; set; }

        /// <summary>
        /// The person's supervisor
        /// </summary>
        public virtual string Supervisor { get; set; }

        /// <summary>
        /// The person's work center.
        /// </summary>
        public virtual string WorkCenter { get; set; }

        /// <summary>
        /// The room in which the person works.
        /// </summary>
        public virtual string WorkRoom { get; set; }

        /// <summary>
        /// A free form text field intended to let the client store the shift of a person - however the client wants to do that.
        /// </summary>
        public virtual string Shift { get; set; }

        /// <summary>
        /// The comments section for the work page
        /// </summary>
        public virtual string WorkRemarks { get; set; }

        /// <summary>
        /// The person's duty status
        /// </summary>
        public virtual DutyStatus DutyStatus { get; set; }

        /// <summary>
        /// The person's UIC
        /// </summary>
        public virtual UIC UIC { get; set; }

        /// <summary>
        /// The date/time that the person arrived at the command.
        /// </summary>
        public virtual DateTime? DateOfArrival { get; set; }

        /// <summary>
        /// The client's job title.
        /// </summary>
        public virtual string JobTitle { get; set; }

        /// <summary>
        /// The date/time of the end of active obligatory service (EAOS) for the person.
        /// </summary>
        public virtual DateTime? EAOS { get; set; }

        /// <summary>
        /// The date/time that the client left/will leave the command.
        /// </summary>
        public virtual DateTime? DateOfDeparture { get; set; }

        #endregion

        #region Contacts Properties

        /// <summary>
        /// The email addresses of this person.
        /// </summary>
        public virtual IList<EmailAddress> EmailAddresses { get; set; }

        /// <summary>
        /// The Phone Numbers of this person.
        /// </summary>
        public virtual IList<PhoneNumber> PhoneNumbers { get; set; }

        /// <summary>
        /// The Physical Addresses of this person
        /// </summary>
        public virtual IList<PhysicalAddress> PhysicalAddresses { get; set; }

        /// <summary>
        /// Instructions from the user on what avenues of contact to follow in the case of an emergency.
        /// </summary>
        public virtual string EmergencyContactInstructions { get; set; }

        /// <summary>
        /// A free form text field intended to allow the user to make comments about their contact fields.
        /// </summary>
        public virtual string ContactRemarks { get; set; }

        #endregion

        #region Account

        /// <summary>
        /// A boolean indicating whether or not this account has been claimed.
        /// </summary>
        public virtual bool IsClaimed { get; set; }

        /// <summary>
        /// The client's username.
        /// </summary>
        public virtual string Username { get; set; }

        /// <summary>
        /// The client's hashed password.
        /// </summary>
        public virtual string PasswordHash { get; set; }

        /// <summary>
        /// The list of the person's permissions.
        /// </summary>
        public virtual IList<PermissionGroup> PermissionGroups { get; set; }

        /// <summary>
        /// The list of change events to which the person is subscribed.
        /// </summary>
        public virtual IList<ChangeEvent> SubscribedChangeEvents { get; set; }

        /// <summary>
        /// A list containing account history events, these are events that track things like login, password reset, etc.
        /// </summary>
        public virtual IList<AccountHistoryEvent> AccountHistory { get; set; }

        /// <summary>
        /// A list containing all changes that have every occurred to the profile.
        /// </summary>
        public virtual IList<Change> Changes { get; set; }



        #endregion

        #endregion

        #region Overrides

        /// <summary>
        /// Returns a friendly name for this user in the form: CTI2 Atwood, Daniel Kurt Roger
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("{0} {1}, {2} {3}", Rate == null ? "" : Rate.Value, LastName, FirstName, MiddleName);
        }

        #endregion

        #region Client Access Methods

        #region Login/Logout

        /// <summary>
        /// WARNING!  THIS IS A CLIENT METHOD.  AUTHENTICATION, AUTHORIZATION AND VALIDATION MUST BE HANDLED PRIOR TO DB INTERACTION.
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
        /// <para />
        /// Options: 
        /// <para />
        /// username : the username of the account that we are trying to log into.
        /// <para />
        /// password : the clear text password for the given username.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        private static void Login_Client(MessageToken token)
        {

            //Let's see if the parameters are here.
            if (!token.Args.ContainsKey("username"))
                token.AddErrorMessage("You didn't send a 'username' parameter.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);

            if (!token.Args.ContainsKey("password"))
                token.AddErrorMessage("You didn't send a 'password' parameter.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);

            if (!token.HasError)
            {
                string username = token.Args["username"] as string;
                string password = token.Args["password"] as string;

                //Validate the username and the password
                //TODO that validation

                //The query itself.  Note that SingleOrDefault will throw an exception if more than one person comes back.
                //This is ok because the username field is marked unique so this shouldn't happen and if it does then we want an exception.
                var person = token.CommunicationSession.QueryOver<Person>()
                    .Where(x => x.Username == username)
                    .SingleOrDefault<Person>();

                if (person == null)
                {
                    token.AddErrorMessage("Either the username or password is wrong.", ErrorTypes.Authentication, System.Net.HttpStatusCode.Forbidden);
                }
                else
                {
                    if (!ClientAccess.PasswordHash.ValidatePassword(password, person.PasswordHash))
                    {
                        //A login to the client's account failed.  We need to send an email.
                        EmailAddress address = person.EmailAddresses.FirstOrDefault(x => x.IsPreferred || x.IsContactable || x.IsDodEmailAddress);

                        if (address == null)
                            throw new Exception(string.Format("Login failed to the person's account whose Id is '{0}'; however, we could find no email to send this person a warning.", person.Id));

                        //Ok, so we have an email we can use to contact the person!
                        EmailHelper.SendFailedAccountLoginEmail(address.Address, person.Id).Wait();

                        token.AddErrorMessage("Either the username or password is wrong.", ErrorTypes.Authentication, System.Net.HttpStatusCode.Forbidden);

                    }
                    else
                    {
                        //Cool then we can make a new authentication session.
                        AuthenticationSession ses = new AuthenticationSession
                        {
                            IsActive = true,
                            LastUsedTime = token.CallTime,
                            LoginTime = token.CallTime,
                            Permissions = new List<PermissionGroup>(person.PermissionGroups), //We can't just assign it, we need to copy it.  This is so that we don't have shared references to the same collection.
                            Person = person
                        };

                        //Now insert it
                        token.CommunicationSession.Save(ses);

                        //Now log the account history event on the person.
                        person.AccountHistory.Add(new AccountHistoryEvent
                        {
                            AccountHistoryEventType = AccountHistoryEventTypes.Login,
                            EventTime = token.CallTime,
                            Person = person
                        });

                        //And update the person
                        token.CommunicationSession.SaveOrUpdate(person);

                        token.SetResult(new { PersonID = person.Id, person.PermissionGroups, AuthenticationToken = ses.Id, FriendlyName = person.ToString() });
                    }
                }
            }
        }

        /// <summary>
        /// WARNING!  THIS IS A CLIENT METHOD.  AUTHENTICATION, AUTHORIZATION AND VALIDATION MUST BE HANDLED PRIOR TO DB INTERACTION.
        /// <para />
        /// Logs out the user by invalidating the session/deleted it from the database.
        /// <para />
        /// Options: 
        /// <para />
        /// There are no parameters.  We use the authentication token from the authentication layer to do the logout.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        private static void Logout_Client(MessageToken token)
        {

            //Just make sure the client is logged in.  The endpoint's description should've handled this but you never know.
            if (token.AuthenticationSession == null)
            {
                token.AddErrorMessage("You must be logged in to update the news.", ErrorTypes.Authentication, System.Net.HttpStatusCode.Unauthorized);
            }
            else
            {
                //Log the event
                token.AuthenticationSession.Person.AccountHistory.Add(new AccountHistoryEvent
                {
                    AccountHistoryEventType = AccountHistoryEventTypes.Logout,
                    EventTime = token.CallTime,
                    Person = token.AuthenticationSession.Person
                });

                //Now update the person
                token.CommunicationSession.SaveOrUpdate(token.AuthenticationSession.Person);

                //Okey dokey, now let's delete the session.
                token.CommunicationSession.Delete(token.AuthenticationSession);

                //Remove the authentication session from the token because it has been deleted.
                token.AuthenticationSession = null;

                token.SetResult("Success");
            }
        }

        #endregion

        #region Registration

        /// <summary>
        /// WARNING!  THIS IS A CLIENT METHOD.  AUTHENTICATION, AUTHORIZATION AND VALIDATION MUST BE HANDLED PRIOR TO DB INTERACTION.
        /// <para />
        /// Begins the registration process by sending an email to the account to whom the SSN belongs.  
        /// This email contains a confirmation key that has also been inserted into the database.
        /// <para />
        /// If the account doesn't have an email address associated to it yet, throw an error.
        /// <para /> 
        /// If the account is already claimed, then trying to start the registration process for this account looks pretty bad and we send an email to a bunch of people to inform them that this happened.
        /// <para />
        /// Options: 
        /// <para />
        /// ssn : The SSN of the account that we are going to try to claim.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        private static void BeginRegistration_Client(MessageToken token)
        {
            //First we need the client's ssn.  This is the account they want to claim.
            if (!token.Args.ContainsKey("ssn"))
            {
                token.AddErrorMessage("You didn't send a 'ssn' parameter.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                return;
            }
            
            string ssn = token.Args["ssn"] as string;
            //TODO validate the ssn.

            //The query itself.  Note that SingleOrDefault will throw an exception if more than one person comes back.
            //This is ok because the ssn field is marked unique so this shouldn't happen and if it does then we want an exception.
            var person = token.CommunicationSession.QueryOver<Person>()
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
                //To do that, we need an email for this user.
                EmailAddress address = person.EmailAddresses.FirstOrDefault(x => x.IsPreferred || x.IsContactable || x.IsDodEmailAddress);

                if (address == null)
                    throw new Exception(string.Format("Another user tried to claim the profile whose Id is '{0}'; however, we could find no email to send this person a warning.", person.Id));

                //Now send that email.
                EmailHelper.SendBeginRegistrationErrorEmail(address.Address, person.Id).Wait();

                token.AddErrorMessage("A user has already claimed that account.  That user has been notified of your attempt to claim the account." +
                                      "If you believe this is in error or if you are the rightful owner of this account, please call the development team immediately.", ErrorTypes.Validation, System.Net.HttpStatusCode.Forbidden);

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

            //Let's see if there is already a pending account confirmation.
            var pendingAccountConfirmations = token.CommunicationSession.QueryOver<PendingAccountConfirmation>()
                .Where(x => x.Person.Id == person.Id)
                .List<PendingAccountConfirmation>();

            //If there are any (should only be one) then we're going to delete all of them.  
            //This would happen if the client let one sit too long and it become invalid and then had to call begin registration again.
            if (pendingAccountConfirmations.Any())
                pendingAccountConfirmations.ToList().ForEach(x => token.CommunicationSession.Delete(x));

            //Well, looks like we have a DOD email address and there are no old pending account confirmations sitting in the database.  Let's make an account confirmation... thing.
            var pendingAccountConfirmation = new PendingAccountConfirmation
            {
                Person = person,
                Time = token.CallTime
            };

            //And then persist it.
            token.CommunicationSession.Save(pendingAccountConfirmation);

            //Now let's make a new account event and then update the person.
            person.AccountHistory.Add(new AccountHistoryEvent
            {
                AccountHistoryEventType = AccountHistoryEventTypes.RegistrationStarted,
                EventTime = token.CallTime,
                Person = person
            });

            //And then persist that by updating the person.
            token.CommunicationSession.Update(person);

            //Wait!  we're not even done yet.  Let's send the client the registration email now.
            EmailHelper.SendConfirmAccountEmail(dodEmailAddress.Address, pendingAccountConfirmation.Id, ssn).Wait();

            //Ok, Jesus Christ.  I think we're finally done.
            token.SetResult("Success");
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
        /// username : the username the client wants to assign to the account.
        /// <para />
        /// password : the password the client wants to assign to the account.
        /// <para />
        /// accountconfirmationid : The unique Id that was sent to the user's email address by the begin registration endpoint.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        private static void CompleteRegistration_Client(MessageToken token)
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

            var pendingAccountConfirmation = token.CommunicationSession.Get<PendingAccountConfirmation>(accountConfirmationId);
            if (pendingAccountConfirmation == null)
            {
                token.AddErrorMessage("For the account confirmation Id that you provided, no account registration process has been started.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                return;
            }
            
            //Is the record valid?
            if (!pendingAccountConfirmation.IsValid())
            {
                //If not we need to delete the record and then tell the client to start over.
                token.CommunicationSession.Delete(pendingAccountConfirmation);

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

            //Ok, let's also add an account history saying we completed registration.
            pendingAccountConfirmation.Person.AccountHistory.Add(new AccountHistoryEvent
            {
                AccountHistoryEventType = AccountHistoryEventTypes.RegistrationCompleted,
                EventTime = token.CallTime,
                Person = pendingAccountConfirmation.Person
            });

            //Cool, so now just update the person object.
            token.CommunicationSession.Update(pendingAccountConfirmation.Person);

            //TODO send completion email.

            //Now delete the pending account confirmation.  We don't need it anymore.
            token.CommunicationSession.Delete(pendingAccountConfirmation);

            token.SetResult("Success");
        }

        #endregion

        #region Password Reset

        /// <summary>
        /// WARNING!  THIS IS A CLIENT METHOD.  AUTHENTICATION, AUTHORIZATION AND VALIDATION MUST BE HANDLED PRIOR TO DB INTERACTION.
        /// <para />
        /// In order to start a password reset, we're going to use the given email address and ssn to load the person's is claimed field.  If is claimed is false, 
        /// then the account hasn't been claimed yet and you can't reset the password.  
        /// <para />
        /// Options: 
        /// <para />
        /// email : The email address of the account we want to reset
        /// ssn : The SSN of the account we want to reset.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        private static void BeginPasswordReset_Client(MessageToken token)
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

            //Now we need to go load the profile that matches this email address/ssn combination.
            //Then we need to ensure that there is only one profile and that the profile we get is claimed (you can't reset a password that doesn't exist.)
            //If that all is good, then we'll create the pending password reset, log the event on the profile, and then send the client an email.

            //Find the user who has the given email address and has the given ssn.
            var person = token.CommunicationSession.QueryOver<Person>()
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

            //And now we know it's claimed.  So make the event, log the event and send the email.
            person.AccountHistory.Add(new AccountHistoryEvent
            {
                AccountHistoryEventType = AccountHistoryEventTypes.PasswordResetInitiated,
                EventTime = token.CallTime,
                Person = person
            });

            //Save the event
            token.CommunicationSession.Update(person);

            //Create the pending password reset thing.
            var pendingPasswordReset = new PendingPasswordReset
            {
                Person = person,
                Time = token.CallTime
            };

            //Save that.
            token.CommunicationSession.Save(pendingPasswordReset);

            //And then send the email.
            EmailHelper.SendBeginPasswordResetEmail(pendingPasswordReset.Id, email).Wait();

            token.SetResult("Success");
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
        private static void CompletePasswordReset_Client(MessageToken token)
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

            var pendingPasswordReset = token.CommunicationSession.Get<PendingPasswordReset>(passwordResetId);

            if (pendingPasswordReset == null)
            {
                token.AddErrorMessage("That password reset Id does not correspond to an actual password reset event.  Try initiating a password reset first.", ErrorTypes.Validation, System.Net.HttpStatusCode.Forbidden);
                return;
            }

            //Is the record still valid?
            if (!pendingPasswordReset.IsValid())
            {
                //If not we need to delete the record and then tell the client to start over.
                token.CommunicationSession.Delete(pendingPasswordReset);
                
                token.AddErrorMessage("It appears you waited too long to register your account and it has become inactive!  Please restart the password reset process.", ErrorTypes.Validation, System.Net.HttpStatusCode.Forbidden);
                return;
            }

            //Well, now we're ready!  All we have to do now is change the password and then log the event and delete the pending password reset.
            pendingPasswordReset.Person.PasswordHash = passwordHash;

            
            pendingPasswordReset.Person.AccountHistory.Add(new AccountHistoryEvent
            {
                AccountHistoryEventType = AccountHistoryEventTypes.PasswordResetCompleted,
                EventTime = token.CallTime,
                Person = pendingPasswordReset.Person
            });

            //Update/save it.
            token.CommunicationSession.Update(pendingPasswordReset);

            //Finally we need to send an email before we delete the object.
            //TODO send that email.

            token.CommunicationSession.Delete(pendingPasswordReset);

            token.SetResult("Success");
        }

        #endregion

        #region Get/Load/Select/Search

        /// <summary>
        /// WARNING!  THIS IS A CLIENT METHOD.  AUTHENTICATION, AUTHORIZATION AND VALIDATION MUST BE HANDLED PRIOR TO DB INTERACTION.
        /// <para />
        /// Loads a single person from the database and sets those fields to null that the client is not allowed to return.  If the client requests their own profile, all fields are returned.
        /// <para />
        /// Options: 
        /// <para />
        /// personid - The ID of the person to load.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        private static void LoadPerson_Client(MessageToken token)
        {

            //First, let's make sure the args are present.
            if (!token.Args.ContainsKey("personid"))
                token.AddErrorMessage("You didn't send a 'personid' parameter.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);

            //If there were any errors from the above checks, then stop now.
            if (token.HasError)
                return;

            Guid personId;
            if (!Guid.TryParse(token.Args["personid"] as string, out personId))
            {
                token.AddErrorMessage("The person ID you sent was not in the right format.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                return;
            }

            //Now let's load the person and then set any fields the client isn't allowed to see to null.
            //We need the entire object so we're going to initialize it and then unproxy it.
            var person = token.CommunicationSession.Get<Person>(personId);
            NHibernate.NHibernateUtil.Initialize(person);
            person = token.CommunicationSession.GetSessionImplementation().PersistenceContext.Unproxy(person) as Person;

            
            //Here we're going to ask if the person is not null (a person was returned) and that the person that was returned is not the person asking for a person. Person.
            if (person != null && personId != token.AuthenticationSession.Person.Id)
            {
                //Now we need to evict this copy of the person from the session so that our changes to it don't reflect in the database.  That would be awkward.
                token.CommunicationSession.Evict(person);

                var personMetadata = DataAccess.NHibernateHelper.GetEntityMetadata("Person");

                //Ok, now we need to set all the fields to null the client can't return.  First let's see what fields the client can return.
                //This is going to go through all model permissions that target a Person, and get all the returnable fields.
                var returnableFields = token.AuthenticationSession.Person.PermissionGroups
                                            .SelectMany(x => x.ModelPermissions
                                                .Where(y => y.ModelName == personMetadata.EntityName)
                                                .SelectMany(y => y.ReturnableFields))
                                            .ToList();

                //Now for every property not in the above list, let's set the property to null.
                var allPropertyNames = personMetadata.PropertyNames;

                //Set the nulls if they're null.
                foreach (var propertyName in allPropertyNames)
                {
                    if (!returnableFields.Contains(propertyName))
                        personMetadata.SetPropertyValue(person, propertyName, null, NHibernate.EntityMode.Poco);
                }
            }

            token.SetResult(person
                );
        }

        #endregion

        #endregion

        /// <summary>
        /// The exposed endpoints
        /// </summary>
        public static List<EndpointDescription> EndpointDescriptions
        {
            get
            {
                return new List<EndpointDescription>
                {
                    new EndpointDescription
                    {
                        Name = "Login",
                        AllowArgumentLogging = false,
                        AllowResponseLogging = false,
                        AuthorizationNote = "None",
                        DataMethod = Login_Client,
                        Description = "Logs in the user given a proper username/password combination and returns a GUID.  This GUID is the client's authentication token and must be included in all subsequent authentication-required requests.",
                        ExampleOutput = () => "TODO",
                        IsActive = true,
                        OptionalParameters = null,
                        RequiredParameters = new List<string>
                        {
                            "apikey - The unique GUID token assigned to your application for metrics purposes.",
                            "username - The user's case sensitive username.",
                            "password - The user's case sensitive password."
                        },
                        RequiredSpecialPermissions = null,
                        RequiresAuthentication = false
                    },
                    new EndpointDescription
                    {
                        Name = "Logout",
                        AllowArgumentLogging = true,
                        AllowResponseLogging = true,
                        AuthorizationNote = "Client must be logged in.",
                        DataMethod = Logout_Client,
                        Description = "Logs out the user by invalidating the session/deleted it from the database.",
                        ExampleOutput = () => "Success - This return value can be ignored entirely and the string that is returned (“Success”) can be replaced with anything else.  Instead, I recommend checking the return container’s .HasError property.  If error is false, then you can assume the method completed successfully.",
                        IsActive = true,
                        OptionalParameters = null,
                        RequiredParameters = new List<string>
                        {
                            "apikey - The unique GUID token assigned to your application for metrics purposes.",
                            "authenticationtoken - The GUID authentication token for the user that was retrieved after successful login."
                        },
                        RequiredSpecialPermissions = null,
                        RequiresAuthentication = true
                    },
                    new EndpointDescription
                    {
                        Name = "BeginRegistration",
                        AllowArgumentLogging = true,
                        AllowResponseLogging = true,
                        AuthorizationNote = "None.",
                        DataMethod = BeginRegistration_Client,
                        Description = "Begins the registration process.",
                        ExampleOutput = () => "Success - This return value can be ignored entirely and the string that is returned (“Success”) can be replaced with anything else.  Instead, I recommend checking the return container’s .HasError property.  If error is false, then you can assume the method completed successfully.",
                        IsActive = true,
                        OptionalParameters = null,
                        RequiredParameters = new List<string>
                        {
                            "apikey - The unique GUID token assigned to your application for metrics purposes.",
                            "ssn - The user's SSN.  SSNs are expected to consist of numbers only.  Non-digit characters will cause an exception."
                        },
                        RequiredSpecialPermissions = null,
                        RequiresAuthentication = false
                    },
                    new EndpointDescription
                    {
                        Name = "CompleteRegistration",
                        AllowArgumentLogging = false,
                        AllowResponseLogging = true,
                        AuthorizationNote = "None.",
                        DataMethod = CompleteRegistration_Client,
                        Description = "Completes the registration process and assigns the username and password to the desired user account.",
                        ExampleOutput = () => "Success - This return value can be ignored entirely and the string that is returned (“Success”) can be replaced with anything else.  Instead, I recommend checking the return container’s .HasError property.  If error is false, then you can assume the method completed successfully.",
                        IsActive = true,
                        OptionalParameters = null,
                        RequiredParameters = new List<string>
                        {
                            "apikey - The unique GUID token assigned to your application for metrics purposes.",
                            "username - The username the client wants to be assigned to the account.",
                            "password - The password the client wants to be assigned to the account.",
                            "accountconfirmationid - The unique GUID token that was sent to the user through their DOD email."
                        },
                        RequiredSpecialPermissions = null,
                        RequiresAuthentication = false
                    },
                    new EndpointDescription
                    {
                        Name = "BeginPasswordReset",
                        AllowArgumentLogging = true,
                        AllowResponseLogging = true,
                        AuthorizationNote = "None.",
                        DataMethod = BeginPasswordReset_Client,
                        Description = "Starts the password reset process by sending the client an email with a link they can click on to reset their password.",
                        ExampleOutput = () => "Success - This return value can be ignored entirely and the string that is returned (“Success”) can be replaced with anything else.  Instead, I recommend checking the return container’s .HasError property.  If error is false, then you can assume the method completed successfully.",
                        IsActive = true,
                        OptionalParameters = null,
                        RequiredParameters = new List<string>
                        {
                            "apikey - The unique GUID token assigned to your application for metrics purposes.",
                            "ssn - The user's SSN.  SSNs are expected to consist of numbers only.  Non-digit characters will cause an exception.",
                            "email - The email address of the account we want to reset.  This must be a DOD email address and be on the same account as the given SSN."
                        },
                        RequiredSpecialPermissions = null,
                        RequiresAuthentication = false
                    },
                    new EndpointDescription
                    {
                        Name = "CompletePasswordReset",
                        AllowArgumentLogging = false,
                        AllowResponseLogging = true,
                        AuthorizationNote = "None.",
                        DataMethod = CompletePasswordReset_Client,
                        Description = "Finishes the password reset process by setting the password to the received password for the reset password id.",
                        ExampleOutput = () => "Success - This return value can be ignored entirely and the string that is returned (“Success”) can be replaced with anything else.  Instead, I recommend checking the return container’s .HasError property.  If error is false, then you can assume the method completed successfully.",
                        IsActive = true,
                        OptionalParameters = null,
                        RequiredParameters = new List<string>
                        {
                            "apikey - The unique GUID token assigned to your application for metrics purposes.",
                            "passwordresetid - The password reset id that was emailed to the client during the start password reset endpoint.",
                            "password - The password the client wants the account to have."
                        },
                        RequiredSpecialPermissions = null,
                        RequiresAuthentication = false
                    },
                    new EndpointDescription
                    {
                        Name = "LoadPerson",
                        AllowArgumentLogging = true,
                        AllowResponseLogging = true,
                        AuthorizationNote = "Those fields the client can't return will be set to null.",
                        DataMethod = LoadPerson_Client,
                        Description = "Loads a single person from the database and sets those fields to null that the client is not allowed to return.  If the client requests their own profile, all fields are returned.",
                        ExampleOutput = () => "An entire person object containing the entire record minus those fields the client was not allowed to return.  These fields are set to null.",
                        IsActive = true,
                        OptionalParameters = null,
                        RequiredParameters = new List<string>
                        {
                            "apikey - The unique GUID token assigned to your application for metrics purposes.",
                            "authenticationtoken - The GUID authentication token for the user that was retrieved after successful login.",
                            "personid - The ID of the person to load."
                        },
                        RequiredSpecialPermissions = null,
                        RequiresAuthentication = true
                    }

                };
            }
        }

        /// <summary>
        /// Maps a person to the database.
        /// </summary>
        public class PersonMapping : ClassMap<Person>
        {
            /// <summary>
            /// Maps a person to the database.
            /// </summary>
            public PersonMapping()
            {
                Table("persons");

                Id(x => x.Id).GeneratedBy.Guid();

                References(x => x.Sex).Nullable().LazyLoad();
                References(x => x.Ethnicity).Nullable().LazyLoad();
                References(x => x.ReligiousPreference).Nullable().LazyLoad();
                References(x => x.Suffix).Nullable().LazyLoad();
                References(x => x.Rank).Not.Nullable().LazyLoad();
                References(x => x.Rate).Not.Nullable().LazyLoad();
                References(x => x.Division).Not.Nullable().LazyLoad();
                References(x => x.Department).Not.Nullable().LazyLoad();
                References(x => x.Command).Not.Nullable().LazyLoad();
                References(x => x.Billet).Nullable().LazyLoad();
                References(x => x.DutyStatus).Not.Nullable().LazyLoad();
                References(x => x.UIC).Not.Nullable().LazyLoad();

                Map(x => x.LastName).Not.Nullable().Length(40);
                Map(x => x.FirstName).Not.Nullable().Length(40).LazyLoad();
                Map(x => x.MiddleName).Nullable().Length(40).LazyLoad();
                Map(x => x.SSN).Not.Nullable().Length(40).Unique().LazyLoad();
                Map(x => x.DateOfBirth).Not.Nullable().LazyLoad();
                Map(x => x.Remarks).Nullable().Length(150).LazyLoad();
                Map(x => x.Supervisor).Nullable().Length(40).LazyLoad();
                Map(x => x.WorkCenter).Nullable().Length(40).LazyLoad();
                Map(x => x.WorkRoom).Nullable().Length(40).LazyLoad();
                Map(x => x.Shift).Nullable().Length(40).LazyLoad();
                Map(x => x.WorkRemarks).Nullable().Length(150).LazyLoad();
                Map(x => x.DateOfArrival).Not.Nullable().LazyLoad();
                Map(x => x.JobTitle).Nullable().Length(40).LazyLoad();
                Map(x => x.EAOS).Not.Nullable().LazyLoad();
                Map(x => x.DateOfDeparture).Nullable().LazyLoad();
                Map(x => x.EmergencyContactInstructions).Nullable().Length(150).LazyLoad();
                Map(x => x.ContactRemarks).Nullable().Length(150).LazyLoad();
                Map(x => x.IsClaimed).Not.Nullable().Default(false.ToString()).LazyLoad();
                Map(x => x.Username).Nullable().Length(40).Unique().LazyLoad();
                Map(x => x.PasswordHash).Nullable().Length(100).LazyLoad();

                HasManyToMany(x => x.NECs).LazyLoad();
                HasManyToMany(x => x.PermissionGroups).LazyLoad();
                HasManyToMany(x => x.SubscribedChangeEvents).LazyLoad();

                HasMany(x => x.AccountHistory).LazyLoad().Cascade.All();
                HasMany(x => x.Changes).LazyLoad().Cascade.All();
                HasMany(x => x.EmailAddresses).LazyLoad().Cascade.All();
                HasMany(x => x.PhoneNumbers).LazyLoad().Cascade.All();
                HasMany(x => x.PhysicalAddresses).LazyLoad().Cascade.All();

                LazyLoad();
            }
        }

        public class PersonValidator : AbstractValidator<Person>
        {

        }

    }

}
