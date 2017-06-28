using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandCentral.Authorization;

namespace CommandCentral.ClientAccess.Endpoints.Watchbill
{
    static class WatchInputRequirementEndpoints
    {

        /// <summary>
        /// WARNING!  THIS METHOD IS EXPOSED TO THE CLIENT AND IS NOT INTENDED FOR INTERNAL USE.  AUTHENTICATION, AUTHORIZATION AND VALIDATION MUST BE HANDLED PRIOR TO DB INTERACTION.
        /// <para />
        /// Loads all those input requirements for which the client is responsible.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        [EndpointMethod(AllowArgumentLogging = true, AllowResponseLogging = true, RequiresAuthentication = true)]
        private static void LoadOpenInputRequirements(MessageToken token)
        {
            using (var session = DataAccess.DataProvider.CreateStatefulSession())
            {
                using (var transaction = session.BeginTransaction())
                {
                    try
                    {
                        var watchbills = session.QueryOver<Entities.Watchbill.Watchbill>()
                            .Where(x => x.CurrentState.Id == Entities.ReferenceLists.ReferenceListHelper<Entities.ReferenceLists.Watchbill.WatchbillStatus>.Find("OpenForInputs").Id)
                            .List();

                        var final = watchbills.Select(watchbill =>
                        {
                            return new
                            {
                                Watchbill = new
                                {
                                    watchbill.Title,
                                    watchbill.CurrentState,
                                    EligibilityGroup = new
                                    {
                                        watchbill.EligibilityGroup.Id,
                                        watchbill.EligibilityGroup.OwningChainOfCommand,
                                        watchbill.EligibilityGroup.Value,
                                        watchbill.EligibilityGroup.Description
                                    }
                                },
                                InputRequirements = watchbill.GetInputRequirementsPersonIsResponsibleFor(token.AuthenticationSession.Person)
                            };
                        });

                        token.SetResult(final);

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
        private static void AnswerInputRequirement(MessageToken token)
        {
            token.AssertLoggedIn();
            token.Args.AssertContainsKeys("watchbillid", "personid");

            if (!Guid.TryParse(token.Args["watchbillid"] as string, out Guid watchbillId))
                throw new CommandCentralException("Your id was not in the right format.", ErrorTypes.Validation);

            if (!Guid.TryParse(token.Args["personid"] as string, out Guid personId))
                throw new CommandCentralException("Your id was not in the right format.", ErrorTypes.Validation);

            //Now let's go get the watchbill from the database.
            using (var session = DataAccess.DataProvider.CreateStatefulSession())
            {
                using (var transaction = session.BeginTransaction())
                {
                    try
                    {
                        var watchbillFromDB = session.Get<Entities.Watchbill.Watchbill>(watchbillId) ??
                            throw new CommandCentralException("Your watchbill's id was not valid.  Please consider creating the watchbill first.", ErrorTypes.Validation);

                        var personFromDB = session.Get<Entities.Person>(personId) ??
                            throw new CommandCentralException("Your person id was not valid.", ErrorTypes.Validation);

                        //Now let's confirm that our client is allowed to submit inputs for this person.
                        var resolvedPermissions = token.AuthenticationSession.Person.ResolvePermissions(personFromDB);

                        if (resolvedPermissions.HighestLevels[watchbillFromDB.EligibilityGroup.OwningChainOfCommand] == ChainOfCommandLevels.Self ||
                             resolvedPermissions.HighestLevels[watchbillFromDB.EligibilityGroup.OwningChainOfCommand] == ChainOfCommandLevels.None)
                            throw new CommandCentralException("You are not authorized to confirm your own watch inputs.  Only watchbill coordinators can do that.", ErrorTypes.Authorization);

                        //I include a check to see if the client is the person with the watch input to allow it to pass.
                        if (!resolvedPermissions.IsInChainOfCommand[watchbillFromDB.EligibilityGroup.OwningChainOfCommand])
                            throw new CommandCentralException("You are not authorized to confirm inputs for this person.", ErrorTypes.Authorization);

                        var inputRequirement = watchbillFromDB.InputRequirements.FirstOrDefault(x => x.Person.Id == personFromDB.Id) ??
                            throw new CommandCentralException("You may not submit inputs for the person because there is no valid input requirement for that person.", ErrorTypes.Validation);

                        inputRequirement.IsAnswered = true;

                        session.Update(watchbillFromDB);

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
