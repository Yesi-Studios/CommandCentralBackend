﻿using CommandCentral.Entities.Watchbill;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AtwoodUtils;
using Itenso.TimePeriod;
using CommandCentral.Authorization;

namespace CommandCentral.ClientAccess.Endpoints.Watchbill
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
            token.AssertLoggedIn();
            token.Args.AssertContainsKeys("id");

            if (!Guid.TryParse(token.Args["id"] as string, out var watchShiftId))
                throw new CommandCentralException("Your watch shift id parameter's format was invalid.", ErrorTypes.Validation);

            using (var session = DataAccess.DataProvider.CreateStatefulSession())
            {
                using (var transaction = session.BeginTransaction())
                {
                    try
                    {
                        var watchShiftFromDB = session.Get<WatchShift>(watchShiftId) ??
                            throw new CommandCentralException("Your watch shift's id was not valid.  Please consider creating the watch shift first.", ErrorTypes.Validation);

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
            token.AssertLoggedIn();
            token.Args.AssertContainsKeys("watchshifts", "watchbillid");

            List<WatchShift> watchShiftsFromClient;
            try
            {
                watchShiftsFromClient = token.Args["watchshifts"].CastJToken<List<WatchShift>>();
            }
            catch
            {
                throw new CommandCentralException("An error occurred while trying to parse your watch shifts.", ErrorTypes.Validation);
            }

            if (!Guid.TryParse(token.Args["watchbillid"] as string, out var watchbillId))
                throw new CommandCentralException("Your watchbill id was in the wrong format.", ErrorTypes.Validation);

            var watchShiftsToInsert = watchShiftsFromClient.Select(x => new WatchShift
            {
                Id = Guid.NewGuid(),
                Range = x.Range,
                ShiftType = x.ShiftType,
                Title = x.Title,
                Points = x.Points
            }).ToList();

            using (var session = DataAccess.DataProvider.CreateStatefulSession())
            {
                using (var transaction = session.BeginTransaction())
                {
                    try
                    {
                        var watchbill = session.Get<Entities.Watchbill.Watchbill>(watchbillId) ??
                            throw new CommandCentralException("Your watchbill id was not valid.", ErrorTypes.Validation);

                        if (token.AuthenticationSession.Person.ResolvePermissions(null).HighestLevels[watchbill.EligibilityGroup.OwningChainOfCommand] != ChainOfCommandLevels.Command)
                        {
                            throw new CommandCentralException("You are not allowed to edit the structure of a watchbill tied to that eligibility group.  " +
                                "You must have command level permissions in the related chain of command.", ErrorTypes.Authorization);
                        }

                        //Check the state.
                        if (!watchbill.CanEditStructure())
                            throw new CommandCentralException("You may not edit the structure of a watchbill that is not in the initial or open for inputs states.  " +
                                "Please consider changing its state first.", ErrorTypes.Validation);


                        foreach (var shift in watchShiftsToInsert)
                        {
                            if (shift.ShiftType == null)
                                throw new CommandCentralException("Your watch shift type is invalid.", ErrorTypes.Validation);

                            //Check the shift type.
                            var shiftTypeFromDB = session.Get<Entities.ReferenceLists.Watchbill.WatchShiftType>(shift.ShiftType.Id) ??
                                throw new CommandCentralException("Your shift type was not valid.", ErrorTypes.Validation);

                            shift.ShiftType = shiftTypeFromDB;

                            var shiftRange = new Itenso.TimePeriod.TimeRange(shift.Range.Start, shift.Range.End);

                            //We also need to set the watchbill of the watch shift and vice versa.
                            shift.Watchbill = watchbill;
                            watchbill.WatchShifts.Add(shift);
                        }

                        var validationResult = new Entities.Watchbill.Watchbill.WatchbillValidator().Validate(watchbill);
                        if (!validationResult.IsValid)
                            throw new AggregateException(validationResult.Errors.Select(x => new CommandCentralException(x.ErrorMessage, ErrorTypes.Validation)));

                        session.Update(watchbill);

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
            token.AssertLoggedIn();
            token.Args.AssertContainsKeys("watchshift");

            WatchShift watchShiftFromClient;
            try
            {
                watchShiftFromClient = token.Args["watchshift"].CastJToken<WatchShift>();
            }
            catch
            {
                throw new CommandCentralException("An error occurred while trying to parse your watch shift.", ErrorTypes.Validation);
            }

            var valdiationResult = new WatchShift.WatchShiftValidator().Validate(watchShiftFromClient);

            if (!valdiationResult.IsValid)
                throw new AggregateException(valdiationResult.Errors.Select(x => new CommandCentralException(x.ErrorMessage, ErrorTypes.Validation)));

            using (var session = DataAccess.DataProvider.CreateStatefulSession())
            {
                using (var transaction = session.BeginTransaction())
                {
                    try
                    {
                        var watchShiftFromDB = session.Get<WatchShift>(watchShiftFromClient.Id) ??
                            throw new CommandCentralException("Your watch shift's id was not valid.  Please consider creating the watch shift first.", ErrorTypes.Validation);

                        if (token.AuthenticationSession.Person.ResolvePermissions(null).HighestLevels[watchShiftFromDB.Watchbill.EligibilityGroup.OwningChainOfCommand] != ChainOfCommandLevels.Command)
                        {
                            throw new CommandCentralException("You are not allowed to edit the structure of a watchbill tied to that eligibility group.  " +
                                "You must have command level permissions in the related chain of command.", ErrorTypes.Authorization);
                        }

                        //Check the state.
                        if (!watchShiftFromDB.Watchbill.CanEditStructure())
                            throw new CommandCentralException("You may not edit the structure of a watchbill that is not in the initial or open for inputs states.  " +
                                "Please consider changing its state first.", ErrorTypes.Validation);

                        watchShiftFromDB.Title = watchShiftFromClient.Title;
                        watchShiftFromDB.Range = watchShiftFromClient.Range;
                        watchShiftFromDB.ShiftType = watchShiftFromClient.ShiftType;

                        //Let's also make sure that the updates to this watchbill didn't result in a validation failure.
                        var watchbillValidationResult = new Entities.Watchbill.Watchbill.WatchbillValidator().Validate(watchShiftFromDB.Watchbill);

                        if (!watchbillValidationResult.IsValid)
                            throw new AggregateException(watchbillValidationResult.Errors.Select(x => new CommandCentralException(x.ErrorMessage, ErrorTypes.Validation)));

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
            token.AssertLoggedIn();
            token.Args.AssertContainsKeys("id");

            if (!Guid.TryParse(token.Args["id"] as string, out var id))
                throw new CommandCentralException("Your id was in the wrong format.", ErrorTypes.Validation);

            using (var session = DataAccess.DataProvider.CreateStatefulSession())
            {
                using (var transaction = session.BeginTransaction())
                {
                    try
                    {
                        var watchShiftFromDB = session.Get<WatchShift>(id) ??
                            throw new CommandCentralException("Your watch shift's id was not valid.  " +
                            "Please consider creating the watch shift first.", ErrorTypes.Validation);

                        if (token.AuthenticationSession.Person.ResolvePermissions(null).HighestLevels[watchShiftFromDB.Watchbill.EligibilityGroup.OwningChainOfCommand] != ChainOfCommandLevels.Command)
                        {
                            throw new CommandCentralException("You are not allowed to edit the structure of a watchbill tied to that eligibility group.  " +
                                "You must have command level permissions in the related chain of command.", ErrorTypes.Authorization);
                        }

                        //Check the state.
                        if (!watchShiftFromDB.Watchbill.CanEditStructure())
                            throw new CommandCentralException("You may not edit the structure of a watchbill that is not in the initial or open for inputs states.  " +
                                "Please consider changing its state first.", ErrorTypes.Validation);


                        watchShiftFromDB.Watchbill.WatchShifts.Remove(watchShiftFromDB);

                        session.Update(watchShiftFromDB);

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
        /// Deletes watch shifts.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        [EndpointMethod(AllowArgumentLogging = true, AllowResponseLogging = true, RequiresAuthentication = true)]
        private static void DeleteWatchShifts(MessageToken token)
        {
            token.AssertLoggedIn();
            token.Args.AssertContainsKeys("ids");

            var ids = token.Args["ids"].CastJToken<List<Guid>>();

            using (var session = DataAccess.DataProvider.CreateStatefulSession())
            {
                using (var transaction = session.BeginTransaction())
                {
                    try
                    {
                        foreach (var id in ids)
                        {
                            var watchShiftFromDB = session.Get<WatchShift>(id) ??
                            throw new CommandCentralException("Your watch shift's id was not valid.  " +
                            "Please consider creating the watch shift first.", ErrorTypes.Validation);

                            if (token.AuthenticationSession.Person.ResolvePermissions(null).HighestLevels[watchShiftFromDB.Watchbill.EligibilityGroup.OwningChainOfCommand] != ChainOfCommandLevels.Command)
                            {
                                throw new CommandCentralException("You are not allowed to edit the structure of a watchbill tied to that eligibility group.  " +
                                    "You must have command level permissions in the related chain of command.", ErrorTypes.Authorization);
                            }

                            //Check the state.
                            if (!watchShiftFromDB.Watchbill.CanEditStructure())
                                throw new CommandCentralException("You may not edit the structure of a watchbill that is not in the initial or open for inputs states.  " +
                                    "Please consider changing its state first.", ErrorTypes.Validation);


                            watchShiftFromDB.Watchbill.WatchShifts.Remove(watchShiftFromDB);

                            session.Update(watchShiftFromDB);
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
