using CCServ.Entities.Watchbill;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AtwoodUtils;
using Itenso.TimePeriod;

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
            token.AssertLoggedIn();
            token.Args.AssertContainsKeys("watchshiftid");

            if (!Guid.TryParse(token.Args["watchshiftid"] as string, out Guid watchShiftId))
                throw new CommandCentralException("Your watch shift id parameter's format was invalid.", ErrorTypes.Validation);

            using (var session = DataAccess.NHibernateHelper.CreateStatefulSession())
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

            if (!Guid.TryParse(token.Args["watchbillid"] as string, out Guid watchbillId))
                throw new CommandCentralException("Your watchbill id was in the wrong format.", ErrorTypes.Validation);

            var watchShiftsToInsert = watchShiftsFromClient.Select(x => new WatchShift
            {
                Id = Guid.NewGuid(),
                Range = x.Range,
                ShiftType = x.ShiftType,
                Title = x.Title,
                WatchDays = new List<WatchDay>(),
                Points = x.Points
            }).ToList();

            using (var session = DataAccess.NHibernateHelper.CreateStatefulSession())
            {
                using (var transaction = session.BeginTransaction())
                {
                    try
                    {
                        var watchbill = session.Get<Entities.Watchbill.Watchbill>(watchbillId) ??
                            throw new CommandCentralException("Your watchbill id was not valid.", ErrorTypes.Validation);

                        if (!token.AuthenticationSession.Person.PermissionGroups.Any(x => x.ChainsOfCommandMemberOf.Contains(watchbill.EligibilityGroup.OwningChainOfCommand) && x.AccessLevel == ChainOfCommandLevels.Command))
                            throw new CommandCentralException("You are not allowed to edit the structure of a watchbill tied to that eligibility group.  " +
                                "You must have command level permissions in the related chain of command.", ErrorTypes.Validation);

                        //Check the state.
                        if (watchbill.CurrentState != Entities.ReferenceLists.Watchbill.WatchbillStatuses.Initial)
                            throw new CommandCentralException("You may not edit the structure of a watchbill that is not in the initial state.  " +
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

                            foreach (var day in watchbill.WatchDays)
                            {
                                if (shiftRange.IntersectsWith(new Itenso.TimePeriod.TimeRange(day.Date, day.Date.AddHours(24))))
                                {
                                    shift.WatchDays.Add(day);
                                    day.WatchShifts.Add(shift);
                                }
                            }

                            if (!shift.WatchDays.Any())
                                throw new CommandCentralException("Your shift falls outside the range of the watchbill's days.  Please consider creating a day first that encompasses the shift's duration.", ErrorTypes.Validation);
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

            using (var session = DataAccess.NHibernateHelper.CreateStatefulSession())
            {
                using (var transaction = session.BeginTransaction())
                {
                    try
                    {
                        var watchShiftFromDB = session.Get<WatchShift>(watchShiftFromClient.Id) ??
                            throw new CommandCentralException("Your watch shift's id was not valid.  Please consider creating the watch shift first.", ErrorTypes.Validation);

                        var watchbill = watchShiftFromDB.WatchDays.First().Watchbill;

                        if (!token.AuthenticationSession.Person.PermissionGroups.Any(x => x.ChainsOfCommandMemberOf.Contains(watchbill.EligibilityGroup.OwningChainOfCommand) && x.AccessLevel == ChainOfCommandLevels.Command))
                            throw new CommandCentralException("You are not allowed to edit the structure of a watchbill tied to that eligibility group.  " +
                                "You must have command level permissions in the related chain of command.", ErrorTypes.Authorization);

                        //Ok let's swap the properties now.
                        //Check the state.
                        if (watchbill.CurrentState != Entities.ReferenceLists.Watchbill.WatchbillStatuses.Initial 
                            && (watchShiftFromClient.ShiftType != watchShiftFromDB.ShiftType || watchShiftFromClient.Range != watchShiftFromDB.Range))
                            throw new CommandCentralException("You may not edit the structure of a watchbill that is not in the initial state.  " +
                                "Please consider changing its state first.", ErrorTypes.Validation);

                        watchShiftFromDB.Title = watchShiftFromClient.Title;
                        watchShiftFromDB.Range = watchShiftFromClient.Range;
                        watchShiftFromDB.ShiftType = watchShiftFromClient.ShiftType;

                        //Let's also make sure that the updates to this watchbill didn't result in a validation failure.
                        var watchbillValidationResult = new Entities.Watchbill.Watchbill.WatchbillValidator().Validate(watchbill);

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

            using (var session = DataAccess.NHibernateHelper.CreateStatefulSession())
            {
                using (var transaction = session.BeginTransaction())
                {
                    try
                    {
                        var watchShiftFromDB = session.Get<WatchShift>(watchShiftFromClient.Id) ??
                            throw new CommandCentralException("Your watch shift's id was not valid.  " +
                            "Please consider creating the watch shift first.", ErrorTypes.Validation);

                        var watchbill = watchShiftFromDB.WatchDays.First().Watchbill;

                        if (!token.AuthenticationSession.Person.PermissionGroups.Any(x => x.ChainsOfCommandMemberOf.Contains(watchbill.EligibilityGroup.OwningChainOfCommand) && x.AccessLevel == ChainOfCommandLevels.Command))
                            throw new CommandCentralException("You are not allowed to edit the structure of a watchbill tied to that eligibility group.  " +
                                "You must have command level permissions in the related chain of command.", ErrorTypes.Authorization);

                        //Check the state.
                        if (watchbill.CurrentState != Entities.ReferenceLists.Watchbill.WatchbillStatuses.Initial)
                            throw new CommandCentralException("You may not edit the structure of a watchbill that is not in the initial state.  " +
                                "Please consider changing its state first.", ErrorTypes.Validation);

                        foreach (var day in watchShiftFromDB.WatchDays)
                            day.WatchShifts.Remove(watchShiftFromDB);

                        session.Update(watchbill);

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
