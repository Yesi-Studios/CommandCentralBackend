using System;
using FluentNHibernate.Mapping;
using CommandCentral.ClientAccess;
using System.Collections.Generic;
using AtwoodUtils;
using System.Linq;

namespace CommandCentral.Entities
{
    /// <summary>
    /// Describes a single profile lock.
    /// </summary>
    public class ProfileLock
    {

        private static readonly TimeSpan _maxAge = TimeSpan.FromHours(1);

        #region Properties

        /// <summary>
        /// The unique GUID assigned to this Profile Lock
        /// </summary>
        public virtual Guid Id { get; set; }

        /// <summary>
        /// The person who owns this lock.
        /// </summary>
        public virtual Person Owner { get; set; }

        /// <summary>
        /// The Person whose profile is locked.
        /// </summary>
        public virtual Person LockedPerson { get; set; }

        /// <summary>
        /// The time at which this lock was submitted.
        /// </summary>
        public virtual DateTime SubmitTime { get; set; }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Returns a timespan indicating for how much longer this profile lock is valid.
        /// </summary>
        /// <returns></returns>
        public virtual TimeSpan GetTimeRemaining()
        {
            return DateTime.Now.Subtract(SubmitTime);
        }

        #endregion

        #region Client Access

        /// <summary>
        /// WARNING!  THIS METHOD IS EXPOSED TO THE CLIENT AND IS NOT INTENDED FOR INTERNAL USE.  AUTHENTICATION, AUTHORIZATION AND VALIDATION MUST BE HANDLED PRIOR TO DB INTERACTION.
        /// <para />
        /// Given a person ID, gets a profile lock owned by that person or null if none exists.
        /// <para />
        /// Client Parameters:  <para />
        ///     personid : the person for whom to check owns a profile lock.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        [EndpointMethod(EndpointName = "GetProfileLockByOwner", AllowArgumentLogging = true, AllowResponseLogging = true, RequiresAuthentication = true)]
        private static void EndpointMethod_GetProfileLockByOwner(MessageToken token)
        {
            if (token.AuthenticationSession == null)
            {
                token.AddErrorMessage("You need to be logged in to request profile locks.", ErrorTypes.Authentication, System.Net.HttpStatusCode.Unauthorized);
                return;
            }

            if (!token.Args.ContainsKey("personid"))
            {
                token.AddErrorMessage("You didn't send a 'personid' parameter.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                return;
            }

            Guid personId;
            if (!Guid.TryParse(token.Args["personid"] as string, out personId))
            {
                token.AddErrorMessage("The 'personid' parameter", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                return;
            }

            using (var session = DataAccess.NHibernateHelper.CreateStatefulSession())
            {
                //Get all profile locks whose owners are the given person's Id.  I use single or defualt so that it'll cause a crash if more than one exists. 
                //For more than one to exist, we must have violated both our rules in logic and the database's foreign key rules.  So that's not good.
                token.SetResult(session.QueryOver<ProfileLock>().Where(x => x.Owner.Id == personId).SingleOrDefault());
            }
        }

        /// <summary>
        /// WARNING!  THIS METHOD IS EXPOSED TO THE CLIENT AND IS NOT INTENDED FOR INTERNAL USE.  AUTHENTICATION, AUTHORIZATION AND VALIDATION MUST BE HANDLED PRIOR TO DB INTERACTION.
        /// <para />
        /// Given a person ID, gets a profile lock on the person's profile or null if none exists.
        /// <para />
        /// Client Parameters: <para />
        ///     personid : the person for whom to check for a profile lock.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        [EndpointMethod(EndpointName = "GetProfileLockByLockedPerson", AllowArgumentLogging = true, AllowResponseLogging = true, RequiresAuthentication = true)]
        private static void EndpointMethod_GetProfileLockByLockedPerson(MessageToken token)
        {
            if (token.AuthenticationSession == null)
            {
                token.AddErrorMessage("You need to be logged in to request profile locks.", ErrorTypes.Authentication, System.Net.HttpStatusCode.Unauthorized);
                return;
            }

            if (!token.Args.ContainsKey("personid"))
            {
                token.AddErrorMessage("You didn't send a 'personid' parameter.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                return;
            }

            Guid personId;
            if (!Guid.TryParse(token.Args["personid"] as string, out personId))
            {
                token.AddErrorMessage("The 'personid' parameter", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                return;
            }

            using (var session = DataAccess.NHibernateHelper.CreateStatefulSession())
            {
                //Get all profile locks where the locked person is the given Id.
                token.SetResult(session.QueryOver<ProfileLock>().Where(x => x.LockedPerson.Id == personId).SingleOrDefault());
            }
        }

        /// <summary>
        /// WARNING!  THIS METHOD IS EXPOSED TO THE CLIENT AND IS NOT INTENDED FOR INTERNAL USE.  AUTHENTICATION, AUTHORIZATION AND VALIDATION MUST BE HANDLED PRIOR TO DB INTERACTION.
        /// <para />
        /// Given a person ID, attempts to take a lock on the person's profile - preventing edits to the profile by anyone else.  If a lock already exists on the account and it is owned by the client, the lock is renewed.  If the lock has aged off, the lock is released and the client is given the lock.  If the lock exists, is not owned by the client, and has not aged off, a LockOwned error is returned.
        /// <para />
        /// Client Parameters: <para />
        ///     personid : the person for whom to attempt to take a lock.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        [EndpointMethod(EndpointName = "TakeProfileLock", AllowArgumentLogging = true, AllowResponseLogging = true, RequiresAuthentication = true)]
        private static void EndpointMethod_TakeProfileLock(MessageToken token)
        {

            //Do our work in a new session
            using (var session = DataAccess.NHibernateHelper.CreateStatefulSession())
            using (var transaction = session.BeginTransaction())
            {
                try
                {
                    if (token.AuthenticationSession == null)
                    {
                        token.AddErrorMessage("You need to be logged in to request profile locks.", ErrorTypes.Authentication, System.Net.HttpStatusCode.Unauthorized);
                        return;
                    }

                    if (!token.Args.ContainsKey("personid"))
                    {
                        token.AddErrorMessage("You didn't send a 'personid' parameter.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                        return;
                    }

                    Guid personId;
                    if (!Guid.TryParse(token.Args["personid"] as string, out personId))
                    {
                        token.AddErrorMessage("The 'personid' parameter", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                        return;
                    }

                    var person = session.Get<Person>(personId);

                    if (person == null)
                    {
                        token.AddErrorMessage("That person id does not correlate to a real person.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                        return;
                    }

                    //Ok so the person is real.  In that case, let's release all locks owned by the client.
                    session.QueryOver<ProfileLock>().Where(x => x.Owner == token.AuthenticationSession.Person)
                        .List()
                        .ToList()
                        .ForEach(x => session.Delete(x));

                    //Now the client has no locks.  Let's also make sure the profile we're trying to lock isn't owned by someone else.
                    var profileLock = session.QueryOver<ProfileLock>().Where(x => x.LockedPerson == person).SingleOrDefault();


                    //If the profile lock is not null, then a lock is owned on this profile already.
                    if (profileLock != null)
                    {
                        //If the lock is owned by the logged in user that's bad because we supposedly jsut deleted all their locks.  Let's throw an exception.
                        if (profileLock.Owner == token.AuthenticationSession.Person)
                        {
                            throw new Exception("How did you get here cotton eye Joe?");
                        }

                        //If the lock is not owned by the client, let's see if it's aged off.
                        if (DateTime.Now.Subtract(profileLock.SubmitTime) < _maxAge)
                        {
                            //Since the profile lock has aged off we can go ahead and delete it and then let this method continue on.
                            session.Delete(profileLock);
                        }
                        else
                        {
                            //If we're here then there is a lock, it is owned by someone else, and the lock has not aged off.
                            token.AddErrorMessage("A lock on this profile is owned by '{0}'; therefore, you will not be able to edit this profile.".FormatS(profileLock.Owner.ToString()), ErrorTypes.LockOwned, System.Net.HttpStatusCode.Forbidden);
                            return;
                        }
                    }

                    //Ok so there's no lock on this profile.  Let's see if the client can be given a lock.  This is determined by whether or not the client can edit a person.
                    if (!token.AuthenticationSession.Person.HasSpecialPermissions(Authorization.SpecialPermissions.EditPerson))
                    {
                        token.AddErrorMessage("You are not authorized to edit a person and can therefore not take a lock on this profile.", ErrorTypes.LockImpossible, System.Net.HttpStatusCode.Forbidden);
                        return;
                    }

                    //Now we just need to make a lock for this client on this person.
                    session.Save(new ProfileLock
                    {
                        LockedPerson = person,
                        Owner = token.AuthenticationSession.Person,
                        SubmitTime = token.CallTime
                    });

                    token.SetResult("Profile Lock Obtained");

                    transaction.Commit();
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        #endregion Client Access

        /// <summary>
        /// Maps a profile lock to the database.
        /// </summary>
        public class ProfileLockMapping : ClassMap<ProfileLock>
        {
            /// <summary>
            /// Maps a profile lock to the database.
            /// </summary>
            public ProfileLockMapping()
            {
                Id(x => x.Id).GeneratedBy.Guid();

                References(x => x.Owner).Not.Nullable();
                References(x => x.LockedPerson).Not.Nullable();

                Map(x => x.SubmitTime).Not.Nullable();

                Cache.ReadWrite();
            }
        }
    }
}
