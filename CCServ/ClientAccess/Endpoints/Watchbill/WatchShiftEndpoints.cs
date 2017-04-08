using CCServ.Entities.Watchbill;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AtwoodUtils;

namespace CCServ.ClientAccess.Endpoints.Watchbill
{
    /// <summary>
    /// Contains all of the endpoints for interacting with the watch shift object.
    /// </summary>
    static class WatchShiftEndpoints
    {
        /// <summary>
        /// WARNING!  THIS METHOD IS EXPOSED TO THE CLIENT AND IS NOT INTENDED FOR INTERNAL USE.  AUTHENTICATION, AUTHORIZATION AND VALIDATION MUST BE HANDLED PRIOR TO DB INTERACTION.
        /// <para />
        /// Loads a watch shift.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        [EndpointMethod(AllowArgumentLogging = true, AllowResponseLogging = true, RequiresAuthentication = true)]
        private static void LoadWatchShift(MessageToken token)
        {
            //Just make sure the client is logged in.  The endpoint's description should've handled this but you never know.
            if (token.AuthenticationSession == null)
                throw new CommandCentralException("You must be logged in to do that.", HttpStatusCodes.AuthenticationFailed);

            token.Args.AssertContainsKeys("watchshiftid");

            if (!Guid.TryParse(token.Args["watchshiftid"] as string, out Guid watchShiftId))
                throw new CommandCentralException("Your watch shift id parameter's format was invalid.", HttpStatusCodes.BadRequest);

            using (var session = DataAccess.NHibernateHelper.CreateStatefulSession())
            {
                using (var transaction = session.BeginTransaction())
                {
                    try
                    {
                        var watchShiftFromDB = session.Get<WatchShift>(watchShiftId) ??
                            throw new CommandCentralException("Your watch shift's id was not valid.  Please consider creating the watch shift first.", HttpStatusCodes.BadRequest);

                        token.SetResult(watchShiftFromDB);

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
        /// Creates multiple watch shifts.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        [EndpointMethod(AllowArgumentLogging = true, AllowResponseLogging = true, RequiresAuthentication = true)]
        private static void CreateWatchShifts(MessageToken token)
        {
            //Just make sure the client is logged in.  The endpoint's description should've handled this but you never know.
            if (token.AuthenticationSession == null)
                throw new CommandCentralException("You must be logged in to do that.", HttpStatusCodes.AuthenticationFailed);

            token.Args.AssertContainsKeys("watchshifts");

            List<WatchShift> watchShiftsFromClient;
            try
            {
                watchShiftsFromClient = token.Args["watchshifts"].CastJToken<List<WatchShift>>();
            }
            catch
            {
                throw new CommandCentralException("An error occurred while trying to parse your watch shifts.", HttpStatusCodes.BadRequest);
            }

            var watchShiftsToInsert = watchShiftsFromClient.Select(x => new WatchShift
            {
                Id = Guid.NewGuid(),
                Range = x.Range,
                ShiftType = x.ShiftType,
                Title = x.Title,
                WatchDays = x.WatchDays
            }).ToList();

            var validationResults = watchShiftsToInsert.Select(x => new WatchShift.WatchShiftValidator().Validate(x)).ToList();
            var invalidResults = validationResults.Where(x => !x.IsValid);
            if (invalidResults.Any())
                throw new AggregateException(invalidResults.SelectMany(x => x.Errors.Select(y => new CommandCentralException(y.ErrorMessage, HttpStatusCodes.BadRequest))));

            using (var session = DataAccess.NHibernateHelper.CreateStatefulSession())
            {
                using (var transaction = session.BeginTransaction())
                {
                    try
                    {
                        foreach (var shift in watchShiftsToInsert)
                        {
                            //Let's get the days the client says this shift will be a part of.
                            var days = session.QueryOver<WatchDay>()
                                .WhereRestrictionOn(x => x.Id)
                                .IsIn(shift.WatchDays.Select(x => x.Id).Cast<object>().ToArray())
                                .List();

                            if (days.Count != shift.WatchDays.Count)
                                throw new CommandCentralException("One or more of your watch days' Ids were invalid.", HttpStatusCodes.BadRequest);

                            var watchbill = days.First().Watchbill;
                            if (days.Any(x => x.Watchbill.Id != watchbill.Id))
                                throw new CommandCentralException("Your requested watch days were not all from the same watchbill.", HttpStatusCodes.BadRequest);

                            if (!token.AuthenticationSession.Person.PermissionGroups.Any(x => x.ChainsOfCommandMemberOf.Contains(watchbill.EligibilityGroup.OwningChainOfCommand) && x.AccessLevel == ChainOfCommandLevels.Command))
                                throw new CommandCentralException("You are not allowed to edit the structure of a watchbill tied to that eligibility group.  " +
                                    "You must have command level permissions in the related chain of command.", HttpStatusCodes.Forbidden);

                            //Check the state.
                            if (watchbill.CurrentState != Entities.ReferenceLists.Watchbill.WatchbillStatuses.Initial)
                                throw new CommandCentralException("You may not edit the structure of a watchbill that is not in the initial state.  " +
                                    "Please consider changing its state first.", HttpStatusCodes.BadRequest);

                            foreach (var day in days)
                            {
                                day.WatchShifts.Add(shift);
                            }

                            session.Update(watchbill);
                        }

                        token.SetResult(watchShiftsToInsert);

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
        /// Updates a watch shift.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        [EndpointMethod(AllowArgumentLogging = true, AllowResponseLogging = true, RequiresAuthentication = true)]
        private static void UpdateWatchShift(MessageToken token)
        {
            //Just make sure the client is logged in.  The endpoint's description should've handled this but you never know.
            if (token.AuthenticationSession == null)
                throw new CommandCentralException("You must be logged in to do that.", HttpStatusCodes.AuthenticationFailed);

            token.Args.AssertContainsKeys("watchshift");

            WatchShift watchShiftFromClient;
            try
            {
                watchShiftFromClient = token.Args["watchshift"].CastJToken<WatchShift>();
            }
            catch
            {
                throw new CommandCentralException("An error occurred while trying to parse your watch shift.", HttpStatusCodes.BadRequest);
            }

            var valdiationResult = new WatchShift.WatchShiftValidator().Validate(watchShiftFromClient);

            if (!valdiationResult.IsValid)
                throw new AggregateException(valdiationResult.Errors.Select(x => new CommandCentralException(x.ErrorMessage, HttpStatusCodes.BadRequest)));

            using (var session = DataAccess.NHibernateHelper.CreateStatefulSession())
            {
                using (var transaction = session.BeginTransaction())
                {
                    try
                    {
                        var watchShiftFromDB = session.Get<WatchShift>(watchShiftFromClient.Id) ??
                            throw new CommandCentralException("Your watch shift's id was not valid.  Please consider creating the watch shift first.", HttpStatusCodes.BadRequest);

                        var watchbill = watchShiftFromDB.WatchDays.First().Watchbill;

                        if (!token.AuthenticationSession.Person.PermissionGroups.Any(x => x.ChainsOfCommandMemberOf.Contains(watchbill.EligibilityGroup.OwningChainOfCommand) && x.AccessLevel == ChainOfCommandLevels.Command))
                            throw new CommandCentralException("You are not allowed to edit the structure of a watchbill tied to that eligibility group.  " +
                                "You must have command level permissions in the related chain of command.", HttpStatusCodes.Unauthorized);

                        //Ok let's swap the properties now.
                        //Check the state.
                        if (watchbill.CurrentState != Entities.ReferenceLists.Watchbill.WatchbillStatuses.Initial 
                            && (watchShiftFromClient.ShiftType != watchShiftFromDB.ShiftType || watchShiftFromClient.Range != watchShiftFromDB.Range))
                            throw new CommandCentralException("You may not edit the structure of a watchbill that is not in the initial state.  " +
                                "Please consider changing its state first.", HttpStatusCodes.BadRequest);

                        watchShiftFromDB.Title = watchShiftFromClient.Title;
                        watchShiftFromDB.Range = watchShiftFromClient.Range;
                        watchShiftFromDB.ShiftType = watchShiftFromClient.ShiftType;

                        //Let's also make sure that the updates to this watchbill didn't result in a validation failure.
                        var watchbillValidationResult = new Entities.Watchbill.Watchbill.WatchbillValidator().Validate(watchbill);

                        if (!watchbillValidationResult.IsValid)
                            throw new AggregateException(watchbillValidationResult.Errors.Select(x => new CommandCentralException(x.ErrorMessage, HttpStatusCodes.BadRequest)));

                        session.Update(watchShiftFromDB);

                        token.SetResult(watchShiftFromDB);

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
        /// Deletes a watch shift.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        [EndpointMethod(AllowArgumentLogging = true, AllowResponseLogging = true, RequiresAuthentication = true)]
        private static void DeleteWatchShift(MessageToken token)
        {
            //Just make sure the client is logged in.  The endpoint's description should've handled this but you never know.
            if (token.AuthenticationSession == null)
                throw new CommandCentralException("You must be logged in to do that.", HttpStatusCodes.AuthenticationFailed);

            token.Args.AssertContainsKeys("watchshift");

            WatchShift watchShiftFromClient;
            try
            {
                watchShiftFromClient = token.Args["watchshift"].CastJToken<WatchShift>();
            }
            catch
            {
                throw new CommandCentralException("An error occurred while trying to parse your watch shift.", HttpStatusCodes.BadRequest);
            }

            using (var session = DataAccess.NHibernateHelper.CreateStatefulSession())
            {
                using (var transaction = session.BeginTransaction())
                {
                    try
                    {
                        var watchShiftFromDB = session.Get<WatchShift>(watchShiftFromClient.Id) ??
                            throw new CommandCentralException("Your watch shift's id was not valid.  " +
                            "Please consider creating the watch shift first.", HttpStatusCodes.BadRequest);

                        var watchbill = watchShiftFromDB.WatchDays.First().Watchbill;

                        if (!token.AuthenticationSession.Person.PermissionGroups.Any(x => x.ChainsOfCommandMemberOf.Contains(watchbill.EligibilityGroup.OwningChainOfCommand) && x.AccessLevel == ChainOfCommandLevels.Command))
                            throw new CommandCentralException("You are not allowed to edit the structure of a watchbill tied to that eligibility group.  " +
                                "You must have command level permissions in the related chain of command.", HttpStatusCodes.Unauthorized);

                        //Check the state.
                        if (watchbill.CurrentState != Entities.ReferenceLists.Watchbill.WatchbillStatuses.Initial)
                            throw new CommandCentralException("You may not edit the structure of a watchbill that is not in the initial state.  " +
                                "Please consider changing its state first.", HttpStatusCodes.BadRequest);

                        session.Delete(watchShiftFromDB);

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
