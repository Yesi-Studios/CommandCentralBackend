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
            {
                token.AddErrorMessage("You must be logged in to do that.", ErrorTypes.Authentication, System.Net.HttpStatusCode.Unauthorized);
                return;
            }

            if (!token.Args.ContainsKey("watchshiftid"))
            {
                token.AddErrorMessage("You failed to send a 'watchshiftid' parameter.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                return;
            }

            Guid watchShiftId;
            if (!Guid.TryParse(token.Args["watchshiftid"] as string, out watchShiftId))
            {
                token.AddErrorMessage("Your watch shift id parameter's format was invalid.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                return;
            }

            using (var session = DataAccess.NHibernateHelper.CreateStatefulSession())
            {
                using (var transaction = session.BeginTransaction())
                {
                    try
                    {
                        var watchShiftFromDB = session.Get<WatchShift>(watchShiftId);

                        if (watchShiftFromDB == null)
                        {
                            token.AddErrorMessage("Your watch shift's id was not valid.  Please consider creating the watch shift first.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                            return;
                        }

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
            {
                token.AddErrorMessage("You must be logged in to do that.", ErrorTypes.Authentication, System.Net.HttpStatusCode.Unauthorized);
                return;
            }

            if (!token.Args.ContainsKey("watchshifts"))
            {
                token.AddErrorMessage("You failed to send a 'watchshifts' parameter.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                return;
            }

            List<WatchShift> watchShiftsFromClient;
            try
            {
                watchShiftsFromClient = token.Args["watchshifts"].CastJToken<List<WatchShift>>();
            }
            catch
            {
                token.AddErrorMessage("An error occurred while trying to parse your watch shifts.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                return;
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

            if (validationResults.Any(x => !x.IsValid))
            {
                token.AddErrorMessages(validationResults.SelectMany(x => x.Errors).Select(x => x.ErrorMessage), ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                return;
            }

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
                            {
                                token.AddErrorMessage("One or more of your watch days' Ids were invalid.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                                return;
                            }

                            var watchbill = days.First().Watchbill;
                            if (!days.All(x => x.Watchbill.Id == watchbill.Id))
                            {
                                token.AddErrorMessage("Your requested watch days were not all from the same watchbill.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                                return;
                            }

                            if (!token.AuthenticationSession.Person.PermissionGroups.Any(x => x.ChainsOfCommandMemberOf.Contains(watchbill.ElligibilityGroup.OwningChainOfCommand) && x.AccessLevel == ChainOfCommandLevels.Command))
                            {
                                token.AddErrorMessage("You are not allowed to edit the structure of a watchbill tied to that elligibility group.  You must have command level permissions in the related chain of command.", ErrorTypes.Authorization, System.Net.HttpStatusCode.Unauthorized);
                                return;
                            }

                            //Check the state.
                            if (watchbill.CurrentState != Entities.ReferenceLists.Watchbill.WatchbillStatuses.Initial)
                            {
                                token.AddErrorMessage("You may not edit the structure of a watchbill that is not in the initial state.  Please consider changing its state first.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                                return;
                            }

                            session.Save(shift);
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
            {
                token.AddErrorMessage("You must be logged in to do that.", ErrorTypes.Authentication, System.Net.HttpStatusCode.Unauthorized);
                return;
            }

            if (!token.Args.ContainsKey("watchshift"))
            {
                token.AddErrorMessage("You failed to send a 'watchshift' parameter.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                return;
            }

            WatchShift watchShiftFromClient;
            try
            {
                watchShiftFromClient = token.Args["watchshift"].CastJToken<WatchShift>();
            }
            catch
            {
                token.AddErrorMessage("An error occurred while trying to parse your watch shift.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                return;
            }

            var valdiationResult = new WatchShift.WatchShiftValidator().Validate(watchShiftFromClient);

            if (!valdiationResult.IsValid)
            {
                token.AddErrorMessages(valdiationResult.Errors.Select(x => x.ErrorMessage), ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                return;
            }

            using (var session = DataAccess.NHibernateHelper.CreateStatefulSession())
            {
                using (var transaction = session.BeginTransaction())
                {
                    try
                    {
                        var watchShiftFromDB = session.Get<WatchShift>(watchShiftFromClient.Id);

                        if (watchShiftFromDB == null)
                        {
                            token.AddErrorMessage("Your watch shift's id was not valid.  Please consider creating the watch shift first.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                            return;
                        }

                        var watchbill = watchShiftFromDB.WatchDays.First().Watchbill;

                        if (!token.AuthenticationSession.Person.PermissionGroups.Any(x => x.ChainsOfCommandMemberOf.Contains(watchbill.ElligibilityGroup.OwningChainOfCommand) && x.AccessLevel == ChainOfCommandLevels.Command))
                        {
                            token.AddErrorMessage("You are not allowed to edit the structure of a watchbill tied to that elligibility group.  You must have command level permissions in the related chain of command.", ErrorTypes.Authorization, System.Net.HttpStatusCode.Unauthorized);
                            return;
                        }

                        //Ok let's swap the properties now.
                        //Check the state.
                        if (watchbill.CurrentState != Entities.ReferenceLists.Watchbill.WatchbillStatuses.Initial 
                            && (watchShiftFromClient.ShiftType != watchShiftFromDB.ShiftType || watchShiftFromClient.Range != watchShiftFromDB.Range))
                        {
                            token.AddErrorMessage("You may not edit the structure of a watchbill that is not in the initial state.  Please consider changing its state first.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                            return;
                        }

                        watchShiftFromDB.Title = watchShiftFromClient.Title;
                        watchShiftFromDB.Range = watchShiftFromClient.Range;
                        watchShiftFromDB.ShiftType = watchShiftFromClient.ShiftType;

                        //Let's also make sure that the updates to this watchbill didn't result in a validation failure.
                        var watchbillValidationResult = new Entities.Watchbill.Watchbill.WatchbillValidator().Validate(watchbill);

                        if (!watchbillValidationResult.IsValid)
                        {
                            token.AddErrorMessages(watchbillValidationResult.Errors.Select(x => x.ErrorMessage), ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                            return;
                        }

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
        /// Updates a watch day.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        [EndpointMethod(AllowArgumentLogging = true, AllowResponseLogging = true, RequiresAuthentication = true)]
        private static void DeleteWatchShift(MessageToken token)
        {
            //Just make sure the client is logged in.  The endpoint's description should've handled this but you never know.
            if (token.AuthenticationSession == null)
            {
                token.AddErrorMessage("You must be logged in to do that.", ErrorTypes.Authentication, System.Net.HttpStatusCode.Unauthorized);
                return;
            }

            if (!token.Args.ContainsKey("watchshift"))
            {
                token.AddErrorMessage("You failed to send a 'watchshift' parameter.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                return;
            }

            WatchShift watchShiftFromClient;
            try
            {
                watchShiftFromClient = token.Args["watchshift"].CastJToken<WatchShift>();
            }
            catch
            {
                token.AddErrorMessage("An error occurred while trying to parse your watch shift.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                return;
            }

            using (var session = DataAccess.NHibernateHelper.CreateStatefulSession())
            {
                using (var transaction = session.BeginTransaction())
                {
                    try
                    {
                        var watchShiftFromDB = session.Get<WatchShift>(watchShiftFromClient.Id);

                        if (watchShiftFromDB == null)
                        {
                            token.AddErrorMessage("Your watch shift's id was not valid.  Please consider creating the watch shift first.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                            return;
                        }

                        var watchbill = watchShiftFromDB.WatchDays.First().Watchbill;

                        if (!token.AuthenticationSession.Person.PermissionGroups.Any(x => x.ChainsOfCommandMemberOf.Contains(watchbill.ElligibilityGroup.OwningChainOfCommand) && x.AccessLevel == ChainOfCommandLevels.Command))
                        {
                            token.AddErrorMessage("You are not allowed to edit the structure of a watchbill tied to that elligibility group.  You must have command level permissions in the related chain of command.", ErrorTypes.Authorization, System.Net.HttpStatusCode.Unauthorized);
                            return;
                        }

                        //Check the state.
                        if (watchbill.CurrentState != Entities.ReferenceLists.Watchbill.WatchbillStatuses.Initial)
                        {
                            token.AddErrorMessage("You may not edit the structure of a watchbill that is not in the initial state.  Please consider changing its state first.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                            return;
                        }

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
