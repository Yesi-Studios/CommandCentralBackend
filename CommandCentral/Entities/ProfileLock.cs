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

            token.SetResult(token.CommunicationSession.QueryOver<ProfileLock>().Where(x => x.Owner.Id == personId).SingleOrDefault());
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

            token.SetResult(token.CommunicationSession.QueryOver<ProfileLock>().Where(x => x.LockedPerson.Id == personId).SingleOrDefault());
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

            //Try to obtain a reference to the persistent person object.  If this returns null, then it's not a real person's id.
            var person = token.CommunicationSession.Load<Person>(personId);

            if (person == null)
            {
                token.AddErrorMessage("That person id does not correlate to a real person.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                return;
            }

            var profileLock = token.CommunicationSession.QueryOver<ProfileLock>().Where(x => x.LockedPerson == person).SingleOrDefault();

            //If the profile lock is not null, then a lock is owned on this profile already.
            if (profileLock != null)
            {
                //If the lock is owned by the logged in user, then they are trying to renew it.
                if (profileLock.Owner == token.AuthenticationSession.Person)
                {
                    profileLock.SubmitTime = DateTime.Now;
                    token.CommunicationSession.Update(profileLock);
                    token.SetResult("Profile lock renewed.");
                    return;
                }
                
                //If the lock is not owned by the client, let's see if it's aged off.
                if (DateTime.Now.Subtract(profileLock.SubmitTime) < _maxAge)
                {
                    //Since the profile lock has aged off we can go ahead and delete it and then let this method continue on.
                    token.CommunicationSession.Delete(profileLock);
                }
                else
                {
                    //If we're here then there is a lock, it is owned by someone else, and the lock has not aged off.
                    token.AddErrorMessage("A lock on this profile is owned by '{0}'; therefore, you will not be able to edit this profile.".FormatS(profileLock.Owner.ToString()), ErrorTypes.LockOwned, System.Net.HttpStatusCode.Forbidden);
                    return;
                }
            }

            //Ok so there's no lock.  Let's see if the client can be given a lock.  This is determined by whether or not the client can edit a person.
            if (!token.AuthenticationSession.Person.HasSpecialPermissions(Authorization.SpecialPermissions.EditPerson))
            {
                token.AddErrorMessage("You are not authorized to edit a person and can therefore not take a lock on this profile.", ErrorTypes.LockImpossible, System.Net.HttpStatusCode.Forbidden);
                return;
            }

            //Now we just need to make a lock for this client on this person.
            token.CommunicationSession.Save(new ProfileLock
                {
                    LockedPerson = person,
                    Owner = token.AuthenticationSession.Person,
                    SubmitTime = token.CallTime
                });

            token.SetResult("Profile Lock Obtained");
        }

        /// <summary>
        /// WARNING!  THIS METHOD IS EXPOSED TO THE CLIENT AND IS NOT INTENDED FOR INTERNAL USE.  AUTHENTICATION, AUTHORIZATION AND VALIDATION MUST BE HANDLED PRIOR TO DB INTERACTION.
        /// <para />
        /// Given a person ID, attempts to release a lock on the given profile.  Locks may be released by their owners or by anyone after the max age.
        /// <para />
        /// Client Parameters: <para />
        ///     personid : the person for whom to attempt to release a lock.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        [EndpointMethod(EndpointName = "ReleaseProfileLock", AllowArgumentLogging = true, AllowResponseLogging = true, RequiresAuthentication = true)]
        private static void EndpointMethod_ReleaseProfileLock(MessageToken token)
        {
            //Stole most of this shit from the TakeProfileLock endpoint lololol.
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

            //Try to obtain a reference to the persistent person object.  If this returns null, then it's not a real person's id.
            var person = token.CommunicationSession.Get<Person>(personId);

            if (person == null)
            {
                token.AddErrorMessage("That person id does not correlate to a real person.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                return;
            }

            var profileLock = token.CommunicationSession.QueryOver<ProfileLock>().Where(x => x.LockedPerson == person).SingleOrDefault();

            //Now we need to find out who is trying to release the lock.  If the owner is trying to release the lock, this is allowed.
            //Additionally, if the lock is older than the max age, then anyone can release it.
            if (profileLock == null)
            {
                token.AddErrorMessage("No lock exists on this profile; therefore, no lock could be released.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                return;
            }

            //If the owner is the logged in user...
            if (profileLock.Owner == token.AuthenticationSession.Person)
            {
                //Then delete the lock.
                token.CommunicationSession.Delete(profileLock);
                token.SetResult("Success");
                return;
            }

            //If the lock has timed out, then just delete it.
            if (DateTime.Now.Subtract(profileLock.SubmitTime) < _maxAge)
            {
                //Then delete the lock.
                token.CommunicationSession.Delete(profileLock);
                token.SetResult("Success");
                return;
            }

            //If we get to here then we can't release the lock.
            token.AddErrorMessage("A lock on this profile is currently owned by '{0}', who has this lock for another '{1}' minutes.".FormatS(profileLock.Owner.ToString(), DateTime.Now.Subtract(profileLock.SubmitTime).TotalMinutes), ErrorTypes.Authorization, System.Net.HttpStatusCode.Forbidden);
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
