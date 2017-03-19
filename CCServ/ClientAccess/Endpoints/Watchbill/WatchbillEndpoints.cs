using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AtwoodUtils;
using CCServ.Authorization;
using CCServ.Entities.Watchbill;

namespace CCServ.ClientAccess.Endpoints.Watchbill
{
    /// <summary>
    /// Contains all of the endpoints for interacting with the parent watchbill object.
    /// </summary>
    static class WatchbillEndpoints
    {

        /// <summary>
        /// WARNING!  THIS METHOD IS EXPOSED TO THE CLIENT AND IS NOT INTENDED FOR INTERNAL USE.  AUTHENTICATION, AUTHORIZATION AND VALIDATION MUST BE HANDLED PRIOR TO DB INTERACTION.
        /// <para />
        /// Creates a watchbill.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        [EndpointMethod(AllowArgumentLogging = true, AllowResponseLogging = true, RequiresAuthentication = true)]
        private static void CreateWatchbill(MessageToken token)
        {
            //Just make sure the client is logged in.  The endpoint's description should've handled this but you never know.
            if (token.AuthenticationSession == null)
            {
                token.AddErrorMessage("You must be logged in to create a watchbill.", ErrorTypes.Authentication, System.Net.HttpStatusCode.Unauthorized);
                return;
            }

            //Make sure the client has permission to manage the news.
            if (!token.AuthenticationSession.Person.PermissionGroups.CanAccessSubmodules(SubModules.CreateWatchbill.ToString()))
            {
                token.AddErrorMessage("You do not have permission to create a watchbill.", ErrorTypes.Authorization, System.Net.HttpStatusCode.Unauthorized);
            }

            if (!token.Args.ContainsKey("watchbill"))
            {
                token.AddErrorMessage("You failed to send a 'watchbill' parameter.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                return;
            }

            Entities.Watchbill.Watchbill watchbillFromClient;
            try
            {
                watchbillFromClient = token.Args["watchbill"].CastJToken<Entities.Watchbill.Watchbill>();
            }
            catch
            {
                token.AddErrorMessage("An error occurred while trying to parse your watchbill.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                return;
            }

            Entities.Watchbill.Watchbill watchbillToInsert = new Entities.Watchbill.Watchbill
            {
                Command = token.AuthenticationSession.Person.Command,
                CreatedBy = token.AuthenticationSession.Person,
                CurrentState = Entities.ReferenceLists.Watchbill.WatchbillStatuses.Initial,
                Id = Guid.NewGuid(),
                LastStateChange = token.CallTime,
                LastStateChangedBy = token.AuthenticationSession.Person,
                Title = watchbillFromClient.Title
            };

            var validationResult = new Entities.Watchbill.Watchbill.WatchbillValidator().Validate(watchbillToInsert);

            if (!validationResult.IsValid)
            {
                token.AddErrorMessages(validationResult.Errors.Select(x => x.ErrorMessage), ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                return;
            }

            using (var session = DataAccess.NHibernateHelper.CreateStatefulSession())
            {
                using (var transaction = session.BeginTransaction())
                {
                    try
                    {
                        session.Save(watchbillToInsert);

                        token.SetResult(watchbillToInsert);

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
