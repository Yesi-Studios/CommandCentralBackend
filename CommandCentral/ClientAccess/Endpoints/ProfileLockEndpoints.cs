using System;
using FluentNHibernate.Mapping;
using CommandCentral.ClientAccess;
using System.Collections.Generic;
using AtwoodUtils;
using System.Linq;
using CommandCentral.Entities;
using CommandCentral.Authorization;

namespace CommandCentral.ClientAccess.Endpoints
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
        /// <para />
        /// Client Parameters: <para />
        ///     personid : the person for whom to attempt to take a lock.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        [EndpointMethod(AllowArgumentLogging = true, AllowResponseLogging = true, RequiresAuthentication = true)]
        private static void TakeProfileLock(MessageToken token)
        {
            token.AssertLoggedIn();
            token.Args.AssertContainsKeys("personid");

            if (!Guid.TryParse(token.Args["personid"] as string, out Guid personId))
                throw new CommandCentralException("The 'personid' parameter was not valid", ErrorTypes.Validation);

            //Do our work in a new session
            using (var session = DataAccess.DataProvider.CreateStatefulSession())
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

                    var person = session.Get<Person>(personId) ??
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
                                throw new CommandCentralException("A lock on this profile is owned by '{0}'; therefore, you will not be able to edit this profile.".With(profileLock.Owner.ToString()), ErrorTypes.LockOwned);
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
                            ExpirationTime = newLock?.SubmitTime.Add(ProfileLock.MaxAge)
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
        /// Given a person ID, attempts to take a lock on the person's profile - preventing edits to the profile by anyone else.  If a lock already exists on the account and it is owned by the client, the lock is renewed.  If the lock has aged off, the lock is released and the client is given the lock.  If the lock exists, is not owned by the client, and has not aged off, a LockOwned error is returned.
        /// <para />
        /// Client Parameters: <para />
        ///     personid : the person for whom to attempt to take a lock.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        [EndpointMethod(AllowArgumentLogging = true, AllowResponseLogging = true, RequiresAuthentication = true)]
        private static void ReleaseProfileLock(MessageToken token)
        {
            token.AssertLoggedIn();
            token.Args.AssertContainsKeys("profilelockid");

            if (!Guid.TryParse(token.Args["profilelockid"] as string, out Guid profileLockId))
                throw new CommandCentralException("The 'profilelockid' parameter was not valid", ErrorTypes.Validation);

            bool forceRelease = false;
            if (token.Args.ContainsKey("forcerelease"))
            {
                forceRelease = (bool)token.Args["forcerelease"];
            }

            //If the client wants to force release the lock, they must have access to the admin tools.
            if (forceRelease)
            {
                if (!token.AuthenticationSession.Person.PermissionGroups.Any(x => x.AccessibleSubModules.Contains(SubModules.AdminTools.ToString(), StringComparer.CurrentCultureIgnoreCase)))
                {
                    throw new CommandCentralException("In order to force a lock to release, you must have access to the Admin Tools.", ErrorTypes.Authorization);
                }
            }

            //Do our work in a new session
            using (var session = DataAccess.DataProvider.CreateStatefulSession())
            using (var transaction = session.BeginTransaction())
            {
                try
                {
                    var profileLock = session.Get<ProfileLock>(profileLockId) ??
                        throw new CommandCentralException("That profile lock id was not valid.", ErrorTypes.Validation);

                    //Ok if the client doesn't own the profile lock, then we need to see if we can force it to release.
                    if (forceRelease)
                    {
                        //This is the easist option.  Regardless of the profile lock state, this is a person with access to admin tools.
                        //So we're just going to drop the profile lock.
                        session.Delete(profileLock);
                    }
                    else if (profileLock.Owner.Id == token.AuthenticationSession.Person.Id)
                    {
                        //Ok, second options.  If the client owns the profile lock, they can release it.
                        //I know I could've done these in the same if statement - I wanted to clearly see the different options.
                        session.Delete(profileLock);
                    }
                    else if (!profileLock.IsValid())
                    {
                        //Ok, next option, if the profile lock is no longer valid, let's throw it out.
                        session.Delete(profileLock);
                    }
                    else
                    {
                        //Welp, if we got there then the client isn't allowed to release this lock.
                        throw new CommandCentralException("You do not have permission to release the profile lock and it is still valid.", ErrorTypes.Validation);
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
