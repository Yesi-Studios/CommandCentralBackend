using System;
using FluentNHibernate.Mapping;
using CCServ.ClientAccess;
using System.Collections.Generic;
using AtwoodUtils;
using System.Linq;
using CCServ.Entities;
using CCServ.Authorization;

namespace CCServ.ClientAccess.Endpoints
{
    /// <summary>
    /// Contains all those endpoints for profile locks.
    /// </summary>
    static class ProfileLockEndpoints
    {
        /// <summary>
        /// WARNING!  THIS METHOD IS EXPOSED TO THE CLIENT AND IS NOT INTENDED FOR INTERNAL USE.  AUTHENTICATION, AUTHORIZATION AND VALIDATION MUST BE HANDLED PRIOR TO DB INTERACTION.
        /// <para />
        /// Given a person ID, attempts to take a lock on the person's profile - preventing edits to the profile by anyone else.  
        /// If a lock already exists on the account and it is owned by the client, the lock is renewed.  
        /// If the lock has aged off, the lock is released and the client is given the lock.  
        /// If the lock exists, is not owned by the client, and has not aged off, a LockOwned error is returned.
        /// </summary>
        /// <param name="token"></param>
        /// <param name="dto"></param>
        /// <returns></returns>
        [EndpointMethod(AllowArgumentLogging = true, AllowResponseLogging = true, RequiresAuthentication = true)]
        private static void TakeProfileLock(MessageToken token, DTOs.ProfileLockEndpoints.TakeProfileLock dto)
        {
            token.AssertLoggedIn();

            //Do our work in a new session
            using (var session = DataAccess.NHibernateHelper.CreateStatefulSession())
            using (var transaction = session.BeginTransaction())
            {
                try
                {
                    //Before we do anything, we need to release any lock the client might own.
                    //Flush the session cause we're about to do stuff in that table as well.
                    var clientLocks = session.QueryOver<ProfileLock>().Where(x => x.Owner == token.AuthenticationSession.Person).List();
                    foreach (var clientLock in clientLocks)
                    {
                        session.Delete(clientLock);
                    }
                    session.Flush();

                    var person = session.Get<Person>(dto.PersonId) ??
                        throw new CommandCentralException("That person id does not correlate to a real person.", ErrorTypes.Validation);

                    //Now the client has no locks.  Let's also make sure the profile we're trying to lock isn't owned by someone else.
                    var profileLock = session.QueryOver<ProfileLock>().Where(x => x.LockedPerson == person).SingleOrDefault();

                    //If the profile lock is not null, then a lock is owned on this profile already.
                    if (profileLock != null)
                    {
                        //If the client owns the lock, then they're trying to renew the lock.  Let's allow that.
                        //This shouldn't even happen since we release all locks owned by the client but whatever.
                        if (profileLock.Owner.Id == token.AuthenticationSession.Person.Id)
                        {
                            profileLock.SubmitTime = token.CallTime;
                            session.Update(profileLock);
                        }
                        else
                        {
                            //Someone else, not the client, owns the lock.
                            //Let's see if it's aged off.
                            if (profileLock.IsValid())
                            {
                                //If we're here then there is a lock, it is owned by someone else, and the lock has not aged off.
                                throw new CommandCentralException("A lock on this profile is owned by '{0}'; therefore, you will not be able to edit this profile.".FormatS(profileLock.Owner.ToString()), ErrorTypes.LockOwned);
                            }
                            else
                            {
                                //Since the profile lock has aged off we can go ahead and give it to the client.
                                profileLock.Owner = token.AuthenticationSession.Person;
                                profileLock.SubmitTime = token.CallTime;
                                session.Update(profileLock);
                            }
                        }

                        //In all cases, we want to tell the client about the profile lock.
                        token.SetResult(new
                        {
                            profileLock.Id,
                            profileLock.SubmitTime,
                            Owner = profileLock.Owner,
                            LockedPerson = profileLock.LockedPerson,
                            ExpirationTime = profileLock?.SubmitTime.Add(ProfileLock.MaxAge)
                        });
                    }
                    else
                    {
                        //If we're here, then there's no profile lock and we need to make one.
                        var newLock = new ProfileLock
                        {
                            Id = Guid.NewGuid(),
                            LockedPerson = person,
                            Owner = token.AuthenticationSession.Person,
                            SubmitTime = token.CallTime
                        };

                        //Save the lock.
                        session.Save(newLock);

                        //And then give it to the client.
                        token.SetResult(new
                        {
                            newLock.Id,
                            newLock.SubmitTime,
                            Owner = newLock.Owner,
                            LockedPerson = newLock.LockedPerson,
                            ExpirationTime = profileLock?.SubmitTime.Add(ProfileLock.MaxAge)
                        });
                    }

                    transaction.Commit();
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        /// <summary>
        /// WARNING!  THIS METHOD IS EXPOSED TO THE CLIENT AND IS NOT INTENDED FOR INTERNAL USE.  AUTHENTICATION, AUTHORIZATION AND VALIDATION MUST BE HANDLED PRIOR TO DB INTERACTION.
        /// <para />
        /// Releases all profile locks owned by the client.
        /// </summary>
        /// <param name="token"></param>
        /// <param name="dto"></param>
        /// <returns></returns>
        [EndpointMethod(AllowArgumentLogging = true, AllowResponseLogging = true, RequiresAuthentication = true)]
        private static void ReleaseProfileLock(MessageToken token)
        {
            token.AssertLoggedIn();

            //Do our work in a new session
            using (var session = DataAccess.NHibernateHelper.CreateStatefulSession())
            using (var transaction = session.BeginTransaction())
            {
                try
                {
                    var profileLocks = session.QueryOver<ProfileLock>().Where(x => x.Owner == token.AuthenticationSession.Person).List();

                    if (profileLocks.Count > 1)
                        throw new Exception("{0} owned more than one profile lock.".FormatS(token.AuthenticationSession.Person.ToString()));
                    
                    foreach (var profileLock in profileLocks)
                    {
                        session.Delete(profileLock);
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
}
