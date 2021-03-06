﻿using CommandCentral.Entities.Watchbill;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AtwoodUtils;
using CommandCentral.Authorization;
using CommandCentral.Entities.ReferenceLists.Watchbill;
using NHibernate.Criterion;
using NHibernate.Linq;

namespace CommandCentral.ClientAccess.Endpoints.Watchbill
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
            token.AssertLoggedIn();
            token.Args.AssertContainsKeys("id");

            if (!Guid.TryParse(token.Args["id"] as string, out var watchInputId))
                throw new CommandCentralException("Your watch input id parameter's format was invalid.", ErrorTypes.Validation);

            using (var session = DataAccess.DataProvider.CreateStatefulSession())
            {
                using (var transaction = session.BeginTransaction())
                {
                    try
                    {
                        var watchInputFromDB = session.Get<WatchInput>(watchInputId) ??
                            throw new CommandCentralException("Your watch input's id was not valid.  Please consider creating the watch input first.", ErrorTypes.Validation);

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
            token.AssertLoggedIn();
            token.Args.AssertContainsKeys("watchinputs", "watchbillid");

            if (!Guid.TryParse(token.Args["watchbillid"] as string, out var watchbillId))
                throw new CommandCentralException("Your watchbill id was not in the right format.", ErrorTypes.Validation);

            var inputsToken = token.Args["watchinputs"].CastJToken();

            if (inputsToken.Type != Newtonsoft.Json.Linq.JTokenType.Array)
                throw new CommandCentralException("Your inputs were in the wrong format.", ErrorTypes.Validation);

            var inputsFromClient = inputsToken.Select(input =>
            {
                if (!Guid.TryParse(input["InputReason"]?.Value<string>("Id"), out var inputReasonId))
                    throw new CommandCentralException("Your input reason id was in the wrong format.", ErrorTypes.Validation);

                if (!Guid.TryParse(input["person"]?.Value<string>("Id"), out var personId))
                    throw new CommandCentralException("Your person id was in the wrong format.", ErrorTypes.Validation);

                var range = input["Range"].CastJToken<TimeRange>();

                var newInput = new
                {
                    InputReasonId = inputReasonId,
                    PersonId = personId,
                    Range = range
                };

                return newInput;
            });

            using (var session = DataAccess.DataProvider.CreateStatefulSession())
            {
                using (var transaction = session.BeginTransaction())
                {
                    try
                    {
                        //Now we just need to walk the watchbill references.
                        var watchbill = session.Get<Entities.Watchbill.Watchbill>(watchbillId) ??
                            throw new CommandCentralException("Your watchbill id was not valid.", ErrorTypes.Validation);

                        foreach (var input in inputsFromClient)
                        {
                            //First, let's get the person the client is talking about.
                            var personFromDB = session.Get<Entities.Person>(input.PersonId) ??
                                throw new CommandCentralException("The person Id given by an input was not valid.", ErrorTypes.Validation);

                            var reasonFromDB = session.Get<WatchInputReason>(input.InputReasonId) ??
                                throw new CommandCentralException("The reason Id was not valid.", ErrorTypes.Validation);

                            //Now let's confirm that our client is allowed to submit inputs for this person.
                            var resolvedPermissions = token.AuthenticationSession.Person.ResolvePermissions(personFromDB);

                            if (!resolvedPermissions.IsInChainOfCommand[watchbill.EligibilityGroup.OwningChainOfCommand] && personFromDB.Id != token.AuthenticationSession.Person.Id)
                                throw new CommandCentralException("You are not authorized to submit inputs for this person.", ErrorTypes.Validation);
                            
                            //Let's also check the watchbill's state.
                            if (watchbill.CurrentState != Entities.ReferenceLists.ReferenceListHelper<WatchbillStatus>.Find("Open for Inputs"))
                                throw new CommandCentralException("You may not submit inputs unless the watchbill is in the Open for Inputs state.", ErrorTypes.Validation);

                            var inputToInsert = new WatchInput
                            {
                                Id = Guid.NewGuid(),
                                DateSubmitted = token.CallTime,
                                InputReason = reasonFromDB,
                                Person = personFromDB,
                                SubmittedBy = token.AuthenticationSession.Person,
                                Range = input.Range
                            };

                            //We also need to find the input requirement for this person and update that.
                            var inputRequirement = watchbill.InputRequirements.FirstOrDefault(x => x.Person.Id == personFromDB.Id) ??
                                throw new CommandCentralException("You may not submit inputs for the person because there is no valid input requirement for that person.", ErrorTypes.Validation);

                            if (!inputRequirement.IsAnswered)
                                inputRequirement.IsAnswered = true;

                            watchbill.WatchInputs.Add(inputToInsert);

                            session.Update(watchbill);
                        }

                        var validationResult = new Entities.Watchbill.Watchbill.WatchbillValidator().Validate(watchbill);
                        if (!validationResult.IsValid)
                            throw new AggregateException(validationResult.Errors.Select(x => new CommandCentralException(x.ErrorMessage, ErrorTypes.Validation)));

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
        private static void ConfirmWatchInput(MessageToken token)
        {
            token.AssertLoggedIn();
            token.Args.AssertContainsKeys("id");

            if (!Guid.TryParse(token.Args["id"] as string, out var id))
                throw new CommandCentralException("Your id was not in the right format.", ErrorTypes.Validation);

            using (var session = DataAccess.DataProvider.CreateStatefulSession())
            {
                using (var transaction = session.BeginTransaction())
                {
                    try
                    {
                        var watchInputFromDB = session.Get<WatchInput>(id) ??
                            throw new CommandCentralException("Your watch input's id was not valid.  Please consider creating the watch input first.", ErrorTypes.Validation);

                        var watchbill = session.Query<Entities.Watchbill.Watchbill>()
                            .Where(x => x.WatchInputs.Any(y => y.Id == id))
                            .SingleOrDefault() ??
                            throw new Exception("A watch input was not owned by any watchbill.");

                        //Check the state.
                        if (watchbill.CurrentState != Entities.ReferenceLists.ReferenceListHelper<WatchbillStatus>.Find("Open for Inputs"))
                            throw new CommandCentralException("You may not edit inputs unless the watchbill is in the Open for Inputs state.", ErrorTypes.Validation);

                        //Now let's confirm that our client is allowed to submit inputs for this person.
                        var resolvedPermissions = token.AuthenticationSession.Person.ResolvePermissions(watchInputFromDB.Person);

                        if (resolvedPermissions.HighestLevels[watchbill.EligibilityGroup.OwningChainOfCommand] == ChainOfCommandLevels.Self ||
                             resolvedPermissions.HighestLevels[watchbill.EligibilityGroup.OwningChainOfCommand] == ChainOfCommandLevels.None)
                            throw new CommandCentralException("You are not authorized to confirm your own watch inputs.  Only watchbill coordinators can do that.", ErrorTypes.Authorization);

                        //I include a check to see if the client is the person with the watch input to allow it to pass.
                        if (!resolvedPermissions.IsInChainOfCommand[watchbill.EligibilityGroup.OwningChainOfCommand])
                            throw new CommandCentralException("You are not authorized to confirm inputs for this person.", ErrorTypes.Authorization);

                        if (watchInputFromDB.IsConfirmed)
                            return;

                        watchInputFromDB.IsConfirmed = true;
                        watchInputFromDB.DateConfirmed = token.CallTime;
                        watchInputFromDB.ConfirmedBy = token.AuthenticationSession.Person;

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
            token.AssertLoggedIn();
            token.Args.AssertContainsKeys("id");

            if (!Guid.TryParse(token.Args["id"] as string, out var id))
                throw new CommandCentralException("Your id was not in the right format.", ErrorTypes.Validation);

            using (var session = DataAccess.DataProvider.CreateStatefulSession())
            {
                using (var transaction = session.BeginTransaction())
                {
                    try
                    {
                        var watchInputFromDB = session.Get<WatchInput>(id) ??
                            throw new CommandCentralException("Your watch input's id was not valid.  Please consider creating the watch input first.", ErrorTypes.Validation);

                        var watchbill = session.Query<Entities.Watchbill.Watchbill>()
                            .Where(x => x.WatchInputs.Any(y => y.Id == id))
                            .SingleOrDefault() ??
                            throw new Exception("A watch input was not owned by any watchbill.");

                        //Check the state.
                        if (watchbill.CurrentState != Entities.ReferenceLists.ReferenceListHelper<WatchbillStatus>.Find("Open for Inputs"))
                            throw new CommandCentralException("You may not edit inputs unless the watchbill is in the Open for Inputs state.", ErrorTypes.Validation);

                        //Now we also need the permissions to determine if this client can edit this input.
                        var resolvedPermissions = token.AuthenticationSession.Person.ResolvePermissions(watchInputFromDB.Person);

                        if (!resolvedPermissions.IsInChainOfCommand[watchbill.EligibilityGroup.OwningChainOfCommand])
                            throw new CommandCentralException("You are not authorized to edit inputs for this person.", ErrorTypes.Authorization);

                        watchbill.WatchInputs.Remove(watchInputFromDB);

                        //Also, if this watch input is the last watch input for this person, then we need to set the watch input requirement back to not answered.
                        if (!watchbill.WatchInputs.Any(x => x.Person.Id == watchInputFromDB.Person.Id))
                        {
                            var requirement = watchbill.InputRequirements.FirstOrDefault(x => x.Person.Id == watchInputFromDB.Person.Id) ??
                                throw new Exception("How are we trying to deleting a watch input for a person without a requirement? Atwood... what did you do?");

                            requirement.IsAnswered = false;
                        }

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
