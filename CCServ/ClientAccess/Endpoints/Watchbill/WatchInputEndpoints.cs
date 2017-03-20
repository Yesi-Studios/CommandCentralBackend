using CCServ.Entities.Watchbill;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AtwoodUtils;
using CCServ.Authorization;

namespace CCServ.ClientAccess.Endpoints.Watchbill
{
    /// <summary>
    /// Contains all the endpoints for interacting with watch inputs.
    /// </summary>
    static class WatchInputEndpoints
    {

        /// <summary>
        /// WARNING!  THIS METHOD IS EXPOSED TO THE CLIENT AND IS NOT INTENDED FOR INTERNAL USE.  AUTHENTICATION, AUTHORIZATION AND VALIDATION MUST BE HANDLED PRIOR TO DB INTERACTION.
        /// <para />
        /// Loads a watch input.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        [EndpointMethod(AllowArgumentLogging = true, AllowResponseLogging = true, RequiresAuthentication = true)]
        private static void LoadWatchInput(MessageToken token)
        {
            //Just make sure the client is logged in.  The endpoint's description should've handled this but you never know.
            if (token.AuthenticationSession == null)
            {
                token.AddErrorMessage("You must be logged in to do that.", ErrorTypes.Authentication, System.Net.HttpStatusCode.Unauthorized);
                return;
            }

            if (!token.Args.ContainsKey("watchinputid"))
            {
                token.AddErrorMessage("You failed to send a 'watchinputid' parameter.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                return;
            }

            Guid watchInputId;
            if (!Guid.TryParse(token.Args["watchinputid"] as string, out watchInputId))
            {
                token.AddErrorMessage("Your watch input id parameter's format was invalid.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                return;
            }

            using (var session = DataAccess.NHibernateHelper.CreateStatefulSession())
            {
                using (var transaction = session.BeginTransaction())
                {
                    try
                    {
                        var watchInputFromDB = session.Get<WatchInput>(watchInputId);

                        if (watchInputFromDB == null)
                        {
                            token.AddErrorMessage("Your watch input's id was not valid.  Please consider creating the watch input first.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                            return;
                        }

                        token.SetResult(watchInputFromDB);

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
        /// Creates multiple watch inputs.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        [EndpointMethod(AllowArgumentLogging = true, AllowResponseLogging = true, RequiresAuthentication = true)]
        private static void CreateWatchInputs(MessageToken token)
        {
            //Just make sure the client is logged in.  The endpoint's description should've handled this but you never know.
            if (token.AuthenticationSession == null)
            {
                token.AddErrorMessage("You must be logged in to do that.", ErrorTypes.Authentication, System.Net.HttpStatusCode.Unauthorized);
                return;
            }

            if (!token.Args.ContainsKey("watchinputs"))
            {
                token.AddErrorMessage("You failed to send a 'watchinputs' parameter.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                return;
            }

            List<WatchInput> watchInputsFromClient;
            try
            {
                watchInputsFromClient = token.Args["watchinputs"].CastJToken<List<WatchInput>>();
            }
            catch
            {
                token.AddErrorMessage("An error occurred while trying to parse your watch inputs.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                return;
            }

            var watchInputsToInsert = watchInputsFromClient.Select(x => new WatchInput
            {
                Id = Guid.NewGuid(),
                DateSubmitted = token.CallTime,
                InputReason = x.InputReason,
                Person = x.Person,
                SubmittedBy = token.AuthenticationSession.Person,
                WatchShifts = x.WatchShifts
            }).ToList();

            var validationResults = watchInputsToInsert.Select(x => new WatchInput.WatchInputValidator().Validate(x)).ToList();

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
                        foreach (var input in watchInputsToInsert)
                        {
                            //First, let's get the person the client is talking about.
                            var personFromDB = session.Get<Entities.Person>(input.Person.Id);
                            if (personFromDB == null)
                            {
                                token.AddErrorMessage("The person Id given by an input was not valid.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                                return;
                            }

                            //Now let's confirm that our client is allowed to submit inputs for this person.
                            var resolvedPermissions = token.AuthenticationSession.Person.PermissionGroups
                                .Resolve(token.AuthenticationSession.Person, personFromDB);

                            if (!resolvedPermissions.ChainOfCommandByModule[ChainsOfCommand.QuarterdeckWatchbill.ToString()]
                                && resolvedPermissions.PersonId != resolvedPermissions.ClientId)
                            {
                                token.AddErrorMessage("You are not authorized to submit inputs for this person.", ErrorTypes.Authorization, System.Net.HttpStatusCode.Unauthorized);
                                return;
                            }

                            //Now we just have to be certain that the watch shifts are all real and from the same watchbill.
                            var watchShiftsFromDB = session.QueryOver<WatchShift>()
                                .WhereRestrictionOn(x => x.Id)
                                .IsIn(input.WatchShifts.Select(x => x.Id).Cast<object>().ToArray())
                                .List();

                            if (watchShiftsFromDB.Count != input.WatchShifts.Count)
                            {
                                token.AddErrorMessage("One or more of your watch shifts' Ids were invalid.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                                return;
                            }

                            //Now we just need to walk the watchbill references.
                            var watchbill = watchShiftsFromDB.First().WatchDays.First().Watchbill;
                            if (watchShiftsFromDB.Any(x => x.WatchDays.First().Watchbill.Id != watchbill.Id))
                            {
                                token.AddErrorMessage("Your requested watch inputs were not all for the same watchbill.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                                return;
                            }

                            session.Save(input);
                        }

                        token.SetResult(watchInputsToInsert);

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
    }
}
