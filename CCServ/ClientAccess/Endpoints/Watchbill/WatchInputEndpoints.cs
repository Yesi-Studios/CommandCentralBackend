using CCServ.Entities.Watchbill;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AtwoodUtils;
using CCServ.Authorization;
using CCServ.Entities.ReferenceLists.Watchbill;

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
            token.AssertLoggedIn();
            token.Args.AssertContainsKeys("id");

            if (!Guid.TryParse(token.Args["id"] as string, out Guid watchInputId))
                throw new CommandCentralException("Your watch input id parameter's format was invalid.", ErrorTypes.Validation);

            using (var session = DataAccess.NHibernateHelper.CreateStatefulSession())
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
        /// Loads all watch inputs for a given watchbill.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        [EndpointMethod(AllowArgumentLogging = true, AllowResponseLogging = true, RequiresAuthentication = true)]
        private static void LoadWatchInputs(MessageToken token)
        {
            token.AssertLoggedIn();
            token.Args.AssertContainsKeys("watchbillid");

            if (!Guid.TryParse(token.Args["watchbillid"] as string, out Guid watchbillId))
                throw new CommandCentralException("Your watchnill's id parameter's format was invalid.", ErrorTypes.Validation);

            using (var session = DataAccess.NHibernateHelper.CreateStatefulSession())
            {
                using (var transaction = session.BeginTransaction())
                {
                    try
                    {
                        var watchinputs = (session.Get<Entities.Watchbill.Watchbill>(watchbillId) ??
                            throw new CommandCentralException("Your watchbill id was not valid.", ErrorTypes.Validation))
                            .WatchShifts.SelectMany(x => x.WatchInputs)
                            .Distinct()
                            .Select(watchInput => new
                            {
                                watchInput.Comments,
                                watchInput.ConfirmedBy,
                                watchInput.DateConfirmed,
                                watchInput.DateSubmitted,
                                watchInput.Id,
                                watchInput.InputReason,
                                watchInput.IsConfirmed,
                                watchInput.Person,
                                watchInput.SubmittedBy,
                                WatchShifts = watchInput.WatchShifts.Select(watchShift => new
                                {
                                    watchShift.Comments,
                                    watchShift.Id,
                                    watchShift.Points,
                                    watchShift.Range,
                                    watchShift.ShiftType,
                                    watchShift.Title
                                })
                            });


                        token.SetResult(watchinputs);

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
            token.Args.AssertContainsKeys("watchinputs");

            var inputsToken = token.Args["watchinputs"].CastJToken();

            if (inputsToken.Type != Newtonsoft.Json.Linq.JTokenType.Array)
                throw new CommandCentralException("Your inputs were in the wrong format.", ErrorTypes.Validation);

            var inputsFromClient = inputsToken.Select(input =>
            {
                if (!Guid.TryParse(input["InputReason"]?.Value<string>("Id"), out Guid inputReasonId))
                    throw new CommandCentralException("Your input reason id was in the wrong format.", ErrorTypes.Validation);

                if (!Guid.TryParse(input["person"]?.Value<string>("Id"), out Guid personId))
                    throw new CommandCentralException("Your person id was in the wrong format.", ErrorTypes.Validation);

                var watchShiftIds = new List<Guid>();

                foreach (var shift in input["WatchShifts"])
                {
                    watchShiftIds.Add(Guid.Parse(shift.Value<string>("Id")));
                }

                var newInput = new
                {
                    InputReasonId = inputReasonId,
                    PersonId = personId,
                    WatchShiftIds = watchShiftIds
                };

                return newInput;
            });

            using (var session = DataAccess.NHibernateHelper.CreateStatefulSession())
            {
                using (var transaction = session.BeginTransaction())
                {
                    try
                    {
                        foreach (var input in inputsFromClient)
                        {

                            

                            //First, let's get the person the client is talking about.
                            var personFromDB = session.Get<Entities.Person>(input.PersonId) ??
                                throw new CommandCentralException("The person Id given by an input was not valid.", ErrorTypes.Validation);

                            var reasonFromDB = session.Get<WatchInputReason>(input.InputReasonId) ??
                                throw new CommandCentralException("The reason Id was not valid.", ErrorTypes.Validation);

                            //Now let's confirm that our client is allowed to submit inputs for this person.
                            var resolvedPermissions = token.AuthenticationSession.Person.PermissionGroups
                                .Resolve(token.AuthenticationSession.Person, personFromDB);

                            //Now we just have to be certain that the watch shifts are all real and from the same watchbill.
                            var watchShiftsFromDB = session.QueryOver<WatchShift>()
                                .WhereRestrictionOn(x => x.Id)
                                .IsIn(input.WatchShiftIds.Cast<object>().ToArray())
                                .List();

                            if (watchShiftsFromDB.Count != input.WatchShiftIds.Count)
                                throw new CommandCentralException("One or more of your watch shifts' Ids were invalid.", ErrorTypes.Validation);

                            //Now we just need to walk the watchbill references.
                            var watchbill = watchShiftsFromDB.First().Watchbill;
                            if (watchShiftsFromDB.Any(x => x.Watchbill.Id != watchbill.Id))
                                throw new CommandCentralException("Your requested watch inputs were not all for the same watchbill.", ErrorTypes.Validation);

                            if (!resolvedPermissions.ChainOfCommandByModule[watchbill.EligibilityGroup.OwningChainOfCommand.ToString()]
                                && resolvedPermissions.PersonId != resolvedPermissions.ClientId)
                                throw new CommandCentralException("You are not authorized to submit inputs for this person.", ErrorTypes.Validation);

                            //Let's also check the watchbill's state.
                            if (watchbill.CurrentState != WatchbillStatuses.OpenForInputs)
                                throw new CommandCentralException("You may not submit inputs unless the watchbill is in the Open for Inputs state.", ErrorTypes.Validation);

                            var inputToInsert = new WatchInput
                            {
                                Id = Guid.NewGuid(),
                                DateSubmitted = token.CallTime,
                                InputReason = reasonFromDB,
                                Person = personFromDB,
                                SubmittedBy = token.AuthenticationSession.Person,
                                WatchShifts = watchShiftsFromDB
                            };

                            //We also need to find the input requirement for this person and update that.
                            var inputRequirement = watchbill.InputRequirements.FirstOrDefault(x => x.Person.Id == personFromDB.Id) ??
                                throw new CommandCentralException("You may not submit inputs for the person because there is no valid input requirement for that person.", ErrorTypes.Validation);

                            if (!inputRequirement.IsAnswered)
                                inputRequirement.IsAnswered = true;

                            foreach (var shift in watchShiftsFromDB)
                            {
                                shift.WatchInputs.Add(inputToInsert);
                            }

                            session.Update(watchbill);
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

            if (!Guid.TryParse(token.Args["id"] as string, out Guid id))
                throw new CommandCentralException("Your id was not in the right format.", ErrorTypes.Validation);

            using (var session = DataAccess.NHibernateHelper.CreateStatefulSession())
            {
                using (var transaction = session.BeginTransaction())
                {
                    try
                    {
                        var watchInputFromDB = session.Get<WatchInput>(id) ??
                            throw new CommandCentralException("Your watch input's id was not valid.  Please consider creating the watch input first.", ErrorTypes.Validation);

                        var watchbill = watchInputFromDB.WatchShifts.First().Watchbill;

                        //Ok let's swap the properties now.
                        //Check the state.
                        if (watchbill.CurrentState != Entities.ReferenceLists.Watchbill.WatchbillStatuses.OpenForInputs)
                            throw new CommandCentralException("You may not edit inputs unless the watchbill is in the Open for Inputs state.", ErrorTypes.Validation);

                        //Now let's confirm that our client is allowed to submit inputs for this person.
                        var resolvedPermissions = token.AuthenticationSession.Person.PermissionGroups
                            .Resolve(token.AuthenticationSession.Person, watchInputFromDB.Person);

                        if (!resolvedPermissions.ChainOfCommandByModule[watchbill.EligibilityGroup.OwningChainOfCommand.ToString()])
                            throw new CommandCentralException("You are not authorized to edit inputs for this person.  " +
                                "If this is your own input and you need to change the date range, " +
                                "please delete the input and then re-create it for the proper range.",
                                ErrorTypes.Authorization);

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

            if (!Guid.TryParse(token.Args["id"] as string, out Guid id))
                throw new CommandCentralException("Your id was not in the right format.", ErrorTypes.Validation);

            using (var session = DataAccess.NHibernateHelper.CreateStatefulSession())
            {
                using (var transaction = session.BeginTransaction())
                {
                    try
                    {
                        var watchInputFromDB = session.Get<WatchInput>(id) ??
                            throw new CommandCentralException("Your watch input's id was not valid.  Please consider creating the watch input first.", ErrorTypes.Validation);

                        var watchbill = watchInputFromDB.WatchShifts.First().Watchbill;

                        //Check the state.
                        if (watchbill.CurrentState != Entities.ReferenceLists.Watchbill.WatchbillStatuses.OpenForInputs)
                            throw new CommandCentralException("You may not edit inputs unless the watchbill is in the Open for Inputs state.", ErrorTypes.Validation);

                        //Now we also need the permissions to determine if this client can edit this input.
                        var resolvedPermissions = token.AuthenticationSession.Person.PermissionGroups
                            .Resolve(token.AuthenticationSession.Person, watchInputFromDB.Person);

                        if (!resolvedPermissions.ChainOfCommandByModule[watchbill.EligibilityGroup.OwningChainOfCommand.ToString()]
                            && watchInputFromDB.Person.Id != token.AuthenticationSession.Person.Id
                            && watchInputFromDB.SubmittedBy.Id != token.AuthenticationSession.Person.Id)
                            throw new CommandCentralException("You are not authorized to edit inputs for this person.  " +
                                "If this is your own input and you need to change the date range, " +
                                "please delete the input and then re-create it for the proper range.",
                                ErrorTypes.Authorization);

                        foreach (var shift in watchInputFromDB.WatchShifts)
                        {
                            shift.WatchInputs.Remove(watchInputFromDB);
                            session.Update(shift);
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
