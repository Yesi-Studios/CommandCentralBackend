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
            //Just make sure the client is logged in.  The endpoint's description should've handled this but you never know.
            if (token.AuthenticationSession == null)
            {
                token.AddErrorMessage("You must be logged in to do that.", ErrorTypes.Authentication, System.Net.HttpStatusCode.Unauthorized);
                return;
            }

            if (!token.Args.ContainsKey("watchdayid"))
            {
                token.AddErrorMessage("You failed to send a 'watchdayid' parameter.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                return;
            }

            Guid watchDayId;
            if (!Guid.TryParse(token.Args["watchdayid"] as string, out watchDayId))
            {
                token.AddErrorMessage("Your watch day id parameter's format was invalid.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                return;
            }

            //Now let's go get the watch day from the database.
            using (var session = DataAccess.NHibernateHelper.CreateStatefulSession())
            {
                using (var transaction = session.BeginTransaction())
                {
                    try
                    {
                        var watchDayFromDB = session.Get<WatchDay>(watchDayId);

                        if (watchDayFromDB == null)
                        {
                            token.AddErrorMessage("Your watch day's id was not valid.  Please consider creating the watch day first.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                            return;
                        }

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
            //Just make sure the client is logged in.  The endpoint's description should've handled this but you never know.
            if (token.AuthenticationSession == null)
            {
                token.AddErrorMessage("You must be logged in to do that.", ErrorTypes.Authentication, System.Net.HttpStatusCode.Unauthorized);
                return;
            }

            if (!token.Args.ContainsKey("watchdays"))
            {
                token.AddErrorMessage("You failed to send a 'watchdays' parameter.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                return;
            }

            List<WatchDay> watchDaysFromClient;
            try
            {
                watchDaysFromClient = token.Args["watchdays"].CastJToken<List<WatchDay>>();
            }
            catch
            {
                token.AddErrorMessage("An error occurred while trying to parse your watch days.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                return;
            }

            var watchDaysToInsert = watchDaysFromClient.Select(x => new WatchDay
            {
                Date = x.Date,
                Id = Guid.NewGuid(),
                Remarks = x.Remarks,
                Watchbill = x.Watchbill
            }).ToList();

            var validationResults = watchDaysToInsert.Select(x => new WatchDay.WatchDayValidator().Validate(x)).ToList();

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
                        foreach (var day in watchDaysToInsert)
                        {
                            //Let's get the watchbill the client says this watch day will be assigned to.
                            var watchbill = session.Get<Entities.Watchbill.Watchbill>(day.Watchbill.Id);

                            if (watchbill == null)
                            {
                                token.AddErrorMessage("Your watchbill's id was not valid.  Please consider creating the watchbill first.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
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
            //Just make sure the client is logged in.  The endpoint's description should've handled this but you never know.
            if (token.AuthenticationSession == null)
            {
                token.AddErrorMessage("You must be logged in to do that.", ErrorTypes.Authentication, System.Net.HttpStatusCode.Unauthorized);
                return;
            }

            if (!token.Args.ContainsKey("watchday"))
            {
                token.AddErrorMessage("You failed to send a 'watchday' parameter.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                return;
            }

            WatchDay watchDayFromClient;
            try
            {
                watchDayFromClient = token.Args["watchday"].CastJToken<WatchDay>();
            }
            catch
            {
                token.AddErrorMessage("An error occurred while trying to parse your watch day.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                return;
            }

            var valdiationResult = new WatchDay.WatchDayValidator().Validate(watchDayFromClient);

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
                        var watchDayFromDB = session.Get<WatchDay>(watchDayFromClient.Id);

                        if (watchDayFromDB == null)
                        {
                            token.AddErrorMessage("Your watch day's id was not valid.  Please consider creating the watch day first.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                            return;
                        }

                        if (!token.AuthenticationSession.Person.PermissionGroups.Any(x => x.ChainsOfCommandMemberOf.Contains(watchDayFromDB.Watchbill.ElligibilityGroup.OwningChainOfCommand) && x.AccessLevel == ChainOfCommandLevels.Command))
                        {
                            token.AddErrorMessage("You are not allowed to edit the structure of a watchbill tied to that elligibility group.  You must have command level permissions in the related chain of command.", ErrorTypes.Authorization, System.Net.HttpStatusCode.Unauthorized);
                            return;
                        }

                        //Ok let's swap the properties now.
                        //Check the state.
                        if (watchDayFromDB.Watchbill.CurrentState != Entities.ReferenceLists.Watchbill.WatchbillStatuses.Initial && watchDayFromClient.Date != watchDayFromDB.Date)
                        {
                            token.AddErrorMessage("You may not edit the structure of a watchbill that is not in the initial state.  Please consider changing its state first.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                            return;
                        }

                        watchDayFromDB.Date = watchDayFromClient.Date;
                        watchDayFromDB.Remarks = watchDayFromClient.Remarks;

                        //Let's also make sure that the updates to this watchbill didn't result in a validation failure.
                        var watchbillValidationResult = new Entities.Watchbill.Watchbill.WatchbillValidator().Validate(watchDayFromDB.Watchbill);

                        if (!watchbillValidationResult.IsValid)
                        {
                            token.AddErrorMessages(watchbillValidationResult.Errors.Select(x => x.ErrorMessage), ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                            return;
                        }

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
            //Just make sure the client is logged in.  The endpoint's description should've handled this but you never know.
            if (token.AuthenticationSession == null)
            {
                token.AddErrorMessage("You must be logged in to do that.", ErrorTypes.Authentication, System.Net.HttpStatusCode.Unauthorized);
                return;
            }

            if (!token.Args.ContainsKey("watchday"))
            {
                token.AddErrorMessage("You failed to send a 'watchday' parameter.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                return;
            }

            WatchDay watchDayFromClient;
            try
            {
                watchDayFromClient = token.Args["watchday"].CastJToken<WatchDay>();
            }
            catch
            {
                token.AddErrorMessage("An error occurred while trying to parse your watch day.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                return;
            }

            using (var session = DataAccess.NHibernateHelper.CreateStatefulSession())
            {
                using (var transaction = session.BeginTransaction())
                {
                    try
                    {
                        var watchDayFromDB = session.Get<WatchDay>(watchDayFromClient.Id);

                        if (watchDayFromDB == null)
                        {
                            token.AddErrorMessage("Your watch day's id was not valid.  Please consider creating the watch day first.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                            return;
                        }

                        if (!token.AuthenticationSession.Person.PermissionGroups.Any(x => x.ChainsOfCommandMemberOf.Contains(watchDayFromDB.Watchbill.ElligibilityGroup.OwningChainOfCommand) && x.AccessLevel == ChainOfCommandLevels.Command))
                        {
                            token.AddErrorMessage("You are not allowed to edit the structure of a watchbill tied to that elligibility group.  You must have command level permissions in the related chain of command.", ErrorTypes.Authorization, System.Net.HttpStatusCode.Unauthorized);
                            return;
                        }

                        //Check the state.
                        if (watchDayFromDB.Watchbill.CurrentState != Entities.ReferenceLists.Watchbill.WatchbillStatuses.Initial)
                        {
                            token.AddErrorMessage("You may not edit the structure of a watchbill that is not in the initial state.  Please consider changing its state first.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                            return;
                        }

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
