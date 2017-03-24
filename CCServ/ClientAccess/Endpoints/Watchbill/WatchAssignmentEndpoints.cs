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
    /// Defines all of the endpoints used to interact with the watch assignment object.
    /// </summary>
    static class WatchAssignmentEndpoints
    {
        /// <summary>
        /// WARNING!  THIS METHOD IS EXPOSED TO THE CLIENT AND IS NOT INTENDED FOR INTERNAL USE.  AUTHENTICATION, AUTHORIZATION AND VALIDATION MUST BE HANDLED PRIOR TO DB INTERACTION.
        /// <para />
        /// Loads a watch assignment.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        [EndpointMethod(AllowArgumentLogging = true, AllowResponseLogging = true, RequiresAuthentication = true)]
        private static void LoadWatchAssignment(MessageToken token)
        {
            //Just make sure the client is logged in.  The endpoint's description should've handled this but you never know.
            if (token.AuthenticationSession == null)
            {
                token.AddErrorMessage("You must be logged in to do that.", ErrorTypes.Authentication, System.Net.HttpStatusCode.Unauthorized);
                return;
            }

            if (!token.Args.ContainsKey("watchassignmentid"))
            {
                token.AddErrorMessage("You failed to send a 'watchassignmentid' parameter.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                return;
            }

            Guid watchAssignmentId;
            if (!Guid.TryParse(token.Args["watchassignmentid"] as string, out watchAssignmentId))
            {
                token.AddErrorMessage("Your watchassignmentid parameter's format was invalid.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                return;
            }

            using (var session = DataAccess.NHibernateHelper.CreateStatefulSession())
            {
                using (var transaction = session.BeginTransaction())
                {
                    try
                    {
                        var watchAssignmentFromDB = session.Get<WatchAssignment>(watchAssignmentId);

                        if (watchAssignmentFromDB == null)
                        {
                            token.AddErrorMessage("Your watch assignemnt's id was not valid.  Please consider creating it first.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                            return;
                        }

                        token.SetResult(watchAssignmentFromDB);

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
        /// Loads all watch assignments by certain criteria.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        [EndpointMethod(AllowArgumentLogging = true, AllowResponseLogging = true, RequiresAuthentication = true)]
        private static void SearchWatchAssignments(MessageToken token)
        {
            //Just make sure the client is logged in.  The endpoint's description should've handled this but you never know.
            if (token.AuthenticationSession == null)
            {
                token.AddErrorMessage("You must be logged in to do that.", ErrorTypes.Authentication, System.Net.HttpStatusCode.Unauthorized);
                return;
            }

            //Now let's see how the client wants to find our watch assignments.

            //Now let's go get the watchbill from the database.
            using (var session = DataAccess.NHibernateHelper.CreateStatefulSession())
            {
                using (var transaction = session.BeginTransaction())
                {
                    try
                    {
                        //TODO
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
        /// Inserts a number of watch assignments.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        [EndpointMethod(AllowArgumentLogging = true, AllowResponseLogging = true, RequiresAuthentication = true)]
        private static void CreateWatchAssignments(MessageToken token)
        {

            if (token.AuthenticationSession == null)
            {
                token.AddErrorMessage("You must be logged in to do that.", ErrorTypes.Authentication, System.Net.HttpStatusCode.Unauthorized);
                return;
            }

            if (!token.Args.ContainsKey("watchassignments"))
            {
                token.AddErrorMessage("You failed to send a 'watchassignments' paramater.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                return;
            }

            List<WatchAssignment> watchAssignmentsFromClient;
            try
            {
                watchAssignmentsFromClient = token.Args["watchassignments"].CastJToken<List<WatchAssignment>>();
            }
            catch
            {
                token.AddErrorMessage("An error occurred while parsing your 'watchassignments' parameter.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                return;
            }

            var watchAssignmentsToInsert = watchAssignmentsFromClient.Select(x =>
                {
                    return new WatchAssignment
                    {
                        AssignedBy = token.AuthenticationSession.Person,
                        CurrentState = Entities.ReferenceLists.Watchbill.WatchAssignmentStates.Assigned,
                        DateAssigned = token.CallTime,
                        Id = Guid.NewGuid(),
                        PersonAssigned = x.PersonAssigned,
                        WatchShift = x.WatchShift
                    };
                })
                .ToList();

            var validationResults = watchAssignmentsToInsert.Select(x => new WatchAssignment.WatchAssignmentValidator().Validate(x)).ToList();

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
                        foreach (var assignment in watchAssignmentsToInsert)
                        {
                            var personFromDB = session.Get<Entities.Person>(assignment.PersonAssigned.Id);

                            if (personFromDB == null)
                            {
                                token.AddErrorMessage("Your person's id was not valid.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                                return;
                            }

                            var shiftFromDB = session.Get<Entities.Watchbill.WatchShift>(assignment.WatchShift.Id);

                            if (shiftFromDB == null)
                            {
                                token.AddErrorMessage("Your shift's id was not valid.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                                return;
                            }

                            //Now we need to know what watchbill we're talking about.
                            var watchbill = shiftFromDB.WatchDays.First().Watchbill;

                            //If we're in the closed for inputs state, only the command level can create assignements.
                            //If we're in the under review state, dvision and department level can create assignemnts based on the person's division/department.
                            //If we're in the published state, command level can add assignemnts.
                            //All other can't add assignments.
                            //Check the state.
                            if (watchbill.CurrentState == Entities.ReferenceLists.Watchbill.WatchbillStatuses.ClosedForInputs ||
                                watchbill.CurrentState == Entities.ReferenceLists.Watchbill.WatchbillStatuses.Published)
                            {

                                if (!token.AuthenticationSession.Person.PermissionGroups
                                    .Any(x => x.ChainsOfCommandMemberOf.Contains(watchbill.ElligibilityGroup.OwningChainOfCommand)
                                        && x.AccessLevel == ChainOfCommandLevels.Command))
                                {
                                    token.AddErrorMessage("Only command level watchbill coordinators may add assignments while the watchbill is in this state.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                                    return;
                                }
                                else
                                {
                                    WatchAssignment supercededWatchAssignment = null;
                                    //We're in the Closed for inputs or published state and the client is command level.
                                    //Therefore, the client can add watch assignments all day.
                                    //Now let's add the assignment to the shift and then update the other assignments.
                                    foreach (var prevAssignment in shiftFromDB.WatchAssignments)
                                    {
                                        if (prevAssignment.CurrentState != Entities.ReferenceLists.Watchbill.WatchAssignmentStates.Superceded)
                                        {
                                            prevAssignment.CurrentState = Entities.ReferenceLists.Watchbill.WatchAssignmentStates.Superceded;
                                            
                                            //There should only ever be one watch assignment to supercede... let's make sure that's the case.
                                            if (supercededWatchAssignment != null)
                                                throw new Exception("More than one watch assignment that was not superceded was found!");

                                            supercededWatchAssignment = prevAssignment;
                                        }
                                    }

                                    shiftFromDB.WatchAssignments.Add(assignment);

                                    //One more thing, let's see if we're in the published state.  If we are, then we need to send the person an email.
                                    if (watchbill.CurrentState == Entities.ReferenceLists.Watchbill.WatchbillStatuses.Published)
                                    {

                                    }


                                    //Ok we're done with authorization and validation... looks like we should be good to add the assignment.
                                    session.Update(shiftFromDB);
                                }

                            }
                            else if (watchbill.CurrentState == Entities.ReferenceLists.Watchbill.WatchbillStatuses.UnderReview)
                            {
                                var resolvedPermissions = token.AuthenticationSession.Person.PermissionGroups.Resolve(token.AuthenticationSession.Person, assignment.PersonAssigned);

                                var highestLevel = resolvedPermissions.HighestLevels[watchbill.ElligibilityGroup.OwningChainOfCommand.ToString()];

                                switch (highestLevel)
                                {
                                    case ChainOfCommandLevels.Command:
                                        {
                                            throw new Exception("We managed to get to the command switch even though that should've been handled by the case above us.");
                                        }
                                    case ChainOfCommandLevels.Department:
                                        {
                                            if (!token.AuthenticationSession.Person.IsInSameDepartmentAs(personFromDB))
                                            {
                                                token.AddErrorMessage("You may not add watch assignments to a person not in your department.", ErrorTypes.Authorization, System.Net.HttpStatusCode.Unauthorized);
                                                return;
                                            }
                                            break;
                                        }
                                    case ChainOfCommandLevels.Division:
                                        {
                                            if (!token.AuthenticationSession.Person.IsInSameDivisionAs(personFromDB))
                                            {
                                                token.AddErrorMessage("You may not add watch assignments to a person not in your division.", ErrorTypes.Authorization, System.Net.HttpStatusCode.Unauthorized);
                                                return;
                                            }
                                            break;
                                        }
                                    case ChainOfCommandLevels.None:
                                    case ChainOfCommandLevels.Self:
                                        {
                                            token.AddErrorMessage("You lack sufficient permissions to add watch assignments.", ErrorTypes.Authorization, System.Net.HttpStatusCode.Unauthorized);
                                            break;
                                        }
                                    default:
                                        {
                                            throw new NotImplementedException("In the highest level chain of command switch in /CreateAssignments.");
                                        }
                                }
                            }
                            else
                            {
                                token.AddErrorMessage("You are not allowed to add assignments while the watchbill is in this state.", ErrorTypes.Authorization, System.Net.HttpStatusCode.Unauthorized);
                                return;
                            }

                            
                        }

                        token.SetResult(watchAssignmentsToInsert);

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
