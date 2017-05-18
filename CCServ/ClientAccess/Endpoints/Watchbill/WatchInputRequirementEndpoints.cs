﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CCServ.Authorization;

namespace CCServ.ClientAccess.Endpoints.Watchbill
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
        private static void LoadInputRequirementsResponsibleFor(MessageToken token)
        {
            token.AssertLoggedIn();
            token.Args.AssertContainsKeys("watchbillid");

            if (!Guid.TryParse(token.Args["watchbillid"] as string, out Guid watchbillId))
                throw new CommandCentralException("Your watchbill id was in the wrong format.", ErrorTypes.Validation);

            using (var session = DataAccess.NHibernateHelper.CreateStatefulSession())
            {
                using (var transaction = session.BeginTransaction())
                {
                    try
                    {

                        var watchbill = session.Get<Entities.Watchbill.Watchbill>(watchbillId) ??
                            throw new CommandCentralException("Your watchbill id was not valid.", ErrorTypes.Validation);

                        var resolvedPermissions = token.AuthenticationSession.Person.PermissionGroups.Resolve(token.AuthenticationSession.Person, null);

                        var highestLevelForWatchbill = resolvedPermissions.HighestLevels[watchbill.EligibilityGroup.OwningChainOfCommand.ToString()];

                        IEnumerable<Entities.Watchbill.WatchInputRequirement> inputRequirements;

                        switch (highestLevelForWatchbill)
                        {
                            case ChainOfCommandLevels.Command:
                                {
                                    inputRequirements = watchbill.InputRequirements.Where(x => x.Person.IsInSameCommandAs(token.AuthenticationSession.Person));

                                    break;
                                }
                            case ChainOfCommandLevels.Department:
                                {
                                    inputRequirements = watchbill.InputRequirements.Where(x => x.Person.IsInSameDepartmentAs(token.AuthenticationSession.Person));

                                    break;
                                }
                            case ChainOfCommandLevels.Division:
                                {
                                    inputRequirements = watchbill.InputRequirements.Where(x => x.Person.IsInSameDivisionAs(token.AuthenticationSession.Person));

                                    break;
                                }
                            case ChainOfCommandLevels.Self:
                                {
                                    inputRequirements = watchbill.InputRequirements.Where(x => x.Person.Id == token.AuthenticationSession.Person.Id);

                                    break;
                                }
                            case ChainOfCommandLevels.None:
                                {
                                    inputRequirements = new List<Entities.Watchbill.WatchInputRequirement>();

                                    break;
                                }
                            default:
                                {
                                    throw new NotImplementedException("Fell to the default case in the chain of command switch of the LoadInputRequirementsResponsibleFor endpoint.");
                                }
                        }

                        token.SetResult(new { InputRequirements = inputRequirements });

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
            using (var session = DataAccess.NHibernateHelper.CreateStatefulSession())
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
                        var resolvedPermissions = token.AuthenticationSession.Person.PermissionGroups
                            .Resolve(token.AuthenticationSession.Person, personFromDB);

                        if (!resolvedPermissions.ChainOfCommandByModule[watchbillFromDB.EligibilityGroup.OwningChainOfCommand.ToString()]
                                && resolvedPermissions.PersonId != resolvedPermissions.ClientId)
                            throw new CommandCentralException("You are not authorized to submit inputs for this person.", ErrorTypes.Validation);

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
