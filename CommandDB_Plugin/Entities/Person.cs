using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentNHibernate.Mapping;
using CommandCentral.ClientAccess;
using NHibernate.Criterion;

namespace CommandCentral.Entities
{
    /// <summary>
    /// Describes a single person and all their properties and data access methods.
    /// </summary>
    public class Person : IExposable
    {

        #region Properties

        /// <summary>
        /// The person's unique ID.
        /// </summary>
        public virtual string ID { get; set; }

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
        public virtual ReferenceLists.Sex Sex { get; set; }

        /// <summary>
        /// The person's remarks.  This is the primary comments section
        /// </summary>
        public virtual string Remarks { get; set; }

        /// <summary>
        /// Stores the person's ethnicity.
        /// </summary>
        public virtual ReferenceLists.Ethnicity Ethnicity { get; set; }

        /// <summary>
        /// The person's religious preference
        /// </summary>
        public virtual ReferenceLists.ReligiousPreference ReligiousPreference { get; set; }

        /// <summary>
        /// The person's suffix, sch as IV, Esquire, etc.
        /// </summary>
        public virtual ReferenceLists.Suffix Suffix { get; set; }

        /// <summary>
        /// The person's rank (e5, etc.)
        /// </summary>
        public virtual ReferenceLists.Rank Rank { get; set; }

        /// <summary>
        /// The person's rate (CTI2, CTR1)
        /// </summary>
        public virtual ReferenceLists.Rate Rate { get; set; }

        /// <summary>
        /// The person's division
        /// </summary>
        public virtual ReferenceLists.Division Division { get; set; }

        /// <summary>
        /// The person's department
        /// </summary>
        public virtual ReferenceLists.Department Department { get; set; }

        /// <summary>
        /// The person's command
        /// </summary>
        public virtual ReferenceLists.Command Command { get; set; }

        #endregion

        #region Work Properties

        /// <summary>
        ///The person's billet.
        /// </summary>
        public virtual Billet Billet { get; set; }

        /// <summary>
        /// The NECs of the person.
        /// </summary>
        public virtual List<ReferenceLists.NEC> NECs { get; set; }

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
        public virtual ReferenceLists.DutyStatus DutyStatus { get; set; }

        /// <summary>
        /// The person's UIC
        /// </summary>
        public virtual ReferenceLists.UIC UIC { get; set; }

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
        public virtual List<EmailAddress> EmailAddresses { get; set; }

        /// <summary>
        /// The Phone Numbers of this person.
        /// </summary>
        public virtual List<PhoneNumber> PhoneNumbers { get; set; }

        /// <summary>
        /// The Physical Addresses of this person
        /// </summary>
        public virtual List<PhysicalAddress> PhysicalAddresses { get; set; }

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
        public virtual List<Authorization.PermissionGroup> PermissionGroups { get; set; }

        /// <summary>
        /// The list of change events to which the person is subscribed.
        /// </summary>
        public virtual List<ChangeEvent> SubscribedChangeEvents { get; set; }

        /// <summary>
        /// A list containing account history events, these are events that track things like login, password reset, etc.
        /// </summary>
        public virtual List<AccountHistoryEvent> AccountHistory { get; set; }

        /// <summary>
        /// A list containing all changes that have every occurred to the profile.
        /// </summary>
        public virtual List<Change> Changes { get; set; }



        #endregion

        #endregion

        #region Overrides

        /// <summary>
        /// Returns a friendly name for this user in the form: CTI2 Atwood, Daniel Kurt Roger
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("{0} {1}, {2} {3}", this.Rate, this.LastName, this.FirstName, this.MiddleName);
        }

        #endregion

