﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AtwoodUtils;
using CommandCentral.Entities.ReferenceLists.Watchbill;
using CommandCentral.Authorization;
using CommandCentral.Entities.ReferenceLists;

namespace CommandCentral.ClientAccess.Endpoints.Watchbill
{
    /// <summary>
    /// The endpoints for interacting with the eligibility groups.
    /// </summary>
    static class WatchEligibilityGroupEndpoints
    {

        /// <summary>
        /// WARNING!  THIS METHOD IS EXPOSED TO THE CLIENT AND IS NOT INTENDED FOR INTERNAL USE.  AUTHENTICATION, AUTHORIZATION AND VALIDATION MUST BE HANDLED PRIOR TO DB INTERACTION.
        /// <para />
        /// The method responsible for editing the membership of an eligibility group.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        [EndpointMethod(AllowArgumentLogging = true, AllowResponseLogging = true, RequiresAuthentication = true)]
        private static void EditWatchEligibilityGroupMembership(MessageToken token)
        {
            token.AssertLoggedIn();
            token.Args.AssertContainsKeys("id", "personids");

            if (!Guid.TryParse(token.Args["id"] as string, out var elGroupId))
                throw new CommandCentralException("Your id was in the wrong format.", ErrorTypes.Validation);

            var idsToken = token.Args["personids"].CastJToken();

            if (idsToken.Type != Newtonsoft.Json.Linq.JTokenType.Array)
                throw new CommandCentralException("Your ids were not in an array.", ErrorTypes.Validation);

            var ids = idsToken.Select(rawId =>
            {
                if (!Guid.TryParse(rawId.ToString(), out var personId))
                    throw new CommandCentralException("One or more of your person ids were in the wrong format.", ErrorTypes.Validation);

                return personId;
            }).ToList();

            using (var session = DataAccess.DataProvider.CreateStatefulSession())
            {
                using (var transaction = session.BeginTransaction())
                {
                    try
                    {
                        var groupFromDB = session.Get<WatchEligibilityGroup>(elGroupId) ??
                            throw new CommandCentralException("Your eligibility group's id was not valid.", ErrorTypes.Validation);

                        //Let's make sure that all of them are real people.
                        var persons = session.QueryOver<Entities.Person>()
                            .WhereRestrictionOn(x => x.Id)
                            .IsIn(ids.ToArray())
                            .List();

                        if (persons.Count != ids.Count)
                            throw new CommandCentralException("One or more of the persons in your eligibility group were not valid.", ErrorTypes.Validation);

                        //Now we just need to make sure the client is in the command level of the group's chain of command.
                        if(token.AuthenticationSession.Person.ResolvePermissions(null).HighestLevels[groupFromDB.OwningChainOfCommand] != ChainOfCommandLevels.Command)
                            throw new CommandCentralException("You are not allowed to edit this watchbill.  " +
                                "You must have command level permissions in the related chain of command.", ErrorTypes.Authorization);

                        //Now we need to add or remove all the people.
                        //This method will cause a pretty big batch update to occur on the database but that's ok.
                        //We also don't allow people whose duty status is set to loss.  We don't want to fail though; instead,
                        //We're just going to send a list back to the client of all those people we failed to add.
                        var failures = new List<Entities.Person>();
                        groupFromDB.EligiblePersons.Clear();
                        foreach (var person in persons)
                        {
                            if (person.DutyStatus == ReferenceListHelper<DutyStatus>.Find("Loss"))
                                failures.Add(person);
                            else
                                groupFromDB.EligiblePersons.Add(person);
                        }

                        token.SetResult(new { Failures = failures });

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
