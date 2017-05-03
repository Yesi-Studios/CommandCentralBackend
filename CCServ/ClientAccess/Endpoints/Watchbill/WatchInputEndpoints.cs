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
            token.AssertLoggedIn();
            token.Args.AssertContainsKeys("watchinputid");

            if (!Guid.TryParse(token.Args["watchinputid"] as string, out Guid watchInputId))
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
        /// Creates multiple watch inputs.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        [EndpointMethod(AllowArgumentLogging = true, AllowResponseLogging = true, RequiresAuthentication = true)]
        private static void CreateWatchInputs(MessageToken token)
        {
            token.AssertLoggedIn();
            token.Args.AssertContainsKeys("watchinputs");

            List<WatchInput> watchInputsFromClient;
            try
            {
                watchInputsFromClient = token.Args["watchinputs"].CastJToken<List<WatchInput>>();
            }
            catch
            {
                throw new CommandCentralException("An error occurred while trying to parse your watch inputs.", ErrorTypes.Validation);
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
            var invalidResults = validationResults.Where(x => !x.IsValid);
            if (invalidResults.Any())
                throw new AggregateException(invalidResults.SelectMany(x => x.Errors.Select(y => new CommandCentralException(y.ErrorMessage, ErrorTypes.Validation))));

            using (var session = DataAccess.NHibernateHelper.CreateStatefulSession())
            {
                using (var transaction = session.BeginTransaction())
                {
                    try
                    {
                        foreach (var input in watchInputsToInsert)
                        {
                            //First, let's get the person the client is talking about.
                            var personFromDB = session.Get<Entities.Person>(input.Person.Id) ??
                                throw new CommandCentralException("The person Id given by an input was not valid.", ErrorTypes.Validation);

                            //Now let's confirm that our client is allowed to submit inputs for this person.
                            var resolvedPermissions = token.AuthenticationSession.Person.PermissionGroups
                                .Resolve(token.AuthenticationSession.Person, personFromDB);

                            //Now we just have to be certain that the watch shifts are all real and from the same watchbill.
                            var watchShiftsFromDB = session.QueryOver<WatchShift>()
                                .WhereRestrictionOn(x => x.Id)
                                .IsIn(input.WatchShifts.Select(x => x.Id).Cast<object>().ToArray())
                                .List();

                            if (watchShiftsFromDB.Count != input.WatchShifts.Count)
                                throw new CommandCentralException("One or more of your watch shifts' Ids were invalid.", ErrorTypes.Validation);

                            //Now we just need to walk the watchbill references.
                            var watchbill = watchShiftsFromDB.First().WatchDays.First().Watchbill;
                            if (watchShiftsFromDB.Any(x => x.WatchDays.First().Watchbill.Id != watchbill.Id))
                                throw new CommandCentralException("Your requested watch inputs were not all for the same watchbill.", ErrorTypes.Validation);

                            if (!resolvedPermissions.ChainOfCommandByModule[watchbill.EligibilityGroup.OwningChainOfCommand.ToString()]
                                && resolvedPermissions.PersonId != resolvedPermissions.ClientId)
                                throw new CommandCentralException("You are not authorized to submit inputs for this person.", ErrorTypes.Validation);

                            //Let's also check the watchbill's state.
                            if (watchbill.CurrentState != Entities.ReferenceLists.Watchbill.WatchbillStatuses.OpenForInputs)
                                throw new CommandCentralException("You may not submit inputs unless the watchbill is in the Open for Inputs state.", ErrorTypes.Validation);

                            //We'll also check to make sure that the watch input reason is real.
                            var inputReasonFromDB = session.Get<Entities.ReferenceLists.Watchbill.WatchInputReason>(input.InputReason.Id) ??
                                throw new CommandCentralException("Your watch input reason was not valid.", ErrorTypes.Validation);

                            input.InputReason = inputReasonFromDB;

                            //We also need to find the input requirement for this person and update that.
                            var inputRequirement = watchbill.InputRequirements.FirstOrDefault(x => x.Person.Id == personFromDB.Id) ??
                                throw new CommandCentralException("You may not submit inputs for the person because there is no valid input requirement for that person.", ErrorTypes.Validation);

                            if (!inputRequirement.IsAnswered)
                                inputRequirement.IsAnswered = true;

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
            token.AssertLoggedIn();
            token.Args.AssertContainsKeys("watchinput");

            var jToken = token.Args["watchinput"].CastJToken();

            if (!Guid.TryParse(jToken.Value<string>("Id"), out Guid id))
                throw new CommandCentralException("Your id was not in the right format.", ErrorTypes.Validation);

            bool isClientConfirmed = jToken.Value<bool>("IsConfirmed");
            
            using (var session = DataAccess.NHibernateHelper.CreateStatefulSession())
            {
                using (var transaction = session.BeginTransaction())
                {
                    try
                    {
                        var watchInputFromDB = session.Get<WatchInput>(id) ??
                            throw new CommandCentralException("Your watch input's id was not valid.  Please consider creating the watch input first.", ErrorTypes.Validation);

                        var watchbill = watchInputFromDB.WatchShifts.First().WatchDays.First().Watchbill;

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

                        //The client is looking to confirm the watch input.
                        if (!watchInputFromDB.IsConfirmed && isClientConfirmed)
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
            token.AssertLoggedIn();
            token.Args.AssertContainsKeys("watchinput");

            if (!Guid.TryParse(token.Args["watchinput"].CastJToken().Value<string>("Id"), out Guid id))
                throw new CommandCentralException("An error occurred while trying to parse your watch input.", ErrorTypes.Validation);

            using (var session = DataAccess.NHibernateHelper.CreateStatefulSession())
            {
                using (var transaction = session.BeginTransaction())
                {
                    try
                    {
                        var watchInputFromDB = session.Get<WatchInput>(id) ??
                            throw new CommandCentralException("Your watch input's id was not valid.  Please consider creating the watch input first.", ErrorTypes.Validation);

                        var watchbill = watchInputFromDB.WatchShifts.First().WatchDays.First().Watchbill;

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
