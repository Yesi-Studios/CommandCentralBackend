using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AtwoodUtils;
using CCServ.Authorization;
using CCServ.Entities.TrainingModule;

namespace CCServ.ClientAccess.Endpoints.TrainingModuleEndpoints
{
    static class RequirementEndpoints
    {
        /// <summary>
        /// WARNING!  THIS METHOD IS EXPOSED TO THE CLIENT AND IS NOT INTENDED FOR INTERNAL USE.  AUTHENTICATION, AUTHORIZATION AND VALIDATION MUST BE HANDLED PRIOR TO DB INTERACTION.
        /// <para />
        /// Creates or updates a training requirement.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        [EndpointMethod(AllowArgumentLogging = true, AllowResponseLogging = true, RequiresAuthentication = true)]
        static void CreateRequirement(MessageToken token)
        {
            //Just make sure the client is logged in.  The endpoint's description should've handled this but you never know.
            if (token.AuthenticationSession == null)
            {
                token.AddErrorMessage("You must be logged in to create a training requirement.", ErrorTypes.Authentication, System.Net.HttpStatusCode.Unauthorized);
                return;
            }

            if (!token.AuthenticationSession.Person.PermissionGroups.CanAccessSubmodules(SubModules.TrainingAdmin.ToString()))
            {
                token.AddErrorMessage("You do not have permission to manage the training module.", ErrorTypes.Authorization, System.Net.HttpStatusCode.Unauthorized);
                return;
            }

            if (!token.Args.ContainsKey("requirement"))
            {
                token.AddErrorMessage("You failed to send a 'requirement' parameter.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                return;
            }

            Requirement requirementFromClient;
            try
            {
                requirementFromClient = token.Args["requirement"].CastJToken<Requirement>();
            }
            catch (Exception e)
            {
                token.AddErrorMessage("There was an error while trying to parse the requirement you sent.  Error: {0}".FormatS(e.Message), ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                return;
            }

            //Now to set some basic values on the object.
            requirementFromClient.Id = Guid.NewGuid();
            requirementFromClient.Creator = token.AuthenticationSession.Person;
            requirementFromClient.DateCreated = token.CallTime;

            //Now let's validate the requirement.
            var validationResult = new Requirement.RequirementValidator().Validate(requirementFromClient);

            if (!validationResult.IsValid)
            {
                token.AddErrorMessages(validationResult.Errors.Select(x => x.ErrorMessage), ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                return;
            }

            //Now that we have passed validation, let's begin the session.
            using (var session = DataAccess.NHibernateHelper.CreateStatefulSession())
            using (var transaction = session.BeginTransaction())
            {
                try
                {
                    //Since we know the requirement from the client is valid, let's go ahead and persist it.
                    session.Save(requirementFromClient);

                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        /// <summary>
        /// WARNING!  THIS METHOD IS EXPOSED TO THE CLIENT AND IS NOT INTENDED FOR INTERNAL USE.  AUTHENTICATION, AUTHORIZATION AND VALIDATION MUST BE HANDLED PRIOR TO DB INTERACTION.
        /// <para />
        /// Deletes a training requirement.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        [EndpointMethod(AllowArgumentLogging = true, AllowResponseLogging = true, RequiresAuthentication = true)]
        static void DeleteRequirement(MessageToken token)
        {
            //Just make sure the client is logged in.  The endpoint's description should've handled this but you never know.
            if (token.AuthenticationSession == null)
            {
                token.AddErrorMessage("You must be logged in to delete a training requirement.", ErrorTypes.Authentication, System.Net.HttpStatusCode.Unauthorized);
                return;
            }

            if (!token.AuthenticationSession.Person.PermissionGroups.CanAccessSubmodules(SubModules.TrainingAdmin.ToString()))
            {
                token.AddErrorMessage("You do not have permission to manage the training module.", ErrorTypes.Authorization, System.Net.HttpStatusCode.Unauthorized);
                return;
            }

            if (!token.Args.ContainsKey("requirement"))
            {
                token.AddErrorMessage("You failed to send a 'requirement' parameter.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                return;
            }

            Requirement requirementFromClient;
            try
            {
                requirementFromClient = token.Args["requirement"].CastJToken<Requirement>();
            }
            catch (Exception e)
            {
                token.AddErrorMessage("There was an error while trying to parse the requirement you sent.  Error: {0}".FormatS(e.Message), ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                return;
            }

            //Now that we have the requirement the client want to delete, let's start a session.
            //We need to make sure the requirement is real, and that it won't affect any assignments.
            using (var session = DataAccess.NHibernateHelper.CreateStatefulSession())
            using (var transaction = session.BeginTransaction())
            {
                try
                {
                    int personsWithAssignments = session
                        .QueryOver<Entities.Person>()
                        .Where(x => x.Assignments)
                        .RowCount();

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
