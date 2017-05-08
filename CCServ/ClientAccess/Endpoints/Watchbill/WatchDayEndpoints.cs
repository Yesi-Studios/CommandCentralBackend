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
                throw new CommandCentralException("Your watch day id parameter's format was invalid.", ErrorTypes.Validation);

            //Now let's go get the watch day from the database.
            using (var session = DataAccess.NHibernateHelper.CreateStatefulSession())
            {
                using (var transaction = session.BeginTransaction())
                {
                    try
                    {
                        var watchDayFromDB = session.Get<WatchDay>(watchDayId) ??
                            throw new CommandCentralException("Your watch day's id was not valid.  Please consider creating the watch day first.", ErrorTypes.Validation);

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
            token.Args.AssertContainsKeys("watchdays", "watchbillid");

            var watchDaysToken = token.Args["watchdays"].CastJToken();

            if (watchDaysToken.Type != Newtonsoft.Json.Linq.JTokenType.Array)
                throw new CommandCentralException("Your watch days parameter must be an array.", ErrorTypes.Validation);

            var watchDaysFromClient = new List<WatchDay>();

            foreach (var day in watchDaysToken)
            {
                watchDaysFromClient.Add(new WatchDay
                {
                    Date = day.Value<DateTime>(nameof(WatchDay.Date)),
                    Id = Guid.NewGuid(),
                    Remarks = day.Value<string>(nameof(WatchDay.Remarks))
                });
            }

            if (!Guid.TryParse(token.Args["watchbillid"] as string, out Guid watchbillId))
                throw new CommandCentralException("Your watchbill id was in the wrong format.", ErrorTypes.Validation);

            using (var session = DataAccess.NHibernateHelper.CreateStatefulSession())
            {
                using (var transaction = session.BeginTransaction())
                {
                    try
                    {
                        var watchbill = session.Get<Entities.Watchbill.Watchbill>(watchbillId) ??
                            throw new CommandCentralException("Your watchbill was not valid.", ErrorTypes.Validation);

                        foreach (var day in watchDaysFromClient)
                        {
                            day.Watchbill = watchbill;

                            var validationResult = new WatchDay.WatchDayValidator().Validate(day);
                            if (!validationResult.IsValid)
                                throw new AggregateException(validationResult.Errors.Select(x => new CommandCentralException(x.ErrorMessage, ErrorTypes.Validation)));

                            if (!token.AuthenticationSession.Person.PermissionGroups.Any(x => x.ChainsOfCommandMemberOf.Contains(watchbill.EligibilityGroup.OwningChainOfCommand) && x.AccessLevel == ChainOfCommandLevels.Command))
                                throw new CommandCentralException("You are not allowed to edit the structure of a watchbill tied to that eligibility group.  " +
                                    "You must have command level permissions in the related chain of command.", ErrorTypes.Authorization);

                            //Check the state.
                            if (watchbill.CurrentState != Entities.ReferenceLists.Watchbill.WatchbillStatuses.Initial)
                                throw new CommandCentralException("You may not edit the structure of a watchbill that is not in the initial state.  " +
                                    "Please consider changing its state first.", ErrorTypes.Validation);

                            watchbill.WatchDays.Add(day);

                            day.Watchbill = watchbill;

                            session.Update(watchbill);
                        }

                        token.SetResult(watchDaysFromClient);

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

            var watchDayToken = token.Args["watchday"].CastJToken();

            if (!Guid.TryParse(watchDayToken.Value<string>(nameof(WatchDay.Id)), out Guid id))
                throw new CommandCentralException("Your watch day id was in the wrong format.", ErrorTypes.Validation);

            var dto = new
            {
                Date  = watchDayToken.Value<DateTime>(nameof(WatchDay.Date)),
                Remarks = watchDayToken.Value<string>(nameof(WatchDay.Remarks)),
                Id = id
            };

            using (var session = DataAccess.NHibernateHelper.CreateStatefulSession())
            {
                using (var transaction = session.BeginTransaction())
                {
                    try
                    {
                        var watchDayFromDB = session.Get<WatchDay>(dto.Id) ??
                            throw new CommandCentralException("Your watch day's id was not valid.  Please consider creating the watch day first.", ErrorTypes.Validation);

                        if (!token.AuthenticationSession.Person.PermissionGroups.Any(x => x.ChainsOfCommandMemberOf.Contains(watchDayFromDB.Watchbill.EligibilityGroup.OwningChainOfCommand) && x.AccessLevel == ChainOfCommandLevels.Command))
                            throw new CommandCentralException("You are not allowed to edit the structure of a watchbill tied to that eligibility group.  " +
                                "You must have command level permissions in the related chain of command.", ErrorTypes.Authorization);

                        //Ok let's swap the properties now.
                        //Check the state.
                        if (watchDayFromDB.Watchbill.CurrentState != Entities.ReferenceLists.Watchbill.WatchbillStatuses.Initial && dto.Date != watchDayFromDB.Date)
                            throw new CommandCentralException("You may not edit the structure of a watchbill that is not in the initial state.  Please consider changing its state first.", ErrorTypes.Validation);

                        watchDayFromDB.Date = dto.Date;
                        watchDayFromDB.Remarks = dto.Remarks;

                        //Let's also make sure that the updates to this watchbill didn't result in a validation failure.
                        var watchbillValidationResult = new Entities.Watchbill.Watchbill.WatchbillValidator().Validate(watchDayFromDB.Watchbill);

                        if (!watchbillValidationResult.IsValid)
                            throw new AggregateException(watchbillValidationResult.Errors.Select(x => new CommandCentralException(x.ErrorMessage, ErrorTypes.Validation)));

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
        /// Deletes a watch day.
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
                throw new CommandCentralException("An error occurred while trying to parse your watch day.", ErrorTypes.Validation);
            }

            using (var session = DataAccess.NHibernateHelper.CreateStatefulSession())
            {
                using (var transaction = session.BeginTransaction())
                {
                    try
                    {
                        var watchDayFromDB = session.Get<WatchDay>(watchDayFromClient.Id) ??
                            throw new CommandCentralException("Your watch day's id was not valid.  Please consider creating the watch day first.", ErrorTypes.Validation);

                        if (!token.AuthenticationSession.Person.PermissionGroups.Any(x => x.ChainsOfCommandMemberOf.Contains(watchDayFromDB.Watchbill.EligibilityGroup.OwningChainOfCommand) && x.AccessLevel == ChainOfCommandLevels.Command))
                            throw new CommandCentralException("You are not allowed to edit the structure of a watchbill tied to that eligibility group.  " +
                                "You must have command level permissions in the related chain of command.", ErrorTypes.Authorization);

                        //Check the state.
                        if (watchDayFromDB.Watchbill.CurrentState != Entities.ReferenceLists.Watchbill.WatchbillStatuses.Initial)
                            throw new CommandCentralException("You may not edit the structure of a watchbill that is not in the initial state.  Please consider changing its state first.", ErrorTypes.Validation);

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

        /// <summary>
        /// WARNING!  THIS METHOD IS EXPOSED TO THE CLIENT AND IS NOT INTENDED FOR INTERNAL USE.  AUTHENTICATION, AUTHORIZATION AND VALIDATION MUST BE HANDLED PRIOR TO DB INTERACTION.
        /// <para />
        /// Deletes multiple watch days.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        [EndpointMethod(AllowArgumentLogging = true, AllowResponseLogging = true, RequiresAuthentication = true)]
        private static void DeleteWatchDays(MessageToken token)
        {
            token.AssertLoggedIn();
            token.Args.AssertContainsKeys("ids");

            var idsToken = token.Args["ids"].CastJToken();

            if (idsToken.Type != Newtonsoft.Json.Linq.JTokenType.Array)
                throw new CommandCentralException("Your ids parameter must be in an array.", ErrorTypes.Validation);

            var ids = idsToken.Select(x =>
            {
                if (!Guid.TryParse(x.ToString(), out Guid id))
                    throw new CommandCentralException("One or more of your ids were in the wrong format.", ErrorTypes.Validation);

                return id;
            });

            using (var session = DataAccess.NHibernateHelper.CreateStatefulSession())
            {
                using (var transaction = session.BeginTransaction())
                {
                    try
                    {
                        foreach (var id in ids)
                        {
                            var watchDayFromDB = session.Get<WatchDay>(id) ??
                                throw new CommandCentralException("Your watch day's id was not valid.  Please consider creating the watch day first.", ErrorTypes.Validation);

                            if (!token.AuthenticationSession.Person.PermissionGroups.Any(x => x.ChainsOfCommandMemberOf.Contains(watchDayFromDB.Watchbill.EligibilityGroup.OwningChainOfCommand) && x.AccessLevel == ChainOfCommandLevels.Command))
                                throw new CommandCentralException("You are not allowed to edit the structure of a watchbill tied to that eligibility group.  " +
                                    "You must have command level permissions in the related chain of command.", ErrorTypes.Authorization);
                            
                            //Check the state.
                            if (watchDayFromDB.Watchbill.CurrentState != Entities.ReferenceLists.Watchbill.WatchbillStatuses.Initial)
                                throw new CommandCentralException("You may not edit the structure of a watchbill that is not in the initial state.  Please consider changing its state first.", ErrorTypes.Validation);

                            session.Delete(watchDayFromDB);
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
}
