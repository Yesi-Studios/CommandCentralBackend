using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AtwoodUtils;
using CCServ.Entities.ReferenceLists.Watchbill;

namespace CCServ.ClientAccess.Endpoints.Watchbill
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
            token.Args.AssertContainsKeys("eligibilitygroup");

            WatchEligibilityGroup groupFromClient;
            try
            {
                groupFromClient = token.Args["eligibilitygroup"].CastJToken<WatchEligibilityGroup>();
            }
            catch
            {
                throw new CommandCentralException("There was an issue while parsing your eligibility group.", HttpStatusCodes.BadRequest);
            }

            var validationResults = new WatchEligibilityGroup.WatchEligibilityGroupValidator().Validate(groupFromClient);

            if (!validationResults.IsValid)
                throw new AggregateException(validationResults.Errors.Select(x => new CommandCentralException(x.ErrorMessage, HttpStatusCodes.BadRequest)));

            using (var session = DataAccess.NHibernateHelper.CreateStatefulSession())
            {
                using (var transaction = session.BeginTransaction())
                {
                    try
                    {
                        var groupFromDB = session.Get<WatchEligibilityGroup>(groupFromClient.Id) ??
                            throw new CommandCentralException("Your eligibility group's id was not valid.", HttpStatusCodes.BadRequest);

                        //Let's make sure that all of them are real people.
                        var persons = session.QueryOver<Entities.Person>()
                            .WhereRestrictionOn(x => x.Id)
                            .IsIn(groupFromClient.EligiblePersons.Select(x => (object)x.Id).ToArray())
                            .List();

                        if (persons.Count != groupFromClient.EligiblePersons.Count)
                            throw new CommandCentralException("One or more of the persons in your eligibility group were not valid.", HttpStatusCodes.BadRequest);

                        //Now we just need to make sure the client is in the command level of the group's chain of command.
                        if (!token.AuthenticationSession.Person.PermissionGroups.Any(x => x.ChainsOfCommandMemberOf.Contains(groupFromDB.OwningChainOfCommand) && x.AccessLevel == ChainOfCommandLevels.Command))
                            throw new CommandCentralException("You are not allowed to edit the membership of this group.  " +
                                "You must be in the same chain of command as the group and be at the command level.", HttpStatusCodes.Unauthorized);

                        //Now we need to add or remove all the people.
                        //This method will cause a pretty big batch update to occur on the database but that's ok.
                        groupFromDB.EligiblePersons.Clear();
                        foreach (var person in persons)
                        {
                            groupFromDB.EligiblePersons.Add(person);
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