        #region Client Access Methods

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
        public static MessageToken Login_Client(MessageToken token)
        {
            try
            {
                //First, we need the username and the password.
                string username = token.GetArgOrFail("username", "You must send a username!") as string;
                string password = token.GetArgOrFail("password", "You must send a password!") as string;

                //TODO: validate username and password

                //Alright, now we're going to grab a session and then use it to load all persons where the username is the username we were sent.
                //We should only get one username back.  
                using (var session = DataAccess.SessionProvider.CreateSession())
                {
                    using (var transaction = session.BeginTransaction())
                    {
                        //The query itself.
                        var results = session.QueryOver<Person>()
                            .Where(x => x.Username == username)
                            .Cacheable().CacheMode(NHibernate.CacheMode.Normal)
                            .List<Person>();

                        if (results.Count() > 1)
                            throw new Exception(string.Format("More that one result was returned for the username, '{0}'.", username));

                        if (results.Count() == 0)
                            throw new ServiceException("Either the username or password was incorrect.", ErrorTypes.Authentication, HTTPStatusCodes.Forbiden);

                        //If the password is bad
                        if (!CommandCentral.PasswordHash.ValidatePassword(password, results[0].PasswordHash))
                        {
                            //If the password doesn't check out we need to send the person who owns this account a failed login email.
                            EmailAddress address = results[0].EmailAddresses.FirstOrDefault(x => x.IsPreferred || x.IsContactable || x.IsDODEmailAddress);

                            if (address == null)
                                throw new Exception(string.Format("Login failed to the person's account whose ID is '{0}'; however, we could find no email to send this person a warning.", results[0].ID));

                            //Ok, so we have an email we can use to contact the person!
                            EmailHelper.SendFailedAccountLoginEmail(address.Address, results[0].ID).Wait();

                            //Now that we have alerted the user, we can fail out.
                            throw new ServiceException("Either the username or password was incorrect.", ErrorTypes.Authentication, HTTPStatusCodes.Forbiden);
                        }

                        //Alright, if we got here, then we have a good password.
                        //In this case, we need to create a new session for the client and then insert it.
                        Session ses = new Session
                        {
                            IsActive = true,
                            LastUsedTime = token.CallTime,
                            LoginTime = token.CallTime,
                            Permissions = results[0].PermissionGroups,
                            Person = results[0]
                        };

                        //Now insert it
                        session.Save(ses);

                        //Now log the account history event on the person.
                        results[0].AccountHistory.Add(new AccountHistoryEvent
                        {
                            AccountHistoryEventType = AccountHistoryEventTypes.Login,
                            EventTime = token.CallTime,
                            Person = results[0]
                        });

                        //And update the person
                        session.SaveOrUpdate(results[0]);

                        //Cool now we just need to give the token back to the client along with some other stuff.
                        token.Result = new { PersonID = results[0].ID, PermissionGroups = results[0].PermissionGroups, AuthenticationToken = ses.ID, FriendlyName = results[0].ToString() };

                        return token;
                    }
                }
            }
            catch
            {
                throw;
            }
        }

        #endregion

        /// <summary>
        /// The exposed endpoints
        /// </summary>
        Dictionary<string, EndpointDescription> IExposable.EndpointDescriptions
        {
            get
            {
                throw new NotImplementedException();
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

                Id(x => x.ID).GeneratedBy.Guid();

                References(x => x.Sex).Not.Nullable();
                References(x => x.Ethnicity).Not.Nullable();
                References(x => x.ReligiousPreference).Not.Nullable();
                References(x => x.Suffix).Not.Nullable();
                References(x => x.Rank).Not.Nullable();
                References(x => x.Rate).Not.Nullable();
                References(x => x.Division).Not.Nullable();
                References(x => x.Department).Not.Nullable();
                References(x => x.Command).Not.Nullable();
                References(x => x.Billet).Not.Nullable();
                References(x => x.DutyStatus).Not.Nullable();
                References(x => x.UIC).Not.Nullable();

                Map(x => x.LastName).Not.Nullable().Length(40);
                Map(x => x.FirstName).Not.Nullable().Length(40);
                Map(x => x.MiddleName).Nullable().Length(40);
                Map(x => x.SSN).Not.Nullable().Length(40).Unique();
                Map(x => x.DateOfBirth).Not.Nullable();
                Map(x => x.Remarks).Nullable().Length(150);
                Map(x => x.Supervisor).Nullable().Length(40);
                Map(x => x.WorkCenter).Nullable().Length(40);
                Map(x => x.WorkRoom).Nullable().Length(40);
                Map(x => x.Shift).Nullable().Length(40);
                Map(x => x.WorkRemarks).Nullable().Length(150);
                Map(x => x.DateOfArrival).Not.Nullable();
                Map(x => x.JobTitle).Nullable().Length(40);
                Map(x => x.EAOS).Not.Nullable();
                Map(x => x.DateOfDeparture).Not.Nullable();
                Map(x => x.EmergencyContactInstructions).Nullable().Length(150);
                Map(x => x.ContactRemarks).Nullable().Length(150);
                Map(x => x.IsClaimed).Not.Nullable().Default(false.ToString());
                Map(x => x.Username).Nullable().Length(40).Unique();
                Map(x => x.PasswordHash).Nullable().Length(100);

                HasManyToMany(x => x.NECs);
                HasManyToMany(x => x.PermissionGroups);
                HasManyToMany(x => x.SubscribedChangeEvents);

                HasMany(x => x.AccountHistory);
                HasMany(x => x.Changes);
                HasMany(x => x.EmailAddresses);
                HasMany(x => x.PhoneNumbers);
                HasMany(x => x.PhysicalAddresses);
            }
        }

    }
}
