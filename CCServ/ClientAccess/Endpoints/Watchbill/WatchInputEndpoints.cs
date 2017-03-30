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
                throw new CommandCentralException("You must be logged in to do that.", ErrorTypes.Authentication, System.Net.HttpStatusCode.Unauthorized);
                return;
            }

            if (!token.Args.ContainsKey("watchinputid"))
            {
                throw new CommandCentralException("You failed to send a 'watchinputid' parameter.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                return;
            }

            Guid watchInputId;
            if (!Guid.TryParse(token.Args["watchinputid"] as string, out watchInputId))
            {
                throw new CommandCentralException("Your watch input id parameter's format was invalid.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
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
                            throw new CommandCentralException("Your watch input's id was not valid.  Please consider creating the watch input first.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
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
                throw new CommandCentralException("You must be logged in to do that.", ErrorTypes.Authentication, System.Net.HttpStatusCode.Unauthorized);
                return;
            }

            if (!token.Args.ContainsKey("watchinputs"))
            {
                throw new CommandCentralException("You failed to send a 'watchinputs' parameter.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                return;
            }

            List<WatchInput> watchInputsFromClient;
            try
            {
                watchInputsFromClient = token.Args["watchinputs"].CastJToken<List<WatchInput>>();
            }
            catch
            {
                throw new CommandCentralException("An error occurred while trying to parse your watch inputs.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
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
                throw new CommandCentralExceptions(validationResults.SelectMany(x => x.Errors).Select(x => x.ErrorMessage), ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
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
                                throw new CommandCentralException("The person Id given by an input was not valid.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                                return;
                            }

                            //Now let's confirm that our client is allowed to submit inputs for this person.
                            var resolvedPermissions = token.AuthenticationSession.Person.PermissionGroups
                                .Resolve(token.AuthenticationSession.Person, personFromDB);

                            if (!resolvedPermissions.ChainOfCommandByModule[ChainsOfCommand.QuarterdeckWatchbill.ToString()]
                                && resolvedPermissions.PersonId != resolvedPermissions.ClientId)
                            {
                                throw new CommandCentralException("You are not authorized to submit inputs for this person.", ErrorTypes.Authorization, System.Net.HttpStatusCode.Unauthorized);
                                return;
                            }

                            //Now we just have to be certain that the watch shifts are all real and from the same watchbill.
                            var watchShiftsFromDB = session.QueryOver<WatchShift>()
                                .WhereRestrictionOn(x => x.Id)
                                .IsIn(input.WatchShifts.Select(x => x.Id).Cast<object>().ToArray())
                                .List();

                            if (watchShiftsFromDB.Count != input.WatchShifts.Count)
                            {
                                throw new CommandCentralException("One or more of your watch shifts' Ids were invalid.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                                return;
                            }

                            //Now we just need to walk the watchbill references.
                            var watchbill = watchShiftsFromDB.First().WatchDays.First().Watchbill;
                            if (watchShiftsFromDB.Any(x => x.WatchDays.First().Watchbill.Id != watchbill.Id))
                            {
                                throw new CommandCentralException("Your requested watch inputs were not all for the same watchbill.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                                return;
                            }

                            //Let's also check the watchbill's state.
                            if (watchbill.CurrentState != Entities.ReferenceLists.Watchbill.WatchbillStatuses.OpenForInputs)
                            {
                                throw new CommandCentralException("You may not submit inputs unless the watchbill is in the Open for Inputs state.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                                return;
                            }

                            foreach (var shift in watchShiftsFromDB)
                            {
                                shift.WatchInputs.Add(input);
                            }

                            session.Update(watchbill);
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
        /// Updates a watch input.  This is how clients can confirm inputs, for example.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        [EndpointMethod(AllowArgumentLogging = true, AllowResponseLogging = true, RequiresAuthentication = true)]
        private static void UpdateWatchInput(MessageToken token)
        {
            //Just make sure the client is logged in.  The endpoint's description should've handled this but you never know.
            if (token.AuthenticationSession == null)
            {
                throw new CommandCentralException("You must be logged in to do that.", ErrorTypes.Authentication, System.Net.HttpStatusCode.Unauthorized);
                return;
            }

            if (!token.Args.ContainsKey("watchinput"))
            {
                throw new CommandCentralException("You failed to send a 'watchinput' parameter.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                return;
            }

            WatchInput watchInputFromClient;
            try
            {
                watchInputFromClient = token.Args["watchinput"].CastJToken<WatchInput>();
            }
            catch
            {
                throw new CommandCentralException("An error occurred while trying to parse your watch input.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                return;
            }

            var valdiationResult = new WatchInput.WatchInputValidator().Validate(watchInputFromClient);

            if (!valdiationResult.IsValid)
            {
                throw new CommandCentralExceptions(valdiationResult.Errors.Select(x => x.ErrorMessage), ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                return;
            }

            using (var session = DataAccess.NHibernateHelper.CreateStatefulSession())
            {
                using (var transaction = session.BeginTransaction())
                {
                    try
                    {
                        var watchInputFromDB = session.Get<WatchInput>(watchInputFromClient.Id);

                        if (watchInputFromDB == null)
                        {
                            throw new CommandCentralException("Your watch input's id was not valid.  Please consider creating the watch input first.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                            return;
                        }

                        var watchbill = watchInputFromDB.WatchShifts.First().WatchDays.First().Watchbill;

                        //Ok let's swap the properties now.
                        //Check the state.
                        if (watchbill.CurrentState != Entities.ReferenceLists.Watchbill.WatchbillStatuses.OpenForInputs)
                        {
                            throw new CommandCentralException("You may not edit inputs unless the watchbill is in the Open for Inputs state.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                            return;
                        }

                        //Now let's confirm that our client is allowed to submit inputs for this person.
                        var resolvedPermissions = token.AuthenticationSession.Person.PermissionGroups
                            .Resolve(token.AuthenticationSession.Person, watchInputFromDB.Person);

                        if (!resolvedPermissions.ChainOfCommandByModule[ChainsOfCommand.QuarterdeckWatchbill.ToString()])
                        {
                            throw new CommandCentralException("You are not authorized to edit inputs for this person.  " +
                                "If this is your own input and you need to change the date range, " +
                                "please delete the input and then re-create it for the proper range.",
                                ErrorTypes.Authorization, System.Net.HttpStatusCode.Unauthorized);
                            return;
                        }

                        //The client is looking to confirm the watch input.
                        if (!watchInputFromDB.IsConfirmed && watchInputFromClient.IsConfirmed)
                        {
                            watchInputFromDB.IsConfirmed = true;
                            watchInputFromDB.DateConfirmed = token.CallTime;
                            watchInputFromDB.ConfirmedBy = token.AuthenticationSession.Person;
                        }

                        session.Update(watchInputFromDB);

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
        /// Deletes a watch shift.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        [EndpointMethod(AllowArgumentLogging = true, AllowResponseLogging = true, RequiresAuthentication = true)]
        private static void DeleteWatchInput(MessageToken token)
        {

            //Just make sure the client is logged in.  The endpoint's description should've handled this but you never know.
            if (token.AuthenticationSession == null)
            {
                throw new CommandCentralException("You must be logged in to do that.", ErrorTypes.Authentication, System.Net.HttpStatusCode.Unauthorized);
                return;
            }

            if (!token.Args.ContainsKey("watchinput"))
            {
                throw new CommandCentralException("You failed to send a 'watchinput' parameter.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                return;
            }

            WatchInput watchInputFromClient;
            try
            {
                watchInputFromClient = token.Args["watchinput"].CastJToken<WatchInput>();
            }
            catch
            {
                throw new CommandCentralException("An error occurred while trying to parse your watch input.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                return;
            }

            using (var session = DataAccess.NHibernateHelper.CreateStatefulSession())
            {
                using (var transaction = session.BeginTransaction())
                {
                    try
                    {
                        var watchInputFromDB = session.Get<WatchInput>(watchInputFromClient.Id);

                        if (watchInputFromDB == null)
                        {
                            throw new CommandCentralException("Your watch input's id was not valid.  Please consider creating the watch input first.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                            return;
                        }

                        var watchbill = watchInputFromDB.WatchShifts.First().WatchDays.First().Watchbill;

                        //Check the state.
                        if (watchbill.CurrentState != Entities.ReferenceLists.Watchbill.WatchbillStatuses.OpenForInputs)
                        {
                            throw new CommandCentralException("You may not edit inputs unless the watchbill is in the Open for Inputs state.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                            return;
                        }

                        //Now we also need the permissions to determine if this client can edit this input.
                        var resolvedPermissions = token.AuthenticationSession.Person.PermissionGroups
                            .Resolve(token.AuthenticationSession.Person, watchInputFromDB.Person);

                        if (!resolvedPermissions.ChainOfCommandByModule[ChainsOfCommand.QuarterdeckWatchbill.ToString()]
                            && watchInputFromDB.Person.Id != token.AuthenticationSession.Person.Id
                            && watchInputFromDB.SubmittedBy.Id != token.AuthenticationSession.Person.Id)
                        {
                            throw new CommandCentralException("You are not authorized to edit inputs for this person.  " +
                                "If this is your own input and you need to change the date range, " +
                                "please delete the input and then re-create it for the proper range.",
                                ErrorTypes.Authorization, System.Net.HttpStatusCode.Unauthorized);
                            return;
                        }

                        session.Delete(watchInputFromDB);

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
