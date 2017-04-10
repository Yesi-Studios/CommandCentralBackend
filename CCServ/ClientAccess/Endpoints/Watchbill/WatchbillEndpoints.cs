using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AtwoodUtils;
using CCServ.Authorization;
using CCServ.Entities.Watchbill;

namespace CCServ.ClientAccess.Endpoints.Watchbill
{
    /// <summary>
    /// Contains all of the endpoints for interacting with the parent watchbill object.
    /// </summary>
    static class WatchbillEndpoints
    {
        /// <summary>
        /// WARNING!  THIS METHOD IS EXPOSED TO THE CLIENT AND IS NOT INTENDED FOR INTERNAL USE.  AUTHENTICATION, AUTHORIZATION AND VALIDATION MUST BE HANDLED PRIOR TO DB INTERACTION.
        /// <para />
        /// Loads a watchbill.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        [EndpointMethod(AllowArgumentLogging = true, AllowResponseLogging = true, RequiresAuthentication = true)]
        private static void LoadWatchbill(MessageToken token)
        {
            token.AssertLoggedIn();
            token.Args.AssertContainsKeys("watchbillid");

            if (!Guid.TryParse(token.Args["watchbillid"] as string, out Guid watchbillId))
                throw new CommandCentralException("Your watchbill id parameter's format was invalid.", HttpStatusCodes.BadRequest);

            bool doPopulation = false;
            if (token.Args.ContainsKey("dopopulation"))
            {
                if (!Boolean.TryParse(token.Args["dopopulation"] as string, out doPopulation))
                {
                    throw new CommandCentralException("Your 'dopopulation' parameter was in an invalid format.", HttpStatusCodes.BadRequest);
                }
            }

            //Now let's go get the watchbill from the database.
            using (var session = DataAccess.NHibernateHelper.CreateStatefulSession())
            {
                using (var transaction = session.BeginTransaction())
                {
                    try
                    {
                        var watchbillFromDB = session.Get<Entities.Watchbill.Watchbill>(watchbillId) ??
                            throw new CommandCentralException("Your watchbill's id was not valid.  Please consider creating the watchbill first.", HttpStatusCodes.BadRequest);

                        if (doPopulation)
                        {
                            //Make sure the client is allowed to.  It's not actually a security issue if the client does the population,
                            //but we may as well restrict it because the population method is very expensive.
                            if (!token.AuthenticationSession.Person.PermissionGroups.Any(x => x.ChainsOfCommandMemberOf.Contains(watchbillFromDB.EligibilityGroup.OwningChainOfCommand) && x.AccessLevel == ChainOfCommandLevels.Command))
                                throw new CommandCentralException("You are not allowed to edit this watchbill.  You must have command level permissions in the related chain of command.", HttpStatusCodes.Unauthorized);

                            //And make sure we're at a state where population can occur.
                            if (watchbillFromDB.CurrentState != Entities.ReferenceLists.Watchbill.WatchbillStatuses.ClosedForInputs)
                                throw new CommandCentralException("You may not populate this watchbill - a watchbill must be in the Closed for Inputs state in order to populate it.", HttpStatusCodes.Forbidden);

                            watchbillFromDB.PopulateWatchbill(token.AuthenticationSession.Person, token.CallTime);
                        }

                        token.SetResult(new 
                        {
                            watchbillFromDB.CreatedBy,
                            watchbillFromDB.CurrentState,
                            watchbillFromDB.EligibilityGroup,
                            watchbillFromDB.Id,
                            watchbillFromDB.InputRequirements,
                            watchbillFromDB.LastStateChange,
                            watchbillFromDB.LastStateChangedBy,
                            watchbillFromDB.Title,
                            WatchDays = watchbillFromDB.WatchDays.OrderBy(x => x.Date).ToList()
                        });

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
        /// Loads all watchbills.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        [EndpointMethod(AllowArgumentLogging = true, AllowResponseLogging = true, RequiresAuthentication = true)]
        private static void LoadWatchbills(MessageToken token)
        {
            token.AssertLoggedIn();

            //Now let's go get the watchbill from the database.
            using (var session = DataAccess.NHibernateHelper.CreateStatefulSession())
            {
                using (var transaction = session.BeginTransaction())
                {
                    try
                    {
                        token.SetResult(session.QueryOver<Entities.Watchbill.Watchbill>().List());

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
        /// Creates a watchbill.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        [EndpointMethod(AllowArgumentLogging = true, AllowResponseLogging = true, RequiresAuthentication = true)]
        private static void CreateWatchbill(MessageToken token)
        {
            token.AssertLoggedIn();
            token.Args.AssertContainsKeys("watchbill");

            Entities.Watchbill.Watchbill watchbillFromClient;
            try
            {
                watchbillFromClient = token.Args["watchbill"].CastJToken<Entities.Watchbill.Watchbill>();
            }
            catch
            {
                throw new CommandCentralException("An error occurred while trying to parse your watchbill.", HttpStatusCodes.BadRequest);
            }

            NHibernate.NHibernateUtil.Initialize(token.AuthenticationSession.Person.Command);
            Entities.Watchbill.Watchbill watchbillToInsert = new Entities.Watchbill.Watchbill
            {
                Command = token.AuthenticationSession.Person.Command,
                CreatedBy = token.AuthenticationSession.Person,
                CurrentState = Entities.ReferenceLists.Watchbill.WatchbillStatuses.Initial,
                Id = Guid.NewGuid(),
                LastStateChange = token.CallTime,
                LastStateChangedBy = token.AuthenticationSession.Person,
                Title = watchbillFromClient.Title,
                EligibilityGroup = watchbillFromClient.EligibilityGroup
            };

            var validationResult = new Entities.Watchbill.Watchbill.WatchbillValidator().Validate(watchbillToInsert);

            if (!validationResult.IsValid)
                throw new AggregateException(validationResult.Errors.Select(x => new CommandCentralException(x.ErrorMessage, HttpStatusCodes.BadRequest)));

            //Let's make sure the client is allowed to make a watchbill with this eligibility group.
            var ellGroup = Entities.ReferenceLists.Watchbill.WatchEligibilityGroups.AllWatchEligibilityGroups.FirstOrDefault(x => Guid.Equals(x.Id, watchbillToInsert.EligibilityGroup.Id)) ??
                throw new CommandCentralException("You failed to provide a proper eligibilty group.", HttpStatusCodes.BadRequest);

            if (!token.AuthenticationSession.Person.PermissionGroups.Any(x => x.ChainsOfCommandMemberOf.Contains(ellGroup.OwningChainOfCommand) && x.AccessLevel == ChainOfCommandLevels.Command))
                throw new CommandCentralException("You are not allowed to create a watchbill tied to that eligibility group.  " +
                    "You must have command level permissions in the related chain of command.", HttpStatusCodes.Unauthorized);

            using (var session = DataAccess.NHibernateHelper.CreateStatefulSession())
            {
                using (var transaction = session.BeginTransaction())
                {
                    try
                    {
                        session.Save(watchbillToInsert);

                        token.SetResult(watchbillToInsert);

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
        /// Updates a watchbill.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        [EndpointMethod(AllowArgumentLogging = true, AllowResponseLogging = true, RequiresAuthentication = true)]
        private static void UpdateWatchbill(MessageToken token)
        {
            token.AssertLoggedIn();
            token.Args.AssertContainsKeys("watchbill");

            Entities.Watchbill.Watchbill watchbillFromClient;
            try
            {
                watchbillFromClient = token.Args["watchbill"].CastJToken<Entities.Watchbill.Watchbill>();
            }
            catch
            {
                throw new CommandCentralException("An error occurred while trying to parse your watchbill.", HttpStatusCodes.BadRequest);
            }

            var validationResult = new Entities.Watchbill.Watchbill.WatchbillValidator().Validate(watchbillFromClient);

            if (!validationResult.IsValid)
                throw new AggregateException(validationResult.Errors.Select(x => new CommandCentralException(x.ErrorMessage, HttpStatusCodes.BadRequest)));

            //Now let's go get the watchbill from the database.
            using (var session = DataAccess.NHibernateHelper.CreateStatefulSession())
            {
                using (var transaction = session.BeginTransaction())
                {
                    try
                    {
                        var watchbillFromDB = session.Get<Entities.Watchbill.Watchbill>(watchbillFromClient.Id) ??
                            throw new CommandCentralException("Your watchbill's id was not valid.  Please consider creating the watchbill first.", HttpStatusCodes.BadRequest);

                        //Ok so there's a watchbill.  Let's get the ell group to determine the permissions.
                        if (!token.AuthenticationSession.Person.PermissionGroups.Any(x => x.ChainsOfCommandMemberOf.Contains(watchbillFromDB.EligibilityGroup.OwningChainOfCommand) && x.AccessLevel == ChainOfCommandLevels.Command))
                            throw new CommandCentralException("You are not allowed to edit this watchbill.  " +
                                "You must have command level permissions in the related chain of command.", HttpStatusCodes.Unauthorized);

                        //Now let's move the properties over that are editable.
                        watchbillFromDB.Title = watchbillFromClient.Title;

                        //If the state is different, we need to move the state as well.  There's a method for that.
                        if (watchbillFromDB.CurrentState != watchbillFromClient.CurrentState)
                        {
                            //It looks like the client is trying to change the state.
                            watchbillFromDB.SetState(watchbillFromClient.CurrentState, token.CallTime, token.AuthenticationSession.Person);
                        }

                        session.Update(watchbillFromDB);

                        token.SetResult(watchbillFromDB);

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
        /// Deletes a watchbill.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        [EndpointMethod(AllowArgumentLogging = true, AllowResponseLogging = true, RequiresAuthentication = true)]
        private static void DeleteWatchbill(MessageToken token)
        {
            token.AssertLoggedIn();
            token.Args.AssertContainsKeys("watchbill");

            Entities.Watchbill.Watchbill watchbillFromClient;
            try
            {
                watchbillFromClient = token.Args["watchbill"].CastJToken<Entities.Watchbill.Watchbill>();
            }
            catch
            {
                throw new CommandCentralException("An error occurred while trying to parse your watchbill.", HttpStatusCodes.BadRequest);
            }

            //Now let's go get the watchbill from the database.
            using (var session = DataAccess.NHibernateHelper.CreateStatefulSession())
            {
                using (var transaction = session.BeginTransaction())
                {
                    try
                    {
                        var watchbillFromDB = session.Get<Entities.Watchbill.Watchbill>(watchbillFromClient.Id) ??
                            throw new CommandCentralException("Your watchbill's id was not valid.  Please consider creating the watchbill first.", HttpStatusCodes.BadRequest);

                        //Ok so there's a watchbill.  Let's get the ell group to determine the permissions.
                        if (!token.AuthenticationSession.Person.PermissionGroups.Any(x => x.ChainsOfCommandMemberOf.Contains(watchbillFromDB.EligibilityGroup.OwningChainOfCommand) && x.AccessLevel == ChainOfCommandLevels.Command))
                            throw new CommandCentralException("You are not allowed to edit this watchbill.  " +
                                "You must have command level permissions in the related chain of command.", HttpStatusCodes.Unauthorized);

                        //Check the state.
                        if (watchbillFromDB.CurrentState != Entities.ReferenceLists.Watchbill.WatchbillStatuses.Initial)
                            throw new CommandCentralException("You may not delete a watchbill that is not in the initial state.  Please consider changing its state first.", HttpStatusCodes.BadRequest);

                        session.Delete(watchbillFromDB);

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
}
