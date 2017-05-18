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
        /// <param name="dto"></param>
        /// <returns></returns>
        [EndpointMethod(AllowArgumentLogging = true, AllowResponseLogging = true, RequiresAuthentication = true)]
        private static void EditWatchEligibilityGroupMembership(MessageToken token, DTOs.Watchbill.WatchEligibilityGroupEndpoints.EditWatchEligibilityGroupMembership dto)
        {
            token.AssertLoggedIn();

            using (var session = DataAccess.NHibernateHelper.CreateStatefulSession())
            {
                using (var transaction = session.BeginTransaction())
                {
                    try
                    {
                        var groupFromDB = session.Get<WatchEligibilityGroup>(dto.Id) ??
                            throw new CommandCentralException("Your eligibility group's id was not valid.", ErrorTypes.Validation);

                        //Let's make sure that all of them are real people.
                        var persons = session.QueryOver<Entities.Person>()
                            .WhereRestrictionOn(x => x.Id)
                            .IsIn(dto.PersonIds.ToArray())
                            .List();

                        if (persons.Count != dto.PersonIds.Count)
                            throw new CommandCentralException("One or more of the persons in your eligibility group were not valid.", ErrorTypes.Validation);

                        //Now we just need to make sure the client is in the command level of the group's chain of command.
                        if (!token.AuthenticationSession.Person.PermissionGroups.Any(x => x.ChainsOfCommandMemberOf.Contains(groupFromDB.OwningChainOfCommand) && x.AccessLevel == ChainOfCommandLevels.Command))
                            throw new CommandCentralException("You are not allowed to edit the membership of this group.  " +
                                "You must be in the same chain of command as the group and be at the command level.", ErrorTypes.Authorization);

                        //Now we need to add or remove all the people.
                        //This method will cause a pretty big batch update to occur on the database but that's ok.
                        //We also don't allow people whose duty status is set to loss.  We don't want to fail though; instead,
                        //We're just going to send a list back to the client of all those people we failed to add.
                        List<Entities.Person> failures = new List<Entities.Person>();
                        groupFromDB.EligiblePersons.Clear();
                        foreach (var person in persons)
                        {
                            if (person.DutyStatus == Entities.ReferenceLists.DutyStatuses.Loss)
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
