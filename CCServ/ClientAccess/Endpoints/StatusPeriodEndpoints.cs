using CCServ.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AtwoodUtils;
using CCServ.Entities.ReferenceLists;
using CCServ.Authorization;

namespace CCServ.ClientAccess.Endpoints
{
    /// <summary>
    /// Contains the feedback endpoints.
    /// </summary>
    static class StatusPeriodEndpoints
    {
        /// <summary>
        /// WARNING!  THIS METHOD IS EXPOSED TO THE CLIENT AND IS NOT INTENDED FOR INTERNAL USE.  AUTHENTICATION, AUTHORIZATION AND VALIDATION MUST BE HANDLED PRIOR TO DB INTERACTION.
        /// <para />
        /// Loads status periods for a given person.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        [EndpointMethod(AllowResponseLogging = true, AllowArgumentLogging = true, RequiresAuthentication = true)]
        private static void LoadStatusPeriodsByPerson(MessageToken token)
        {
            token.AssertLoggedIn();
            token.Args.AssertContainsKeys("personid");

            if (!Guid.TryParse(token.Args["personid"] as string, out Guid personId))
                throw new CommandCentralException("Your person id was not in the right format.", ErrorTypes.Validation);

            using (var session = DataAccess.NHibernateHelper.CreateStatefulSession())
            {
                token.SetResult(session.QueryOver<StatusPeriod>().Where(x => x.Person.Id == personId).List());
            }
        }

        /// <summary>
        /// WARNING!  THIS METHOD IS EXPOSED TO THE CLIENT AND IS NOT INTENDED FOR INTERNAL USE.  AUTHENTICATION, AUTHORIZATION AND VALIDATION MUST BE HANDLED PRIOR TO DB INTERACTION.
        /// <para />
        /// Creates a new status period.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        [EndpointMethod(AllowResponseLogging = true, AllowArgumentLogging = true, RequiresAuthentication = true)]
        private static void SubmitStatusPeriod(MessageToken token)
        {
            token.AssertLoggedIn();
            token.Args.AssertContainsKeys("statusperiod");

            StatusPeriod periodFromClient;

            try
            {
                periodFromClient = token.Args["statusperiod"].CastJToken<StatusPeriod>();
            }
            catch
            {
                throw new CommandCentralException("There was a problem deserializing your status period object.", ErrorTypes.Validation);
            }

            if (periodFromClient.StatusPeriodType == null)
                throw new CommandCentralException("You must select a status period type.", ErrorTypes.Validation);

            using (var session = DataAccess.NHibernateHelper.CreateStatefulSession())
            {
                using (var transaction = session.BeginTransaction())
                {
                    try
                    {

                        var statusType = session.Get<StatusPeriodType>(periodFromClient.StatusPeriodType.Id) ??
                            throw new CommandCentralException("Your status period type was not valid.", ErrorTypes.Validation);

                        var person = session.Get<Person>(periodFromClient.Person.Id) ??
                            throw new CommandCentralException("Your person was not valid.", ErrorTypes.Validation);

                        var resolvedPermissions = token.AuthenticationSession.Person.PermissionGroups.Resolve(token.AuthenticationSession.Person, person);

                        if (resolvedPermissions.ch)

                        var periodToInsert = new StatusPeriod
                        {
                            Id = Guid.NewGuid(),
                            Person = person, 

                        };
                    

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
