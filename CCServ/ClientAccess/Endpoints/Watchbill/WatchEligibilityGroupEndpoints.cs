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
    /// The endpoints for interacting with the elligibility groups.
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

            //Just make sure the client is logged in.  The endpoint's description should've handled this but you never know.
            if (token.AuthenticationSession == null)
            {
                token.AddErrorMessage("You must be logged in to do that.", ErrorTypes.Authentication, System.Net.HttpStatusCode.Unauthorized);
                return;
            }

            if (!token.Args.ContainsKey("eligibilitygroup"))
            {
                token.AddErrorMessage("You failed to send a 'eligibilitygroup' parameter.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                return;
            }

            WatchElligibilityGroup groupFromClient;
            try
            {
                groupFromClient = token.Args["eligibilitygroup"].CastJToken<WatchElligibilityGroup>();
            }
            catch
            {
                token.AddErrorMessage("There was an issue while parsing your eligibility group.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                return;
            }

            var validationResults = new WatchElligibilityGroup.WatchElligibilityGroupValidator().Validate(groupFromClient);

            if (!validationResults.IsValid)
            {
                token.AddErrorMessages(validationResults.Errors.Select(x => x.ErrorMessage), ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                return;
            }

            using (var session = DataAccess.NHibernateHelper.CreateStatefulSession())
            {
                using (var transaction = session.BeginTransaction())
                {
                    try
                    {
                        var groupFromDB = session.Get<WatchElligibilityGroup>(groupFromClient.Id);

                        if (groupFromDB == null)
                        {
                            token.AddErrorMessage("Your eligibility group's id was not valid.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                            return;
                        }

                        //Let's make sure that all of them are real people.
                        var persons = session.QueryOver<Entities.Person>()
                            .WhereRestrictionOn(x => x.Id)
                            .IsIn(groupFromClient.ElligiblePersons.Select(x => (object)x.Id).ToArray())
                            .List();

                        if (persons.Count != groupFromClient.ElligiblePersons.Count)
                        {
                            token.AddErrorMessage("One or more of the persons in your eligibility group were not valid..", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                            return;
                        }

                        //Now we just need to make sure the client is in the command level of the group's chain of command.
                        if (!token.AuthenticationSession.Person.PermissionGroups.Any(x => x.ChainsOfCommandMemberOf.Contains(groupFromDB.OwningChainOfCommand) && x.AccessLevel == ChainOfCommandLevels.Command))
                        {
                            token.AddErrorMessage("You are not allowed to edit the membership of this group.  You must be in the same chain of command as the group and be at the command level.", ErrorTypes.Authorization, System.Net.HttpStatusCode.Unauthorized);
                            return;
                        }

                        //Now we need to add or remove all the people.
                        //This method will cause a pretty big batch update to occur on the database but that's ok.
                        groupFromDB.ElligiblePersons.Clear();
                        foreach (var person in persons)
                        {
                            groupFromDB.ElligiblePersons.Add(person);
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
