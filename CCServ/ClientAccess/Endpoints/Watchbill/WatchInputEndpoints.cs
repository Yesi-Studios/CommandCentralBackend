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
            //Just make sure the client is logged in.  The endpoint's description should've handled this but you never know.
            if (token.AuthenticationSession == null)
            {
                token.AddErrorMessage("You must be logged in to do that.", ErrorTypes.Authentication, System.Net.HttpStatusCode.Unauthorized);
                return;
            }

            if (!token.Args.ContainsKey("watchinputid"))
            {
                token.AddErrorMessage("You failed to send a 'watchinputid' parameter.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                return;
            }

            Guid watchInputId;
            if (!Guid.TryParse(token.Args["watchinputid"] as string, out watchInputId))
            {
                token.AddErrorMessage("Your watch input id parameter's format was invalid.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                return;
            }

            using (var session = DataAccess.NHibernateHelper.CreateStatefulSession())
            {
                using (var transaction = session.BeginTransaction())
                {
                    try
                    {
                        var watchInputFromDB = session.Get<WatchInput>(watchInputId);

                        if (watchInputFromDB == null)
                        {
                            token.AddErrorMessage("Your watch input's id was not valid.  Please consider creating the watch input first.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                            return;
                        }

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
            //Just make sure the client is logged in.  The endpoint's description should've handled this but you never know.
            if (token.AuthenticationSession == null)
            {
                token.AddErrorMessage("You must be logged in to do that.", ErrorTypes.Authentication, System.Net.HttpStatusCode.Unauthorized);
                return;
            }

            if (!token.Args.ContainsKey("watchinputs"))
            {
                token.AddErrorMessage("You failed to send a 'watchinputs' parameter.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                return;
            }

            List<WatchInput> watchInputsFromClient;
            try
            {
                watchInputsFromClient = token.Args["watchinputs"].CastJToken<List<WatchInput>>();
            }
            catch
            {
                token.AddErrorMessage("An error occurred while trying to parse your watch inputs.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                return;
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

            if (validationResults.Any(x => !x.IsValid))
            {
                token.AddErrorMessages(validationResults.SelectMany(x => x.Errors).Select(x => x.ErrorMessage), ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                return;
            }

            using (var session = DataAccess.NHibernateHelper.CreateStatefulSession())
            {
                using (var transaction = session.BeginTransaction())
                {
                    try
                    {
                        foreach (var input in watchInputsToInsert)
                        {
                            //First, let's get the person the client is talking about.
                            var personFromDB = session.Get<Entities.Person>(input.Person.Id);
                            if (personFromDB == null)
                            {
                                token.AddErrorMessage("The person Id given by an input was not valid.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                                return;
                            }

                            //Now let's confirm that our client is allowed to submit inputs for this person.
                            var resolvedPermissions = token.AuthenticationSession.Person.PermissionGroups
                                .Resolve(token.AuthenticationSession.Person, personFromDB);

                            if (resolvedPermissions.ChainOfCommandByModule[ChainsOfCommand.QuarterdeckWatchbill.ToString()])
                            {
                                token.AddErrorMessage("You are not authorized to submit inputs for this person.", ErrorTypes.Authorization, System.Net.HttpStatusCode.Unauthorized);
                                return;
                            }


                            session.Save(input);
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


    }
}
