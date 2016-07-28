﻿using System;
using System.Collections.Generic;
using System.Linq;
using CommandCentral.Authorization;
using CommandCentral.ClientAccess;
using CommandCentral.Entities.ReferenceLists;
using CommandCentral.DataAccess;
using FluentNHibernate.Mapping;
using FluentValidation;
using NHibernate.Transform;
using NHibernate.Criterion;
using NHibernate.Linq;
using AtwoodUtils;

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
        /// The person's suffix.
        /// </summary>
        public virtual string Suffix { get; set; }

        /// <summary>
        /// The person's date of birth.
        /// </summary>
        public virtual DateTime? DateOfBirth { get; set; }

        /// <summary>
        /// The person's sex.
        /// </summary>
        public virtual Sexes Sex { get; set; }

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
        /// The person's paygrade (e5, O1, O5, CWO2, GS1,  etc.)
        /// </summary>
        public virtual Paygrades Paygrade { get; set; }

        /// <summary>
        /// The person's Designation (CTI2, CTR1, 1114, Job title)
        /// </summary>
        public virtual Designation Designation { get; set; }

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
        public virtual DutyStatuses DutyStatus { get; set; }

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

        /// <summary>
        /// Represents this person's current muster status for the current muster day.  This property is intended to be updated only by the muster endpoints, not generic updates.
        /// </summary>
        public virtual Muster.MusterRecord CurrentMusterStatus { get; set; }

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
            return string.Format("{0} {1}, {2} {3}", Designation == null ? "" : Designation.Value, LastName, FirstName, MiddleName);
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Returns an object containing two properties: this object's Id and this object's .ToString in a parameter called FriendlyName.  Intended for use with DTOs.
        /// </summary>
        /// <returns></returns>
        public virtual object ToBasicPerson()
        {
            return new
            {
                Id = this.Id,
                FriendlyName = this.ToString()
            };
        }
        
        /// <summary>
        /// Returns a boolean indicating whether or not this person is in the chain of command of a given person.
        /// <para/>
        /// Person A (this person) is in Person B's chain of command if:
        /// <para/>
        /// A and B have the same command and Person A has a command-level permission OR
        /// <para />
        /// A and B have the same command and department and Person A has a department-level permission OR
        /// <para />
        /// A and B have the same command and department and division and Person A has a division-level permission.
        /// <para />
        /// Note: No person may be in his/her own chain of command.  Developers are in everyone's chain of command.
        /// </summary>
        /// <param name="person"></param>
        /// <param name="track">The track in which to look for the chain of command.</param>
        /// <returns></returns>
        public virtual bool IsInChainOfCommandOf(Person person, PermissionTracks track)
        {
            if (Id == person.Id)
                return false;

            if (HasPermissionLevelInTrack(PermissionLevels.Command, track) && Command.Equals(person.Command))
                return true;

            if (HasPermissionLevelInTrack(PermissionLevels.Command, track) && Command.Equals(person.Command) && Department.Equals(person.Department))
                return true;

            if (HasPermissionLevelInTrack(PermissionLevels.Command, track) && Command.Equals(person.Command) && Department.Equals(person.Department) && Division.Equals(person.Division))
                return true;

            return false;
        }

        /// <summary>
        /// Returns a boolean indicating if this person has all of the special permissions passed.
        /// </summary>
        /// <param name="permissions"></param>
        /// <returns></returns>
        public virtual bool HasSpecialPermissions(params SpecialPermissions[] permissions)
        {
            var userPermissions = PermissionGroups.SelectMany(x => x.SpecialPermissions);
            foreach (var perm in permissions)
                if (!userPermissions.Contains(perm))
                    return false;

            return true;
        }

        /// <summary>
        /// Returns a boolean indicating whether or not this person has a permission that grants him/her the given permission level.
        /// </summary>
        /// <param name="permissionLevel"></param>
        /// <returns></returns>
        public virtual bool HasPermissionLevelInTrack(PermissionLevels permissionLevel, PermissionTracks track)
        {
            return PermissionGroups.Any(x => x.PermissionTrack == track && x.PermissionLevel == permissionLevel);
        }

        /// <summary>
        /// Returns a boolean indicating if this person is in a given permission track at any level.
        /// </summary>
        /// <param name="track"></param>
        /// <returns></returns>
        public virtual bool IsInPermissionTrack(PermissionTracks track)
        {
            return PermissionGroups.Any(x => x.PermissionTrack == track);
        }

        /// <summary>
        /// Returns a boolean indicating if this person is in the same command as the given person.
        /// </summary>
        /// <param name="person"></param>
        /// <returns></returns>
        public virtual bool IsInSameCommandAs(Person person)
        {
            return this.Command.Id == person.Command.Id;
        }

        /// <summary>
        /// Returns a boolean indicating that this person is in the same command and department as the given person.
        /// </summary>
        /// <param name="person"></param>
        /// <returns></returns>
        public virtual bool IsInSameDepartmentAs(Person person)
        {
            return this.Command.Id == person.Command.Id && this.Department.Id == person.Department.Id;
        }

        /// <summary>
        /// Returns a boolean indicating that this person is in the same command, department, and division as the given person.
        /// </summary>
        /// <param name="person"></param>
        /// <returns></returns>
        public virtual bool IsInSameDivisionAs(Person person)
        {
            return this.Command.Id == person.Command.Id && this.Department.Id == person.Department.Id && this.Division.Id == person.Division.Id;
        }

        /// <summary>
        /// Returns a permission level indicating the highest level of permissions a person has in a given track.
        /// <para/>
        /// For example, if a person has two permission groups in the Muster track, one at the division level and one at the command level, their highest permissions in the Muster track are command level.
        /// </summary>
        /// <param name="track"></param>
        /// <returns></returns>
        public virtual PermissionLevels GetHighestLevelInTrack(PermissionTracks track)
        {
            var groups = PermissionGroups.Where(x => x.PermissionTrack == track);

            if (!groups.Any())
                return PermissionLevels.None;

            return groups.Max(x => x.PermissionLevel);
        }

        /// <summary>
        /// Returns a boolean indicating whether or not this person can search in the given fields.
        /// </summary>
        /// <param name="entityName"></param>
        /// <param name="fields"></param>
        /// <returns></returns>
        public virtual bool CanSearchFields(string entityName = "Person", params string[] fields)
        {
            var searchableFields = PermissionGroups
                .SelectMany(x => x.ModelPermissions)
                .Where(x => x.ModelName == "Person")
                .SelectMany(x => x.SearchableFields);

            foreach (var field in fields)
                if (!searchableFields.Contains(field))
                    return false;

            return true;
        }

        #endregion
        
        #region Client Access

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
                using (var session = DataAccess.NHibernateHelper.CreateStatefulSession())
                using (var transaction = session.BeginTransaction())
                {
                    try
                    {
                        string username = token.Args["username"] as string;
                        string password = token.Args["password"] as string;

                        //Validate the username and the password
                        //TODO that validation

                        //The query itself.  Note that SingleOrDefault will throw an exception if more than one person comes back.
                        //This is ok because the username field is marked unique so this shouldn't happen and if it does then we want an exception.
                        var person = session.QueryOver<Person>()
                            .Where(x => x.Username == username)
                            .Fetch(x => x.PermissionGroups).Eager
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
                                    Person = person
                                };

                                //Now insert it
                                session.Save(ses);

                                token.SetResult(new { PersonId = person.Id, person.PermissionGroups, AuthenticationToken = ses.Id, FriendlyName = person.ToString() });
                            }
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
            using (var session = DataAccess.NHibernateHelper.CreateStatefulSession())
            using (var transaction = session.BeginTransaction())
            {
                try
                {

                    //First we need the client's ssn.  This is the account they want to claim.
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
                    var pendingAccountConfirmations = session.QueryOver<PendingAccountConfirmation>()
                        .Where(x => x.Person.Id == person.Id)
                        .List<PendingAccountConfirmation>();

                    //If there are any (should only be one) then we're going to delete all of them.  
                    //This would happen if the client tries to begin registration after already doing it.
                    if (pendingAccountConfirmations.Any())
                        pendingAccountConfirmations.ToList().ForEach(x => session.Delete(x));

                    //Well, looks like we have a DOD email address and there are no old pending account confirmations sitting in the database.  Let's make an account confirmation... thing.
                    var pendingAccountConfirmation = new PendingAccountConfirmation
                    {
                        Person = person,
                        Time = token.CallTime
                    };

                    //And then persist it.
                    session.Save(pendingAccountConfirmation);

                    //And then persist that by updating the person.
                    session.Update(person);

                    //Wait!  we're not even done yet.  Let's send the client the registration email now.
                    EmailHelper.SendConfirmAccountEmail(dodEmailAddress.Address, pendingAccountConfirmation.Id, ssn).Wait();

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

            using (var session = DataAccess.NHibernateHelper.CreateStatefulSession())
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

                    //Cool, so now just update the person object.
                    session.Update(pendingAccountConfirmation.Person);

                    //TODO send completion email.

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

            //Now we need to go load the profile that matches this email address/ssn combination.
            //Then we need to ensure that there is only one profile and that the profile we get is claimed (you can't reset a password that doesn't exist.)
            //If that all is good, then we'll create the pending password reset, log the event on the profile, and then send the client an email.

            //Here we go
            using (var session = DataAccess.NHibernateHelper.CreateStatefulSession())
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
                    EmailHelper.SendBeginPasswordResetEmail(pendingPasswordReset.Id, email).Wait();

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

            using (var session = DataAccess.NHibernateHelper.CreateStatefulSession())
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

                        token.AddErrorMessage("It appears you waited too long to register your account and it has become inactive!  Please restart the password reset process.", ErrorTypes.Validation, System.Net.HttpStatusCode.Forbidden);
                        return;
                    }

                    //Well, now we're ready!  All we have to do now is change the password and then log the event and delete the pending password reset.
                    pendingPasswordReset.Person.PasswordHash = passwordHash;


                    //Update/save it.
                    session.Update(pendingPasswordReset);

                    //Finally we need to send an email before we delete the object.
                    //TODO send that email.

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

        #region Create

        /// <summary>
        /// WARNING!  THIS METHOD IS EXPOSED TO THE CLIENT AND IS NOT INTENDED FOR INTERNAL USE.  AUTHENTICATION, AUTHORIZATION AND VALIDATION MUST BE HANDLED PRIOR TO DB INTERACTION.
        /// <para />
        /// Creates a new person in the database by taking a person object from the client, picking out only the properties we want, and saving them.  Then it returns the Id we assigned to the person.
        /// <para />
        /// Client Parameters: <para />
        ///     person - a properly formatted, optionally partial, person object containing the necessary information.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        [EndpointMethod(EndpointName = "CreatePerson", AllowArgumentLogging = true, AllowResponseLogging = true, RequiresAuthentication = true)]
        private static void EndpointMethod_CreatePerson(MessageToken token)
        {
            //Just make sure the client is logged in.
            if (token.AuthenticationSession == null)
            {
                token.AddErrorMessage("You must be logged in to create a person.", ErrorTypes.Authentication, System.Net.HttpStatusCode.Unauthorized);
                return;
            }

            //You have permission?
            if (!token.AuthenticationSession.Person.HasSpecialPermissions(SpecialPermissions.CreatePerson))
            {
                token.AddErrorMessage("You don't have permission to create persons.", ErrorTypes.Authorization, System.Net.HttpStatusCode.Unauthorized);
                return;
            }

            //Ok, since the client has permission to create a person, we'll assume they have permission to udpate all of the required fields.
            if (!token.Args.ContainsKey("person"))
            {
                token.AddErrorMessage("You failed to send a 'person' parameter!", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                return;
            }

            var personFromClient = token.Args["person"].CastJToken<Person>();

            //The person from the client... let's make sure that it is valid.  If it passes validation then it can be inserted.
            //For security we're going to take only the parameters that we need explicilty from the client.  All others will be thrown out. #whitelisting
            Person newPerson = new Person
            {
                FirstName = personFromClient.FirstName,
                MiddleName = personFromClient.MiddleName,
                LastName = personFromClient.LastName,
                Division = token.AuthenticationSession.Person.Division,
                Department = token.AuthenticationSession.Person.Department,
                Command = token.AuthenticationSession.Person.Command,
                Paygrade = personFromClient.Paygrade,
                UIC = personFromClient.UIC,
                Designation = personFromClient.Designation,
                Sex = personFromClient.Sex,
                SSN = personFromClient.SSN,
                DateOfBirth = personFromClient.DateOfBirth,
                DateOfArrival = personFromClient.DateOfArrival,
                DutyStatus = personFromClient.DutyStatus,
                Id = Guid.NewGuid(),
                IsClaimed = false,
                PermissionGroups = new List<PermissionGroup>()
            };
            newPerson.CurrentMusterStatus = Muster.MusterRecord.CreateDefaultMusterRecordForPerson(newPerson, token.CallTime);

            //We're also going to add on the default permission group.
            newPerson.PermissionGroups.Add(PermissionGroup.GetDefaultPermissionGroup());

            //Now for validation!
            var results = new PersonValidator().Validate(newPerson);

            if (results.Errors.Any())
            {
                token.AddErrorMessages(results.Errors.Select(x => x.ErrorMessage), ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                return;
            }

            using (var session = DataAccess.NHibernateHelper.CreateStatefulSession())
            using (var transaction = session.BeginTransaction())
            {
                try
                {
                    //The person is a valid object.  Let's go ahead and insert it.  If insertion fails it's most likely because we violated a Uniqueness rule in the database.
                    session.Save(newPerson);

                    //And now return the perosn's Id.
                    token.SetResult(newPerson.Id);

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

        #region Get/Load/Select/Search

        /// <summary>
        /// WARNING!  THIS METHOD IS EXPOSED TO THE CLIENT AND IS NOT INTENDED FOR INTERNAL USE.  AUTHENTICATION, AUTHORIZATION AND VALIDATION MUST BE HANDLED PRIOR TO DB INTERACTION.
        /// <para />
        /// Loads a single person from the database and sets those fields to null that the client is not allowed to return.  If the client requests their own profile, all fields are returned.
        /// <para />
        /// Client Parameters: <para />
        ///     personid - The ID of the person to load.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        [EndpointMethod(EndpointName = "LoadPerson", AllowArgumentLogging = true, AllowResponseLogging = true, RequiresAuthentication = true)]
        private static void EndpointMethod_LoadPerson(MessageToken token)
        {

            //Just make sure the client is logged in.  The endpoint's description should've handled this but you never know.
            if (token.AuthenticationSession == null)
            {
                token.AddErrorMessage("You must be logged in to load persons.", ErrorTypes.Authentication, System.Net.HttpStatusCode.Unauthorized);
                return;
            }

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

            using (var session = DataAccess.NHibernateHelper.CreateStatefulSession())
            {
                //Now let's load the person and then set any fields the client isn't allowed to see to null.
                var person = session.Get<Person>(personId);

                //If person is null then we need to stop here.
                if (person == null)
                {
                    token.AddErrorMessage("The Id you sent appears to be invalid.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                    return;
                }

                Dictionary<string, object> returnData = new Dictionary<string, object>();

                List<string> returnableFields = new PersonAuthorizer().GetAuthorizedProperties(token.AuthenticationSession.Person, person, AuthorizationRuleCategoryEnum.Return);

                var personMetadata = DataAccess.NHibernateHelper.GetEntityMetadata("Person");

                //Now just set the fields the client is allowed to see.
                foreach (var propertyName in returnableFields)
                {
                    //There's a stupid thing with NHibernate where it sees Ids as, well... Ids instead of as Properties.  So we do need a special case for it.
                    if (propertyName.ToLower() == "id")
                    {
                        returnData.Add("Id", personMetadata.GetIdentifier(person, NHibernate.EntityMode.Poco));
                    }
                    else
                    {
                        bool wasSet = false;

                        switch (propertyName.ToLower())
                        {
                            case "command":
                            case "department":
                            case "division":
                                {
                                    returnData.Add(propertyName, NHibernateHelper.GetIdentifier(personMetadata.GetPropertyValue(person, propertyName, NHibernate.EntityMode.Poco)));

                                    wasSet = true;
                                    break;
                                }

                        }

                        if (!wasSet)
                        {
                            returnData.Add(propertyName, personMetadata.GetPropertyValue(person, propertyName, NHibernate.EntityMode.Poco));
                        }

                    }
                }

                //We also need to tell the client what they can edit.
                List<string> editableFields = new PersonAuthorizer().GetAuthorizedProperties(token.AuthenticationSession.Person, person, AuthorizationRuleCategoryEnum.Edit);

                token.SetResult(new
                {
                    Person = returnData,
                    IsMyProfile = token.AuthenticationSession.Person.Id == person.Id,
                    EditableFields = editableFields,
                    ReturnableFields = returnableFields
                });
            }
        }

        /// <summary>
        /// WARNING!  THIS METHOD IS EXPOSED TO THE CLIENT AND IS NOT INTENDED FOR INTERNAL USE.  AUTHENTICATION, AUTHORIZATION AND VALIDATION MUST BE HANDLED PRIOR TO DB INTERACTION.
        /// <para />
        /// Returns all account history for the given person.  These are the login events, etc.
        /// <para />
        /// Client Parameters: <para />
        ///     personid - The ID of the person for whom to return the account historiiiiies.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        [EndpointMethod(AllowArgumentLogging = true, AllowResponseLogging = true, EndpointName = "LoadAccountHistoryByPerson", RequiresAuthentication = true)]
        private static void EndpointMethod_LoadAccountHistoryByPerson(MessageToken token)
        {
            //Just make sure the client is logged in.  The endpoint's description should've handled this but you never know.
            if (token.AuthenticationSession == null)
            {
                token.AddErrorMessage("You must be logged in to view the news.", ErrorTypes.Authentication, System.Net.HttpStatusCode.Unauthorized);
                return;
            }

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

            using (var session = DataAccess.NHibernateHelper.CreateStatefulSession())
            {
                token.SetResult(session.Get<Person>(personId).AccountHistory);
            }
        }

        /// <summary>
        /// WARNING!  THIS METHOD IS EXPOSED TO THE CLIENT AND IS NOT INTENDED FOR INTERNAL USE.  AUTHENTICATION, AUTHORIZATION AND VALIDATION MUST BE HANDLED PRIOR TO DB INTERACTION.
        /// <para />
        /// Conducts a simple search.  Simple search uses a list of search terms and returns all those rows in which each term appears at least once in each of the search fields.
        /// <para/>
        /// In this case those fields are FirstName, LastName, MiddleName, UIC, Paygrade, Designation, Command, Department and Division.
        /// <para />
        /// Client Parameters: <para />
        ///     searchterm - A single string in which the search terms are broken up by a space.  Intended to be the exact input as given by the user.  This string will be split into an array of search terms by all whitespace.  The search terms are parameterized and no scrubbing of the user input is necessary.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        [EndpointMethod(EndpointName = "SimpleSearchPersons", AllowArgumentLogging = true, AllowResponseLogging = true, RequiresAuthentication = true)]
        private static void EndpointMethod_SimpleSearchPersons(MessageToken token)
        {
            //Just make sure the client is logged in.  The endpoint's description should've handled this but you never know.
            if (token.AuthenticationSession == null)
            {
                token.AddErrorMessage("You must be logged in to view the news.", ErrorTypes.Authentication, System.Net.HttpStatusCode.Unauthorized);
                return;
            }

            //Make sure the client has permission to search persons.
            if (!token.AuthenticationSession.Person.HasSpecialPermissions(SpecialPermissions.SearchPersons))
            {
                token.AddErrorMessage("You do not have permission to search persons.", ErrorTypes.Authorization, System.Net.HttpStatusCode.Unauthorized);
                return;
            }

            //If you can search persons then we'll assume you can search/return the required fields.
            
            if (!token.Args.ContainsKey("searchterm"))
            {
                token.AddErrorMessage("You did not send a 'searchterm' parameter.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                return;
            }

            string searchTerm = token.Args["searchterm"] as string;

            //Let's require a search term.  That's nice.
            if (String.IsNullOrEmpty(searchTerm))
            {
                token.AddErrorMessage("You must send a search term. A blank term isn't valid. Sorry :(", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                return;
            }
            
            //And now we're going to split the search term by any white space into a list of search terms.
            var searchTerms = searchTerm.Split((char[])null);

            using (var session = DataAccess.NHibernateHelper.CreateStatefulSession())
            {
                //Build the query over simple search for each of the search terms.  It took like a fucking week to learn to write simple search in NHibernate.
                var queryOver = session.QueryOver<Person>();
                foreach (string term in searchTerms)
                {
                    queryOver = queryOver.Where(Restrictions.Disjunction()
                        .Add<Person>(x => x.LastName.IsInsensitiveLike(term, MatchMode.Anywhere))
                        .Add<Person>(x => x.FirstName.IsInsensitiveLike(term, MatchMode.Anywhere))
                        .Add<Person>(x => x.MiddleName.IsInsensitiveLike(term, MatchMode.Anywhere))
                        //.Add(Restrictions.InsensitiveLike(Projections.Property<Person>(x => x.Paygrade), term, MatchMode.Anywhere))
                        .Add(Subqueries.WhereProperty<Person>(x => x.Designation.Id).In(QueryOver.Of<Designation>().WhereRestrictionOn(x => x.Value).IsInsensitiveLike(term, MatchMode.Anywhere).Select(x => x.Id)))
                        .Add(Subqueries.WhereProperty<Person>(x => x.UIC.Id).In(QueryOver.Of<UIC>().WhereRestrictionOn(x => x.Value).IsInsensitiveLike(term, MatchMode.Anywhere).Select(x => x.Id)))
                        .Add(Subqueries.WhereProperty<Person>(x => x.Command.Id).In(QueryOver.Of<Command>().WhereRestrictionOn(x => x.Value).IsInsensitiveLike(term, MatchMode.Anywhere).Select(x => x.Id)))
                        .Add(Subqueries.WhereProperty<Person>(x => x.Department.Id).In(QueryOver.Of<Department>().WhereRestrictionOn(x => x.Value).IsInsensitiveLike(term, MatchMode.Anywhere).Select(x => x.Id)))
                        .Add(Subqueries.WhereProperty<Person>(x => x.Division.Id).In(QueryOver.Of<Division>().WhereRestrictionOn(x => x.Value).IsInsensitiveLike(term, MatchMode.Anywhere).Select(x => x.Id))));
                }

                //And finally, return the results.  We need to project them into only what we want to send to the client so as to remove them from the proxy shit that NHibernate has sullied them with.
                var results = queryOver.List().Select(x =>
                {
                    return new
                    {
                        x.Id,
                        x.LastName,
                        x.MiddleName,
                        x.FirstName,
                        Paygrade = x.Paygrade,
                        Designation = (x.Designation == null) ? "" : x.Designation.Value,
                        UIC = (x.UIC == null) ? "" : x.UIC.Value,
                        Command = (x.Command == null) ? "" : x.Command.Value,
                        Department = (x.Department == null) ? "" : x.Department.Value,
                        Division = (x.Division == null) ? "" : x.Division.Value
                    };
                });

                token.SetResult(new 
                { 
                    Results = results,
                    Fields = new[] { "FirstName", "MiddleName", "LastName", "Paygrade", "Designation", "UIC", "Command", "Department", "Division" } 
                });
            }
        }

        /// <summary>
        /// WARNING!  THIS METHOD IS EXPOSED TO THE CLIENT AND IS NOT INTENDED FOR INTERNAL USE.  AUTHENTICATION, AUTHORIZATION AND VALIDATION MUST BE HANDLED PRIOR TO DB INTERACTION.
        /// <para />
        /// Conducts an advanced search.  Advanced search uses a series of key/value pairs where the key is a property to be searched and the value is a string of text which make up the search terms in
        /// a simple search across the property.
        /// <para />
        /// Client Parameters: <para />
        ///     filters - The properties to search and the values to search for.
        ///     returnfields - The fields the client would like returned.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        [EndpointMethod(EndpointName = "AdvancedSearchPersons", AllowArgumentLogging = true, AllowResponseLogging = true, RequiresAuthentication = true)]
        private static void EndpointMethod_AdvancedSearchPersons(MessageToken token)
        {
            //Just make sure the client is logged in.  The endpoint's description should've handled this but you never know.
            if (token.AuthenticationSession == null)
            {
                token.AddErrorMessage("You must be logged in to view the news.", ErrorTypes.Authentication, System.Net.HttpStatusCode.Unauthorized);
                return;
            }

            //Make sure the client has permission to search persons.
            if (!token.AuthenticationSession.Person.HasSpecialPermissions(SpecialPermissions.SearchPersons))
            {
                token.AddErrorMessage("You do not have permission to search persons.", ErrorTypes.Authorization, System.Net.HttpStatusCode.Unauthorized);
                return;
            }

            //Let's find which fields the client wants to search in.  This should be a dictionary.
            if (!token.Args.ContainsKey("filters"))
            {
                token.AddErrorMessage("You didn't send a 'filters' parameter.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                return;
            }
            Dictionary<string, string> filters = token.Args["filters"].CastJToken<Dictionary<string, string>>();

            //Alright, now let's make sure the client is allowed to search in all of these fields.
            if (!token.AuthenticationSession.Person.CanSearchFields("Person", filters.Select(x => x.Key).ToArray()))
            {
                token.AddErrorMessage("You were not allowed to search in one or more of the fields you asked to search in.", ErrorTypes.Authorization, System.Net.HttpStatusCode.Unauthorized);
                return;
            }

            //And the fields the client wants to return.
            if (!token.Args.ContainsKey("returnfields"))
            {
                token.AddErrorMessage("You didn't send a 'returnfields' parameter.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                return;
            }
            List<string> returnFields = token.Args["returnfields"].CastJToken<List<string>>();

            //We're going to need the person object's metadata for the rest of this.
            var personMetadata = DataAccess.NHibernateHelper.GetEntityMetadata("Person");

            using (var session = DataAccess.NHibernateHelper.CreateStatefulSession())
            {
                //Ok the client can search and return everything.  Now we need to build the actual query.
                //To do this we need to determine what type each property is and then add it to the query.
                var queryOver = session.QueryOver<Person>();
                foreach (var filter in filters)
                {
                    var searchTerms = filter.Value.Split((char[])null);

                    var property = personMetadata.GetPropertyType(filter.Key);

                    //If it's any besides a basic type, then we need to declare the search strategy for each one.
                    if (property.IsAssociationType || property.IsCollectionType || property.IsComponentType)
                    {
                        //For now we're going to provide options for every property and a default.
                        switch (filter.Key)
                        {
                            case "Designation":
                                {
                                    var disjunction = Restrictions.Disjunction();
                                    foreach (var term in searchTerms)
                                        disjunction.Add(Subqueries.WhereProperty<Person>(x => x.Designation.Id).In(QueryOver.Of<Designation>().WhereRestrictionOn(x => x.Value).IsInsensitiveLike(term, MatchMode.Anywhere).Select(x => x.Id)));
                                    queryOver = queryOver.Where(disjunction);
                                    break;
                                }
                            case "UIC":
                                {
                                    var disjunction = Restrictions.Disjunction();
                                    foreach (var term in searchTerms)
                                        disjunction.Add(Subqueries.WhereProperty<Person>(x => x.UIC.Id).In(QueryOver.Of<UIC>().WhereRestrictionOn(x => x.Value).IsInsensitiveLike(term, MatchMode.Anywhere).Select(x => x.Id)));
                                    queryOver = queryOver.Where(disjunction);
                                    break;
                                }
                            case "Command":
                                {
                                    var disjunction = Restrictions.Disjunction();
                                    foreach (var term in searchTerms)
                                        disjunction.Add(Subqueries.WhereProperty<Person>(x => x.Command.Id).In(QueryOver.Of<Command>().WhereRestrictionOn(x => x.Value).IsInsensitiveLike(term, MatchMode.Anywhere).Select(x => x.Id)));
                                    queryOver = queryOver.Where(disjunction);
                                    break;
                                }
                            case "Department":
                                {
                                    var disjunction = Restrictions.Disjunction();
                                    foreach (var term in searchTerms)
                                        disjunction.Add(Subqueries.WhereProperty<Person>(x => x.Department.Id).In(QueryOver.Of<Department>().WhereRestrictionOn(x => x.Value).IsInsensitiveLike(term, MatchMode.Anywhere).Select(x => x.Id)));
                                    queryOver = queryOver.Where(disjunction);
                                    break;
                                }
                            case "Division":
                                {
                                    var disjunction = Restrictions.Disjunction();
                                    foreach (var term in searchTerms)
                                        disjunction.Add(Subqueries.WhereProperty<Person>(x => x.Division.Id).In(QueryOver.Of<Division>().WhereRestrictionOn(x => x.Value).IsInsensitiveLike(term, MatchMode.Anywhere).Select(x => x.Id)));
                                    queryOver = queryOver.Where(disjunction);
                                    break;
                                }
                            case "Ethnicity":
                                {
                                    var disjunction = Restrictions.Disjunction();
                                    foreach (var term in searchTerms)
                                        disjunction.Add(Subqueries.WhereProperty<Person>(x => x.Ethnicity.Id).In(QueryOver.Of<Ethnicity>().WhereRestrictionOn(x => x.Value).IsInsensitiveLike(term, MatchMode.Anywhere).Select(x => x.Id)));
                                    queryOver = queryOver.Where(disjunction);
                                    break;
                                }
                            case "ReligiousPreference":
                                {
                                    var disjunction = Restrictions.Disjunction();
                                    foreach (var term in searchTerms)
                                        disjunction.Add(Subqueries.WhereProperty<Person>(x => x.ReligiousPreference.Id).In(QueryOver.Of<ReligiousPreference>().WhereRestrictionOn(x => x.Value).IsInsensitiveLike(term, MatchMode.Anywhere).Select(x => x.Id)));
                                    queryOver = queryOver.Where(disjunction);
                                    break;
                                }
                            case "Billet":
                                {
                                    //If the client is searching in billet, then it's a simple search, meaning we have to split the search term.

                                    foreach (string term in searchTerms)
                                    {
                                        queryOver = queryOver.Where(Subqueries.WhereProperty<Person>(x => x.Billet.Id).In(QueryOver.Of<Billet>().Where(Restrictions.Disjunction()
                                        .Add<Billet>(x => x.Designation.IsInsensitiveLike(term, MatchMode.Anywhere))
                                        .Add<Billet>(x => x.Funding.IsInsensitiveLike(term, MatchMode.Anywhere))
                                        .Add<Billet>(x => x.IdNumber.IsInsensitiveLike(term, MatchMode.Anywhere))
                                        .Add<Billet>(x => x.Remarks.IsInsensitiveLike(term, MatchMode.Anywhere))
                                        .Add<Billet>(x => x.SuffixCode.IsInsensitiveLike(term, MatchMode.Anywhere))
                                        .Add<Billet>(x => x.Title.IsInsensitiveLike(term, MatchMode.Anywhere))
                                        .Add(Subqueries.WhereProperty<Billet>(x => x.NEC.Id).In(QueryOver.Of<NEC>().WhereRestrictionOn(x => x.Value).IsInsensitiveLike(term, MatchMode.Anywhere).Select(x => x.Id)))
                                        .Add(Subqueries.WhereProperty<Billet>(x => x.UIC.Id).In(QueryOver.Of<UIC>().WhereRestrictionOn(x => x.Value).IsInsensitiveLike(term, MatchMode.Anywhere).Select(x => x.Id)))).Select(x => x.Id)));
                                    }

                                    break;
                                }
                            case "NECs":
                                {
                                    var disjunction = Restrictions.Disjunction();
                                    foreach (var term in searchTerms)
                                        disjunction.Add(Restrictions.Where<Person>(x => x.NECs.Any(nec => nec.Value.IsInsensitiveLike(term, MatchMode.Anywhere))));
                                    queryOver = queryOver.Where(disjunction);
                                    break;
                                }
                            case "EmailAddresses":
                                {
                                    EmailAddress addressAlias = null;
                                    queryOver = queryOver
                                        .JoinAlias(x => x.EmailAddresses, () => addressAlias)
                                        .Fetch(x => x.EmailAddresses).Eager;

                                    var disjunction = Restrictions.Disjunction();
                                    foreach (var term in searchTerms)
                                        disjunction.Add(() => addressAlias.Address.IsInsensitiveLike(term, MatchMode.Anywhere));
                                    queryOver = queryOver.Where(disjunction);
                                    break;
                                }
                            case "PhysicalAddresses":
                                {

                                    //Physical addresses allow a simple search across the object.

                                    foreach (string term in searchTerms)
                                    {
                                        queryOver = queryOver.Where(x =>
                                            x.PhysicalAddresses.Any(
                                                physicalAddress =>
                                                    physicalAddress.City.IsInsensitiveLike(term, MatchMode.Anywhere) ||
                                                    physicalAddress.Country.IsInsensitiveLike(term, MatchMode.Anywhere) ||
                                                    physicalAddress.Route.IsInsensitiveLike(term, MatchMode.Anywhere) ||
                                                    physicalAddress.State.IsInsensitiveLike(term, MatchMode.Anywhere) ||
                                                    physicalAddress.StreetNumber.IsInsensitiveLike(term, MatchMode.Anywhere) ||
                                                    physicalAddress.ZipCode.IsInsensitiveLike(term, MatchMode.Anywhere)));
                                    }

                                    break;
                                }
                            case "PhoneNumbers":
                                {
                                    var disjunction = Restrictions.Disjunction();
                                    foreach (var term in searchTerms)
                                        disjunction.Add(Restrictions.Where<Person>(x => x.PhoneNumbers.Any(phoneNumber => phoneNumber.Number.IsInsensitiveLike(term, MatchMode.Anywhere))));
                                    queryOver = queryOver.Where(disjunction);
                                    break;
                                }
                            case "PermissionGroups":
                                {
                                    var disjunction = Restrictions.Disjunction();
                                    foreach (var term in searchTerms)
                                        disjunction.Add(Restrictions.Where<Person>(x => x.PermissionGroups.Any(perm => perm.Name.IsInsensitiveLike(term, MatchMode.Anywhere))));
                                    queryOver = queryOver.Where(disjunction);
                                    break;
                                }
                            case "SubscribedChangeEvents":
                                {
                                    var disjunction = Restrictions.Disjunction();
                                    foreach (var term in searchTerms)
                                        disjunction.Add(Restrictions.Where<Person>(x => x.SubscribedChangeEvents.Any(changeEvent => changeEvent.Name.IsInsensitiveLike(term, MatchMode.Anywhere))));
                                    queryOver = queryOver.Where(disjunction);
                                    break;
                                }
                            case "CurrentMusterStatus":
                                {
                                    //A search in current muster status is a simple search across multiple fields with a filter parameter for the current days.
                                    foreach (string term in searchTerms)
                                    {
                                        queryOver = queryOver.Where(Subqueries.WhereProperty<Person>(x => x.CurrentMusterStatus.Id).In(QueryOver.Of<Muster.MusterRecord>().Where(Restrictions.Disjunction()
                                        .Add<Muster.MusterRecord>(x => x.Command.IsInsensitiveLike(term, MatchMode.Anywhere))
                                        .Add<Muster.MusterRecord>(x => x.Department.IsInsensitiveLike(term, MatchMode.Anywhere))
                                        .Add<Muster.MusterRecord>(x => x.Division.IsInsensitiveLike(term, MatchMode.Anywhere))
                                        .Add<Muster.MusterRecord>(x => x.DutyStatus.IsInsensitiveLike(term, MatchMode.Anywhere))
                                        .Add<Muster.MusterRecord>(x => x.MusterStatus.IsInsensitiveLike(term, MatchMode.Anywhere))
                                        .Add<Muster.MusterRecord>(x => x.Paygrade.IsInsensitiveLike(term, MatchMode.Anywhere))
                                        .Add<Muster.MusterRecord>(x => x.UIC.IsInsensitiveLike(term, MatchMode.Anywhere)))
                                        .And(x => x.MusterDayOfYear == Muster.MusterRecord.GetMusterDay(token.CallTime) && x.MusterYear == Muster.MusterRecord.GetMusterYear(token.CallTime))
                                        .Select(x => x.Id)));
                                    }

                                    break;
                                }
                            default:
                                {
                                    //If the client tried to search in something that isn't supported, then fuck em.
                                    token.AddErrorMessage("Your request to search in the field, '{0}', is not supported.  We do not currently provide a search strategy for this property.  If you think we should provide the ability to search in this property, please contact the development team with your suggestion.".FormatS(filter.Key), ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                                    return;
                                }
                        }
                    }
                    else
                    {
                        var disjunction = Restrictions.Disjunction();
                        foreach (var term in searchTerms)
                            disjunction.Add(Restrictions.InsensitiveLike(filter.Key, term, MatchMode.Anywhere));
                        queryOver = queryOver.Where(disjunction);
                    }
                }

                //Here we iterate over every returned person, do an authorization check and cast the results into DTOs.
                //Important note: the client expects every field to be a string.  We don't return object results.
                var result = queryOver.List().Select(returnedPerson =>
                {
                    //We need to know the fields the client is allowed ot return for this client.
                    var returnableFields = new PersonAuthorizer().GetAuthorizedProperties(token.AuthenticationSession.Person, returnedPerson, AuthorizationRuleCategoryEnum.Return);

                    var temp = new Dictionary<string, string>();
                    foreach (var returnField in returnFields)
                    {
                        //if the client isn't allowed to return this field, replace its value with "redacted"
                        if (returnableFields.Contains(returnField))
                        {
                            var value = personMetadata.GetPropertyValue(returnedPerson, returnField, NHibernate.EntityMode.Poco);
                            temp.Add(returnField, value == null ? "" : value.ToString());
                        }
                        else
                        {
                            temp.Add(returnField, "REDACTED");
                        }

                    }
                    //We're also going to append the Id onto every search result so that the client knows who this is.
                    temp.Add("Id", personMetadata.GetIdentifier(returnedPerson, NHibernate.EntityMode.Poco).ToString());
                    return temp;
                });

                token.SetResult(new 
                { 
                    Results = result 
                });
            }
        }

        #endregion

        #region Update

        /// <summary>
        /// WARNING!  THIS METHOD IS EXPOSED TO THE CLIENT AND IS NOT INTENDED FOR INTERNAL USE.  AUTHENTICATION, AUTHORIZATION AND VALIDATION MUST BE HANDLED PRIOR TO DB INTERACTION.
        /// <para />
        /// Given a person, updates the person assuming the person is allowed to edit the properties that have changed.  Additionally, a lock must be owned on the person by the client.
        /// <para />
        /// Client Parameters: <para />
        ///     person - a properly formatted JSON person to be updated.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        [EndpointMethod(EndpointName = "UpdatePerson", AllowArgumentLogging = true, AllowResponseLogging = true, RequiresAuthentication = true)]
        private static void EndpointMethod_UpdatePerson(MessageToken token)
        {

            //First make sure we have a session.
            if (token.AuthenticationSession == null)
            {
                token.AddErrorMessage("You must be logged in to edit a person.", ErrorTypes.Authentication, System.Net.HttpStatusCode.Unauthorized);
                return;
            }

            //Now make sure we have permission to edit users.
            if (!token.AuthenticationSession.Person.HasSpecialPermissions(SpecialPermissions.EditPerson))
            {
                token.AddErrorMessage("You must have permission to edit a person in order to edit a person. lol.", ErrorTypes.Authorization, System.Net.HttpStatusCode.Forbidden);
                return;
            }

            //Ok, now we need to find the person the client sent us and try to parse it into a person.
            if (!token.Args.ContainsKey("person"))
            {
                token.AddErrorMessage("In order to update a person, you must send a person... ", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                return;
            }

            //Try the parse
            Person personFromClient;
            try
            {
                personFromClient = token.Args["person"].CastJToken<Person>();

                if (personFromClient == null)
                {
                    token.AddErrorMessage("An error occurred while trying to parse the person into its proper form.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                    return;
                }
            }
            catch
            {
                token.AddErrorMessage("An error occurred while trying to parse the person into its proper form.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                return;
            }

            //Ok, so since we're read to do ze WORK we're going to do it on a separate session.
            using (var session = NHibernateHelper.CreateStatefulSession())
            using (var transaction = session.BeginTransaction())
            {
                try
                {

                    //Ok now we need to see if a lock exists for the person the client wants to edit.  Later we'll see if the client owns that lock.
                    ProfileLock profileLock = session.QueryOver<ProfileLock>()
                                            .Where(x => x.LockedPerson.Id == personFromClient.Id)
                                            .SingleOrDefault();


                    //If we got no profile lock, then bail
                    if (profileLock == null)
                    {
                        token.AddErrorMessage("In order to edit this person, you must first take a lock on the person.", ErrorTypes.Authorization, System.Net.HttpStatusCode.Forbidden);
                        return;
                    }

                    //Ok, well there is a lock on the person, now let's make sure the client owns that lock.
                    if (profileLock.Owner.Id != token.AuthenticationSession.Person.Id)
                    {
                        token.AddErrorMessage("The lock on this person is owned by '{0}' and will expire in {1} minutes unless the owned closes the profile prior to that.".FormatS(profileLock.Owner.ToString(), profileLock.GetTimeRemaining().TotalMinutes), ErrorTypes.LockOwned, System.Net.HttpStatusCode.Forbidden);
                        return;
                    }

                    //Ok, so it's a valid person and the client owns the lock, now let's load the person by their ID, and see what they look like in the database.
                    Person personFromDB = session.Get<Person>(personFromClient.Id);

                    personFromDB = session.GetSessionImplementation().PersistenceContext.Unproxy(personFromDB) as Person;
                    //session.Evict(personFromDB);


                    //Did we get a person?  If not, the person the client gave us is bullshit.
                    if (personFromDB == null)
                    {
                        token.AddErrorMessage("The person you supplied had an Id that belongs to no actual person.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                        return;
                    }

                    //Get the authorizer we need.
                    var authorizer = new PersonAuthorizer();

                    //Get the editable and returnable fields and also those fields that, even if they are edited, will be ignored.
                    var editableFields = authorizer.GetAuthorizedProperties(token.AuthenticationSession.Person, personFromClient, AuthorizationRuleCategoryEnum.Edit);
                    var returnableFields = authorizer.GetAuthorizedProperties(token.AuthenticationSession.Person, personFromClient, AuthorizationRuleCategoryEnum.Return);
                    var propertiesThatIgnoreEdit = authorizer.GetPropertiesThatIgnoreEdit();

                    //Go through all returnable fields that don't ignore edits and then move the values into the person from the database.
                    foreach (var field in returnableFields.Where(x => !propertiesThatIgnoreEdit.Contains(x)).ToList())
                    {
                        var property = typeof(Person).GetProperty(field);

                        property.SetValue(personFromDB, property.GetValue(personFromClient));
                    }

                    //Determine what changed.
                    var variances = session.GetDirtyProperties(personFromDB).ToList();

                    //Ok, let's validate the entire person object.  This will be what it used to look like plus the changes from the client.
                    var results = new PersonValidator().Validate(personFromDB);

                    //If there are any errors with the validation, let's throw those back to the client.
                    if (results.Errors.Any())
                    {
                        token.AddErrorMessages(results.Errors.Select(x => x.ErrorMessage), ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                        return;
                    }

                    //Ok so the client only changed what they are allowed to see.  Now are those edits authorized.
                    var unauthorizedEdits = variances.Where(x => !editableFields.Contains(x.PropertyName));
                    if (unauthorizedEdits.Any())
                    {
                        token.AddErrorMessages(unauthorizedEdits.Select(x => "You lacked permission to edit the field '{0}'.".FormatS(x.PropertyName)), ErrorTypes.Authorization, System.Net.HttpStatusCode.Forbidden);
                        return;
                    }

                    //Ok, so the client is authorized to edit all the fields that changed.  Let's submit the update to the database.
                    session.Merge(personFromDB);

                    //And then we're done!
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

        #endregion

        #region Startup Methods

        /// <summary>
        /// Loads all persons from the database, thus initializing most of the 2nd level cache, and tells the host how many persons we have in the database.
        /// </summary>
        [ServiceManagement.StartMethod(Priority = 7)]
        private static void ReadPersons()
        {
            using (var session = DataAccess.NHibernateHelper.CreateStatefulSession())
            {
                var persons = session.QueryOver<Person>().List();

                Communicator.PostMessageToHost("Found {0} persons.".FormatS(persons.Count), Communicator.MessageTypes.Informational);
            }
        }

        #endregion

        /// <summary>
        /// Declares authorization rules for the person object and its returnable and editable fields.  Used in conjunction with permission groups, defines unified rules for access to a person object.
        /// </summary>
        public class PersonAuthorizer : AbstractAuthorizer<Person>
        {
            /// <summary>
            /// Declares authorization rules for the person object and its returnable and editable fields.  Used in conjunction with permission groups, defines unified rules for access to a person object.
            /// </summary>
            public PersonAuthorizer()
            {
                RulesFor(x => x.Id).MakeIgnoreGenericEdits()
                    .Returnable()
                        .ForEveryone()
                    .Editable()
                        .Never();

                //Properties in this group are updated by other endpoints and not the generic update endpoint.
                RulesFor(x => x.IsClaimed).MakeIgnoreGenericEdits()
                .AndFor(x => x.Changes)
                .AndFor(x => x.AccountHistory)
                .AndFor(x => x.CurrentMusterStatus)
                    .Returnable()
                        .IfGrantedByPermissionGroup().Or().IfSelf()
                    .Editable()
                        .Never();

                RulesFor(x => x.PasswordHash)
                    .Returnable()
                        .Never()
                    .Editable()
                        .IfSelf();

                RulesFor(x => x.FirstName)
                .AndFor(x => x.LastName)
                .AndFor(x => x.MiddleName)
                .AndFor(x => x.DateOfBirth)
                .AndFor(x => x.Sex)
                .AndFor(x => x.Remarks)
                .AndFor(x => x.Ethnicity)
                .AndFor(x => x.ReligiousPreference)
                .AndFor(x => x.Suffix)
                .AndFor(x => x.Designation)
                .AndFor(x => x.Supervisor)
                .AndFor(x => x.WorkCenter)
                .AndFor(x => x.WorkRoom)
                .AndFor(x => x.Shift)
                .AndFor(x => x.WorkRemarks)
                .AndFor(x => x.DateOfArrival)
                .AndFor(x => x.JobTitle)
                .AndFor(x => x.EmailAddresses)
                .AndFor(x => x.PhoneNumbers)
                .AndFor(x => x.PhysicalAddresses)
                .AndFor(x => x.ContactRemarks)
                    .Returnable()
                        .IfSelf().Or().IfGrantedByPermissionGroup()
                    .Editable()
                        .IfSelf().Or().IfInCoCAndInPermissionGroup();

                RulesFor(x => x.SSN)
                    .Returnable()
                        .IfSelf().Or().IfInCoCAndInPermissionGroup()
                    .Editable()
                        .IfSelf().Or().IfInCoCAndInPermissionGroup();


                RulesFor(x => x.Paygrade)
                .AndFor(x => x.Division)
                .AndFor(x => x.Department)
                .AndFor(x => x.Command)
                    .Returnable()
                        .IfSelf().Or().IfGrantedByPermissionGroup()
                    .Editable()
                        .IfInCoCAndInPermissionGroup();

                RulesFor(x => x.Billet)
                .AndFor(x => x.NECs)
                .AndFor(x => x.DutyStatus)
                .AndFor(x => x.UIC)
                .AndFor(x => x.EAOS)
                .AndFor(x => x.DateOfDeparture)
                    .Returnable()
                        .IfGrantedByPermissionGroup()
                    .Editable()
                        .IfHasSpecialPermission(SpecialPermissions.ManPowerAdmin)
                        .And()
                        .IfGrantedByPermissionGroup();

                RulesFor(x => x.EmergencyContactInstructions)
                .AndFor(x => x.Username)
                .AndFor(x => x.SubscribedChangeEvents)
                    .Returnable()
                        .IfGrantedByPermissionGroup()
                    .Editable()
                        .IfSelf()
                        .And()
                        .IfGrantedByPermissionGroup();

                RulesFor(x => x.PermissionGroups).MakeIgnoreGenericEdits()
                    .Returnable()
                        .IfGrantedByPermissionGroup()
                    .Editable()
                        .Never();
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
                Id(x => x.Id).GeneratedBy.Assigned();

                References(x => x.Ethnicity).Nullable().LazyLoad(Laziness.False).Cascade.All();
                References(x => x.ReligiousPreference).Nullable().LazyLoad(Laziness.False).Cascade.All();
                References(x => x.Designation).Nullable().LazyLoad(Laziness.False).Cascade.All();
                References(x => x.UIC).Nullable().LazyLoad(Laziness.False).Cascade.All();
                References(x => x.Division).Nullable().LazyLoad(Laziness.False);
                References(x => x.Department).Nullable().LazyLoad(Laziness.False);
                References(x => x.Command).Nullable().LazyLoad(Laziness.False);
                References(x => x.Billet).Nullable().LazyLoad(Laziness.False);
                References(x => x.CurrentMusterStatus).Cascade.All().Nullable().LazyLoad(Laziness.False);

                Map(x => x.DutyStatus).Not.Nullable().Not.LazyLoad();
                Map(x => x.Paygrade).Not.Nullable().CustomType<NHibernate.Type.EnumStringType<Paygrades>>().Not.LazyLoad();
                Map(x => x.Sex).Not.Nullable().Not.LazyLoad();
                Map(x => x.LastName).Not.Nullable().Length(40).Not.LazyLoad();
                Map(x => x.FirstName).Not.Nullable().Length(40).Not.LazyLoad();
                Map(x => x.MiddleName).Nullable().Length(40).Not.LazyLoad();
                Map(x => x.SSN).Not.Nullable().Length(40).Unique().Not.LazyLoad();
                Map(x => x.DateOfBirth).Not.Nullable().Not.LazyLoad();
                Map(x => x.Remarks).Nullable().Length(150).Not.LazyLoad();
                Map(x => x.Supervisor).Nullable().Length(40).Not.LazyLoad();
                Map(x => x.WorkCenter).Nullable().Length(40).Not.LazyLoad();
                Map(x => x.WorkRoom).Nullable().Length(40).Not.LazyLoad();
                Map(x => x.Shift).Nullable().Length(40).Not.LazyLoad();
                Map(x => x.WorkRemarks).Nullable().Length(150).Not.LazyLoad();
                Map(x => x.DateOfArrival).Not.Nullable().Not.LazyLoad();
                Map(x => x.JobTitle).Nullable().Length(40).Not.LazyLoad();
                Map(x => x.EAOS).Nullable().Not.LazyLoad();
                Map(x => x.DateOfDeparture).Nullable().Not.LazyLoad();
                Map(x => x.EmergencyContactInstructions).Nullable().Length(150).Not.LazyLoad();
                Map(x => x.ContactRemarks).Nullable().Length(150).Not.LazyLoad();
                Map(x => x.IsClaimed).Not.Nullable().Default(false.ToString()).Not.LazyLoad();
                Map(x => x.Username).Nullable().Length(40).Unique().Not.LazyLoad();
                Map(x => x.PasswordHash).Nullable().Length(100).Not.LazyLoad();
                Map(x => x.Suffix).Nullable().Length(40).Not.LazyLoad();

                HasManyToMany(x => x.NECs).Not.LazyLoad();
                HasManyToMany(x => x.PermissionGroups).Not.LazyLoad();
                HasManyToMany(x => x.SubscribedChangeEvents).Not.LazyLoad();

                HasMany(x => x.AccountHistory).Not.LazyLoad().Cascade.All();
                HasMany(x => x.Changes).Not.LazyLoad().Cascade.All();
                HasMany(x => x.EmailAddresses).Not.LazyLoad().Cascade.All();
                HasMany(x => x.PhoneNumbers).Not.LazyLoad().Cascade.All();
                HasMany(x => x.PhysicalAddresses).Not.LazyLoad().Cascade.All();
            }
        }

        /// <summary>
        /// Validates a person object.
        /// </summary>
        public class PersonValidator : AbstractValidator<Person>
        {
            /// <summary>
            /// Validates a person object.
            /// </summary>
            public PersonValidator()
            {
                RuleFor(x => x.Id).NotEmpty();
                RuleFor(x => x.LastName).NotEmpty().Length(1, 40)
                    .WithMessage("The last name must not be left blank and must not exceed 40 characters.");
                RuleFor(x => x.FirstName).Length(0, 40)
                    .WithMessage("The first name must not exceed 40 characters.");
                RuleFor(x => x.MiddleName).Length(0, 40)
                    .WithMessage("The middle name must not exceed 40 characters.");
                RuleFor(x => x.Suffix).Length(0, 40)
                    .WithMessage("The suffix must not exceed 40 characters.");
                RuleFor(x => x.SSN).Must(x => System.Text.RegularExpressions.Regex.IsMatch(x, @"^(?!\b(\d)\1+-(\d)\1+-(\d)\1+\b)(?!123-45-6789|219-09-9999|078-05-1120)(?!666|000|9\d{2})\d{3}(?!00)\d{2}(?!0{4})\d{4}$"))
                    .WithMessage("The SSN must be valid and contain only numbers.");
                RuleFor(x => x.DateOfBirth).NotEmpty()
                    .WithMessage("The DOB must not be left blank.");
                RuleFor(x => x.Sex).NotNull()
                    .WithMessage("The sex must not be left blank.");
                RuleFor(x => x.Remarks).Length(0, 150)
                    .WithMessage("Remarks must not exceed 150 characters.");
                RuleFor(x => x.Ethnicity).Must(x =>
                    {
                        if (x == null)
                            return true;

                        Ethnicity ethnicity = DataAccess.NHibernateHelper.CreateStatefulSession().Get<Ethnicity>(x.Id);

                        if (ethnicity == null)
                            return false;

                        return ethnicity.Equals(x);
                    })
                    .WithMessage("The ethnicity wasn't valid.  It must match exactly a list item in the database.");
                RuleFor(x => x.ReligiousPreference).Must(x =>
                    {
                        if (x == null)
                            return true;

                        ReligiousPreference pref = DataAccess.NHibernateHelper.CreateStatefulSession().Get<ReligiousPreference>(x.Id);

                        if (pref == null)
                            return false;

                        return pref.Equals(x);
                    })
                    .WithMessage("The religious preference wasn't valid.  It must match exactly a list item in the database.");
                RuleFor(x => x.Designation).Must(x =>
                    {
                        if (x == null)
                            return true;

                        Designation designation = DataAccess.NHibernateHelper.CreateStatefulSession().Get<Designation>(x.Id);

                        if (designation == null)
                            return false;

                        return designation.Equals(x);
                    })
                    .WithMessage("The designation wasn't valid.  It must match exactly a list item in the database.");
                RuleFor(x => x.Division).Must(x =>
                    {
                        if (x == null)
                            return true;

                        Division division = DataAccess.NHibernateHelper.CreateStatefulSession().Get<Division>(x.Id);

                        if (division == null)
                            return false;

                        return division.Equals(x);
                    })
                    .WithMessage("The division wasn't a valid division.  It must match exactly.");
                RuleFor(x => x.Department).Must(x =>
                    {
                        if (x == null)
                            return true;

                        Department department = DataAccess.NHibernateHelper.CreateStatefulSession().Get<Department>(x.Id);

                        if (department == null)
                            return false;

                        return department.Equals(x);
                    })
                    .WithMessage("The department was invalid.");
                RuleFor(x => x.Command).Must(x =>
                    {
                        if (x == null)
                            return true;

                        Command command = DataAccess.NHibernateHelper.CreateStatefulSession().Get<Command>(x.Id);

                        if (command == null)
                            return false;

                        return command.Equals(x);
                    })
                    .WithMessage("The command was invalid.");
                RuleFor(x => x.Billet).Must(x =>
                    {
                        if (x == null)
                            return true;

                        Billet billet = DataAccess.NHibernateHelper.CreateStatefulSession().Get<Billet>(x.Id);

                        if (billet == null)
                            return false;

                        return billet.Equals(x);
                    });
                RuleForEach(x => x.NECs).Must(x =>
                    {
                        if (x == null)
                            return true;

                        NEC nec = DataAccess.NHibernateHelper.CreateStatefulSession().Get<NEC>(x.Id);

                        if (nec == null)
                            return false;

                        return nec.Equals(x);
                    });
                RuleFor(x => x.Supervisor).Length(0, 40)
                    .WithMessage("The supervisor field may not be longer than 40 characters.");
                RuleFor(x => x.WorkCenter).Length(0, 40)
                    .WithMessage("The work center field may not be longer than 40 characters.");
                RuleFor(x => x.WorkRoom).Length(0, 40)
                    .WithMessage("The work room field may not be longer than 40 characters.");
                RuleFor(x => x.Shift).Length(0, 40)
                    .WithMessage("The shift field may not be longer than 40 characters.");
                RuleFor(x => x.WorkRemarks).Length(0, 150)
                    .WithMessage("The work remarks field may not be longer than 150 characters.");
                RuleFor(x => x.UIC).Must(x =>
                    {
                        if (x == null)
                            return true;

                        UIC uic = DataAccess.NHibernateHelper.CreateStatefulSession().Get<UIC>(x.Id);

                        if (uic == null)
                            return false;

                        return uic.Equals(x);
                    })
                    .WithMessage("The UIC was invalid.");
                RuleFor(x => x.JobTitle).Length(0, 40)
                    .WithMessage("The job title may not be longer than 40 characters.");


                //Set validations
                RuleFor(x => x.EmailAddresses)
                    .SetCollectionValidator(new EmailAddress.EmailAddressValidator());
                RuleFor(x => x.PhoneNumbers)
                    .SetCollectionValidator(new PhoneNumber.PhoneNumberValidator());
                RuleFor(x => x.PhysicalAddresses)
                    .SetCollectionValidator(new PhysicalAddress.PhysicalAddressValidator());
               



            }

        }

    }

}
