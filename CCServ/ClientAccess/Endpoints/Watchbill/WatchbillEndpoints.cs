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
        /// Loads a watchbill.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        [EndpointMethod(AllowArgumentLogging = true, AllowResponseLogging = true, RequiresAuthentication = true)]
        private static void LoadWatchbill(MessageToken token)
        {
            //Just make sure the client is logged in.  The endpoint's description should've handled this but you never know.
            if (token.AuthenticationSession == null)
            {
                token.AddErrorMessage("You must be logged in to do that.", ErrorTypes.Authentication, System.Net.HttpStatusCode.Unauthorized);
                return;
            }

            if (!token.Args.ContainsKey("watchbillid"))
            {
                token.AddErrorMessage("You failed to send a 'watchbillid' parameter.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                return;
            }

            Guid watchbillId;
            if (!Guid.TryParse(token.Args["watchbillid"] as string, out watchbillId))
            {
                token.AddErrorMessage("Your watchbill id parameter's format was invalid.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                return;
            }

            //Now let's go get the watchbill from the database.
            using (var session = DataAccess.NHibernateHelper.CreateStatefulSession())
            {
                using (var transaction = session.BeginTransaction())
                {
                    try
                    {
                        var watchbillFromDB = session.Get<Entities.Watchbill.Watchbill>(watchbillId);

                        if (watchbillFromDB == null)
                        {
                            token.AddErrorMessage("Your watchbill's id was not valid.  Please consider creating the watchbill first.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                            return;
                        }

                        token.SetResult(watchbillFromDB);

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
        /// Loads a watchbill.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        [EndpointMethod(AllowArgumentLogging = true, AllowResponseLogging = true, RequiresAuthentication = true)]
        private static void LoadWatchbills(MessageToken token)
        {
            //Just make sure the client is logged in.  The endpoint's description should've handled this but you never know.
            if (token.AuthenticationSession == null)
            {
                token.AddErrorMessage("You must be logged in to do that.", ErrorTypes.Authentication, System.Net.HttpStatusCode.Unauthorized);
                return;
            }

            //Now let's go get the watchbill from the database.
            using (var session = DataAccess.NHibernateHelper.CreateStatefulSession())
            {
                using (var transaction = session.BeginTransaction())
                {
                    try
                    {
                        token.SetResult(session.QueryOver<Entities.Watchbill.Watchbill>().List());

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
                token.AddErrorMessage("You must be logged in to do that.", ErrorTypes.Authentication, System.Net.HttpStatusCode.Unauthorized);
                return;
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

            NHibernate.NHibernateUtil.Initialize(token.AuthenticationSession.Person.Command);
            Entities.Watchbill.Watchbill watchbillToInsert = new Entities.Watchbill.Watchbill
            {
                Command = token.AuthenticationSession.Person.Command,
                CreatedBy = token.AuthenticationSession.Person,
                CurrentState = Entities.ReferenceLists.Watchbill.WatchbillStatuses.Initial,
                Id = Guid.NewGuid(),
                LastStateChange = token.CallTime,
                LastStateChangedBy = token.AuthenticationSession.Person,
                Title = watchbillFromClient.Title,
                ElligibilityGroup = watchbillFromClient.ElligibilityGroup
            };

            var validationResult = new Entities.Watchbill.Watchbill.WatchbillValidator().Validate(watchbillToInsert);

            if (!validationResult.IsValid)
            {
                token.AddErrorMessages(validationResult.Errors.Select(x => x.ErrorMessage), ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                return;
            }

            //Let's make sure the client is allowed to make a watchbill with this elligibility group.
            var ellGroup = Entities.ReferenceLists.Watchbill.WatchElligibilityGroups.AllWatchElligibilityGroups.FirstOrDefault(x => Guid.Equals(x.Id, watchbillToInsert.ElligibilityGroup.Id));

            if (ellGroup == null)
            {
                token.AddErrorMessage("You failed to provide a proper elligibilty group.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                return;
            }

            if (!token.AuthenticationSession.Person.PermissionGroups.Any(x => x.ChainsOfCommandMemberOf.Contains(ellGroup.OwningChainOfCommand) && x.AccessLevel == ChainOfCommandLevels.Command))
            {
                token.AddErrorMessage("You are not allowed to create a watchbill tied to that elligibility group.  You must have command level permissions in the related chain of command.", ErrorTypes.Authorization, System.Net.HttpStatusCode.Unauthorized);
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

        /// <summary>
        /// WARNING!  THIS METHOD IS EXPOSED TO THE CLIENT AND IS NOT INTENDED FOR INTERNAL USE.  AUTHENTICATION, AUTHORIZATION AND VALIDATION MUST BE HANDLED PRIOR TO DB INTERACTION.
        /// <para />
        /// Updates a watchbill.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        [EndpointMethod(AllowArgumentLogging = true, AllowResponseLogging = true, RequiresAuthentication = true)]
        private static void UpdateWatchbill(MessageToken token)
        {

            //Just make sure the client is logged in.  The endpoint's description should've handled this but you never know.
            if (token.AuthenticationSession == null)
            {
                token.AddErrorMessage("You must be logged in to do that.", ErrorTypes.Authentication, System.Net.HttpStatusCode.Unauthorized);
                return;
            }

            //Now we need to determine who can update this watchbill.
            if (!token.Args.ContainsKey("watchbill"))
            {
                token.AddErrorMessage("You failed to send a watchbill parameter.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                return;
            }

            Entities.Watchbill.Watchbill watchbillFromClient;
            try
            {
                watchbillFromClient = token.Args["watchbill"].CastJToken<Entities.Watchbill.Watchbill>();
            }
            catch
            {
                token.AddErrorMessage("An error occurred while deserializing your watchbill.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                return;
            }

            var validationResult = new Entities.Watchbill.Watchbill.WatchbillValidator().Validate(watchbillFromClient);

            if (!validationResult.IsValid)
            {
                token.AddErrorMessages(validationResult.Errors.Select(x => x.ErrorMessage), ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                return;
            }

            //Now let's go get the watchbill from the database.
            using (var session = DataAccess.NHibernateHelper.CreateStatefulSession())
            {
                using (var transaction = session.BeginTransaction())
                {
                    try
                    {
                        var watchbillFromDB = session.Get<Entities.Watchbill.Watchbill>(watchbillFromClient.Id);

                        if (watchbillFromDB == null)
                        {
                            token.AddErrorMessage("Your watchbill's id was not valid.  Please consider creating the watchbill first.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                            return;
                        }

                        //Ok so there's a watchbill.  Let's get the ell group to determine the permissions.
                        if (!token.AuthenticationSession.Person.PermissionGroups.Any(x => x.ChainsOfCommandMemberOf.Contains(watchbillFromDB.ElligibilityGroup.OwningChainOfCommand) && x.AccessLevel == ChainOfCommandLevels.Command))
                        {
                            token.AddErrorMessage("You are not allowed to edit this watchbill.  You must have command level permissions in the related chain of command.", ErrorTypes.Authorization, System.Net.HttpStatusCode.Unauthorized);
                            return;
                        }

                        //Now let's move the properties over that are editable.
                        watchbillFromDB.Title = watchbillFromClient.Title;

                        //If the state is different, we need to move the state as well.  There's a method for that.
                        if (watchbillFromDB.CurrentState != watchbillFromClient.CurrentState)
                        {
                            //It looks like the client is trying to change the state.
                            watchbillFromDB.SetState(watchbillFromClient.CurrentState, token.CallTime, token.AuthenticationSession.Person);
                        }

                        session.Update(watchbillFromDB);

                        token.SetResult(watchbillFromDB);

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
        /// Deletes a watchbill.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        [EndpointMethod(AllowArgumentLogging = true, AllowResponseLogging = true, RequiresAuthentication = true)]
        private static void DeleteWatchbill(MessageToken token)
        {

            //Just make sure the client is logged in.  The endpoint's description should've handled this but you never know.
            if (token.AuthenticationSession == null)
            {
                token.AddErrorMessage("You must be logged in to do that.", ErrorTypes.Authentication, System.Net.HttpStatusCode.Unauthorized);
                return;
            }

            //Now we need to determine who can update this watchbill.
            if (!token.Args.ContainsKey("watchbill"))
            {
                token.AddErrorMessage("You failed to send a watchbill parameter.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                return;
            }

            Entities.Watchbill.Watchbill watchbillFromClient;
            try
            {
                watchbillFromClient = token.Args["watchbill"].CastJToken<Entities.Watchbill.Watchbill>();
            }
            catch
            {
                token.AddErrorMessage("An error occurred while deserializing your watchbill.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                return;
            }

            //Now let's go get the watchbill from the database.
            using (var session = DataAccess.NHibernateHelper.CreateStatefulSession())
            {
                using (var transaction = session.BeginTransaction())
                {
                    try
                    {
                        var watchbillFromDB = session.Get<Entities.Watchbill.Watchbill>(watchbillFromClient.Id);

                        if (watchbillFromDB == null)
                        {
                            token.AddErrorMessage("Your watchbill's id was not valid.  Please consider creating the watchbill first.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                            return;
                        }

                        //Ok so there's a watchbill.  Let's get the ell group to determine the permissions.
                        if (!token.AuthenticationSession.Person.PermissionGroups.Any(x => x.ChainsOfCommandMemberOf.Contains(watchbillFromDB.ElligibilityGroup.OwningChainOfCommand) && x.AccessLevel == ChainOfCommandLevels.Command))
                        {
                            token.AddErrorMessage("You are not allowed to edit this watchbill.  You must have command level permissions in the related chain of command.", ErrorTypes.Authorization, System.Net.HttpStatusCode.Unauthorized);
                            return;
                        }

                        //Check the state.
                        if (watchbillFromDB.CurrentState != Entities.ReferenceLists.Watchbill.WatchbillStatuses.Initial)
                        {
                            token.AddErrorMessage("You may not delete a watchbill that is not in the initial state.  Please consider changing its state first.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                            return;
                        }

                        session.Delete(watchbillFromDB);

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
