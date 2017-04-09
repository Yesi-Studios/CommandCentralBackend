using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AtwoodUtils;
using CCServ.Entities.Watchbill;

namespace CCServ.ClientAccess.Endpoints.Watchbill
{
    /// <summary>
    /// Contains all of the endpoints for interacting with the watch day object.
    /// </summary>
    class WatchDayEndpoints
    {
        /// <summary>
        /// WARNING!  THIS METHOD IS EXPOSED TO THE CLIENT AND IS NOT INTENDED FOR INTERNAL USE.  AUTHENTICATION, AUTHORIZATION AND VALIDATION MUST BE HANDLED PRIOR TO DB INTERACTION.
        /// <para />
        /// Loads a watch day.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        [EndpointMethod(AllowArgumentLogging = true, AllowResponseLogging = true, RequiresAuthentication = true)]
        private static void LoadWatchDay(MessageToken token)
        {
            token.AssertLoggedIn();
            token.Args.AssertContainsKeys("watchdayid");

            if (!Guid.TryParse(token.Args["watchdayid"] as string, out Guid watchDayId))
                throw new CommandCentralException("Your watch day id parameter's format was invalid.", HttpStatusCodes.BadRequest);

            //Now let's go get the watch day from the database.
            using (var session = DataAccess.NHibernateHelper.CreateStatefulSession())
            {
                using (var transaction = session.BeginTransaction())
                {
                    try
                    {
                        var watchDayFromDB = session.Get<WatchDay>(watchDayId) ??
                            throw new CommandCentralException("Your watch day's id was not valid.  Please consider creating the watch day first.", HttpStatusCodes.BadRequest);

                        token.SetResult(watchDayFromDB);

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
        /// Creates multiple watch days.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        [EndpointMethod(AllowArgumentLogging = true, AllowResponseLogging = true, RequiresAuthentication = true)]
        private static void CreateWatchDays(MessageToken token)
        {
            token.AssertLoggedIn();
            token.Args.AssertContainsKeys("watchdays");

            List<WatchDay> watchDaysFromClient;
            try
            {
                watchDaysFromClient = token.Args["watchdays"].CastJToken<List<WatchDay>>();
            }
            catch
            {
                throw new CommandCentralException("An error occurred while trying to parse your watch days.", HttpStatusCodes.BadRequest);
            }

            var watchDaysToInsert = watchDaysFromClient.Select(x => new WatchDay
            {
                Date = x.Date,
                Id = Guid.NewGuid(),
                Remarks = x.Remarks,
                Watchbill = x.Watchbill
            }).ToList();

            var validationResults = watchDaysToInsert.Select(x => new WatchDay.WatchDayValidator().Validate(x)).ToList();
            var invalidResults = validationResults.Where(x => !x.IsValid);
            if (invalidResults.Any())
                throw new AggregateException(invalidResults.SelectMany(x => x.Errors.Select(y => new CommandCentralException(y.ErrorMessage, HttpStatusCodes.BadRequest))));

            using (var session = DataAccess.NHibernateHelper.CreateStatefulSession())
            {
                using (var transaction = session.BeginTransaction())
                {
                    try
                    {
                        foreach (var day in watchDaysToInsert)
                        {
                            //Let's get the watchbill the client says this watch day will be assigned to.
                            var watchbill = session.Get<Entities.Watchbill.Watchbill>(day.Watchbill.Id) ??
                                throw new CommandCentralException("Your watchbill's id was not valid.  Please consider creating the watchbill first.", HttpStatusCodes.BadRequest);

                            if (!token.AuthenticationSession.Person.PermissionGroups.Any(x => x.ChainsOfCommandMemberOf.Contains(watchbill.EligibilityGroup.OwningChainOfCommand) && x.AccessLevel == ChainOfCommandLevels.Command))
                                throw new CommandCentralException("You are not allowed to edit the structure of a watchbill tied to that eligibility group.  " +
                                    "You must have command level permissions in the related chain of command.", HttpStatusCodes.Unauthorized);

                            //Check the state.
                            if (watchbill.CurrentState != Entities.ReferenceLists.Watchbill.WatchbillStatuses.Initial)
                                throw new CommandCentralException("You may not edit the structure of a watchbill that is not in the initial state.  " +
                                    "Please consider changing its state first.", HttpStatusCodes.Forbidden);

                            watchbill.WatchDays.Add(day);

                            session.Update(watchbill);
                        }

                        token.SetResult(watchDaysToInsert);

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
        /// Updates a watch day.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        [EndpointMethod(AllowArgumentLogging = true, AllowResponseLogging = true, RequiresAuthentication = true)]
        private static void UpdateWatchDay(MessageToken token)
        {
            token.AssertLoggedIn();
            token.Args.AssertContainsKeys("watchday");

            WatchDay watchDayFromClient;
            try
            {
                watchDayFromClient = token.Args["watchday"].CastJToken<WatchDay>();
            }
            catch
            {
                throw new CommandCentralException("An error occurred while trying to parse your watch day.", HttpStatusCodes.BadRequest);
            }

            var valdiationResult = new WatchDay.WatchDayValidator().Validate(watchDayFromClient);

            if (!valdiationResult.IsValid)
                throw new AggregateException(valdiationResult.Errors.Select(x => new CommandCentralException(x.ErrorMessage, HttpStatusCodes.BadRequest)));

            using (var session = DataAccess.NHibernateHelper.CreateStatefulSession())
            {
                using (var transaction = session.BeginTransaction())
                {
                    try
                    {
                        var watchDayFromDB = session.Get<WatchDay>(watchDayFromClient.Id) ??
                            throw new CommandCentralException("Your watch day's id was not valid.  Please consider creating the watch day first.", HttpStatusCodes.BadRequest);

                        if (!token.AuthenticationSession.Person.PermissionGroups.Any(x => x.ChainsOfCommandMemberOf.Contains(watchDayFromDB.Watchbill.EligibilityGroup.OwningChainOfCommand) && x.AccessLevel == ChainOfCommandLevels.Command))
                            throw new CommandCentralException("You are not allowed to edit the structure of a watchbill tied to that eligibility group.  " +
                                "You must have command level permissions in the related chain of command.", HttpStatusCodes.Unauthorized);

                        //Ok let's swap the properties now.
                        //Check the state.
                        if (watchDayFromDB.Watchbill.CurrentState != Entities.ReferenceLists.Watchbill.WatchbillStatuses.Initial && watchDayFromClient.Date != watchDayFromDB.Date)
                            throw new CommandCentralException("You may not edit the structure of a watchbill that is not in the initial state.  Please consider changing its state first.", HttpStatusCodes.BadRequest);

                        watchDayFromDB.Date = watchDayFromClient.Date;
                        watchDayFromDB.Remarks = watchDayFromClient.Remarks;

                        //Let's also make sure that the updates to this watchbill didn't result in a validation failure.
                        var watchbillValidationResult = new Entities.Watchbill.Watchbill.WatchbillValidator().Validate(watchDayFromDB.Watchbill);

                        if (!watchbillValidationResult.IsValid)
                            throw new AggregateException(watchbillValidationResult.Errors.Select(x => new CommandCentralException(x.ErrorMessage, HttpStatusCodes.BadRequest)));

                        session.Update(watchDayFromDB);

                        token.SetResult(watchDayFromDB);

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
        /// Updates a watch day.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        [EndpointMethod(AllowArgumentLogging = true, AllowResponseLogging = true, RequiresAuthentication = true)]
        private static void DeleteWatchDay(MessageToken token)
        {
            token.AssertLoggedIn();
            token.Args.AssertContainsKeys("watchday");

            WatchDay watchDayFromClient;
            try
            {
                watchDayFromClient = token.Args["watchday"].CastJToken<WatchDay>();
            }
            catch
            {
                throw new CommandCentralException("An error occurred while trying to parse your watch day.", HttpStatusCodes.BadRequest);
            }

            using (var session = DataAccess.NHibernateHelper.CreateStatefulSession())
            {
                using (var transaction = session.BeginTransaction())
                {
                    try
                    {
                        var watchDayFromDB = session.Get<WatchDay>(watchDayFromClient.Id) ??
                            throw new CommandCentralException("Your watch day's id was not valid.  Please consider creating the watch day first.", HttpStatusCodes.BadRequest);

                        if (!token.AuthenticationSession.Person.PermissionGroups.Any(x => x.ChainsOfCommandMemberOf.Contains(watchDayFromDB.Watchbill.EligibilityGroup.OwningChainOfCommand) && x.AccessLevel == ChainOfCommandLevels.Command))
                            throw new CommandCentralException("You are not allowed to edit the structure of a watchbill tied to that eligibility group.  " +
                                "You must have command level permissions in the related chain of command.", HttpStatusCodes.Unauthorized);

                        //Check the state.
                        if (watchDayFromDB.Watchbill.CurrentState != Entities.ReferenceLists.Watchbill.WatchbillStatuses.Initial)
                            throw new CommandCentralException("You may not edit the structure of a watchbill that is not in the initial state.  Please consider changing its state first.", HttpStatusCodes.BadRequest);

                        session.Delete(watchDayFromDB);

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
