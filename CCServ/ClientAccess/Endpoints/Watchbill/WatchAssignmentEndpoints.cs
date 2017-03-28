using CCServ.Entities.Watchbill;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AtwoodUtils;
using CCServ.Authorization;
using CCServ.Entities.ReferenceLists.Watchbill;
using System.Reflection;
using NHibernate.Transform;

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

            //Let's find which fields the client wants to search in.  This should be a dictionary.
            if (!token.Args.ContainsKey("filters"))
            {
                token.AddErrorMessage("You didn't send a 'filters' parameter.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                return;
            }
            Dictionary<string, object> filters = token.Args["filters"].CastJToken<Dictionary<string, object>>();

            var convertedFilters = filters.ToDictionary(
                    x => (MemberInfo)PropertySelector.SelectPropertyFrom<WatchAssignment>(x.Key),
                    x => x.Value);

            //Now let's go get the watchbill from the database.
            using (var session = DataAccess.NHibernateHelper.CreateStatefulSession())
            {
                using (var transaction = session.BeginTransaction())
                {
                    try
                    {
                        var resultQueryToken = new WatchAssignment.WatchAssignmentQueryProvider().CreateAdvancedQueryFor(convertedFilters);

                        var results = resultQueryToken.Query.GetExecutableQueryOver(session)
                            .TransformUsing(Transformers.DistinctRootEntity)
                            .List();

                        token.SetResult(results);

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
                        CurrentState = WatchAssignmentStates.Assigned,
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
                        //Let's make sure all the watch assignments are from the same watchbill, and there shifts and people are real.
                        Entities.Watchbill.Watchbill watchbill = null;

                        foreach (var assignment in watchAssignmentsToInsert)
                        {
                            var personFromDB = session.Get<Entities.Person>(assignment.PersonAssigned.Id);

                            if (personFromDB == null)
                            {
                                token.AddErrorMessage("A person's id was not valid.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                                return;
                            }
                            assignment.PersonAssigned = personFromDB;

                            var shiftFromDB = session.Get<WatchShift>(assignment.WatchShift.Id);

                            if (shiftFromDB == null)
                            {
                                token.AddErrorMessage("A shift's id was not valid.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                                return;
                            }
                            assignment.WatchShift = shiftFromDB;

                            //Now we need to know what watchbill we're talking about.
                            if (watchbill == null)
                            {
                                watchbill = shiftFromDB.WatchDays.First().Watchbill;
                            }
                            else if (watchbill.Id != shiftFromDB.WatchDays.First().Watchbill.Id)
                            {
                                token.AddErrorMessage("You may not submit watch assignments for multiple watchbills at the same time.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                                return;
                            }

                            //Let's make sure the person we're about to assign is in the eligibility group.
                            if (!watchbill.EligibilityGroup.EligiblePersons.Any(x => x.Id == personFromDB.Id))
                            {
                                token.AddErrorMessage("You may not add this person to shift in this watchbill because they are not eligible for it.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                                return;
                            }
                        }

                        bool isCommandCoordinator = token.AuthenticationSession.Person.PermissionGroups
                                .Any(x => x.ChainsOfCommandMemberOf.Contains(watchbill.EligibilityGroup.OwningChainOfCommand)
                                    && x.AccessLevel == ChainOfCommandLevels.Command);

                        //Now, if the watchbill is in the closed for inputs, published, or under review, and the client is a coordinator, then we can insert all of the assignments.
                        if ((watchbill.CurrentState == WatchbillStatuses.ClosedForInputs ||
                            watchbill.CurrentState == WatchbillStatuses.Published ||
                            watchbill.CurrentState == WatchbillStatuses.UnderReview) && isCommandCoordinator)
                        {
                            foreach (var assignment in watchAssignmentsToInsert)
                            {
                                //We need to set the other assignments to "superceded"
                                SupercedeWatchAssignment(assignment.WatchShift, assignment, watchbill);
                            }
                        }
                        else if (!isCommandCoordinator && watchbill.CurrentState == WatchbillStatuses.UnderReview)
                        {
                            //We have to make sure that the watch assignments were submitted in a pair, and that the business rules about those are kept.  They're complicated and make my head hurt.
                            if (watchAssignmentsToInsert.Count != 2)
                            {
                                token.AddErrorMessage("You may not swap watches unless your watches are submitted in pairs.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                                return;
                            }

                            //Make sure they actually swap each other.
                            if (watchAssignmentsToInsert.First().PersonAssigned.Id != watchAssignmentsToInsert.Last().WatchShift.WatchAssignments.First(x => x.CurrentState != WatchAssignmentStates.Superceded).PersonAssigned.Id ||
                                watchAssignmentsToInsert.Last().PersonAssigned.Id != watchAssignmentsToInsert.First().WatchShift.WatchAssignments.First(x => x.CurrentState != WatchAssignmentStates.Superceded).PersonAssigned.Id)
                            {
                                token.AddErrorMessage("You may not submit new watch assignments unless the previously assigned people for each shift are the other assignment's person.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                                return;
                            }

                            //And they're not the same shift.  That would be weird.
                            if (watchAssignmentsToInsert.First().WatchShift.Id == watchAssignmentsToInsert.Last().WatchShift.Id)
                            {
                                token.AddErrorMessage("The watch shifts may not be the same during a watch swap.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                                return;
                            }

                            //Ok, now let's start iterating to check the permissions.  If everything is good, we'll update the watches after this loop.
                            foreach (var assignment in watchAssignmentsToInsert)
                            {
                                //If we're here, we know that the person is not a command coordinator.
                                var resolvedPermissions = token.AuthenticationSession.Person.PermissionGroups.Resolve(token.AuthenticationSession.Person, assignment.PersonAssigned);

                                var highestLevel = resolvedPermissions.HighestLevels[watchbill.EligibilityGroup.OwningChainOfCommand.ToString()];

                                switch (highestLevel)
                                {
                                    case ChainOfCommandLevels.Command:
                                        {
                                            throw new Exception("We managed to get to the command switch even though that should've been handled by the case above us.");
                                        }
                                    case ChainOfCommandLevels.Department:
                                        {
                                            if (!token.AuthenticationSession.Person.IsInSameDepartmentAs(assignment.PersonAssigned))
                                            {
                                                token.AddErrorMessage("You may not add watch assignments for a person not in your department.", ErrorTypes.Authorization, System.Net.HttpStatusCode.Unauthorized);
                                                return;
                                            }
                                            break;
                                        }
                                    case ChainOfCommandLevels.Division:
                                        {
                                            if (!token.AuthenticationSession.Person.IsInSameDivisionAs(assignment.PersonAssigned))
                                            {
                                                token.AddErrorMessage("You may not add watch assignments for a person not in your division.", ErrorTypes.Authorization, System.Net.HttpStatusCode.Unauthorized);
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

                            SupercedeWatchAssignment(watchAssignmentsToInsert.First().WatchShift, watchAssignmentsToInsert.Last(), watchbill);
                            SupercedeWatchAssignment(watchAssignmentsToInsert.Last().WatchShift, watchAssignmentsToInsert.First(), watchbill);
                        }
                        else
                        {
                            token.AddErrorMessage("You may not add watch assignments to a watchbill in this state with your permissions.", ErrorTypes.Authorization, System.Net.HttpStatusCode.Unauthorized);
                            return;
                        }

                        //Well if we got down here, some change occurred!  Let's update the watchbill.
                        session.Update(watchbill);

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

        private static void SupercedeWatchAssignment(WatchShift shift, WatchAssignment newAssignment, Entities.Watchbill.Watchbill watchbill)
        {
            WatchAssignment supercededWatchAssignment = null;

            //We're in the Closed for inputs or published state and the client is command level.
            //Therefore, the client can add watch assignments all day.
            //Now let's add the assignment to the shift and then update the other assignments.
            foreach (var prevAssignment in shift.WatchAssignments)
            {
                if (prevAssignment.CurrentState != WatchAssignmentStates.Superceded)
                {
                    prevAssignment.CurrentState = WatchAssignmentStates.Superceded;

                    //There should only ever be one watch assignment to supercede... let's make sure that's the case.
                    if (supercededWatchAssignment != null)
                        throw new Exception("More than one watch assignment that was not superceded was found!");

                    supercededWatchAssignment = prevAssignment;
                }
            }

            shift.WatchAssignments.Add(newAssignment);

            //One more thing, let's see if we're in the published state.  If we are, then we need to send the person an email.
            if (watchbill.CurrentState == WatchbillStatuses.Published)
            {
                //Let's do the previous one first.
                var supercededAddresses = supercededWatchAssignment.PersonAssigned.EmailAddresses.Where(x => x.IsPreferred);
                if (supercededAddresses.Any())
                {

                    var model = new Email.Models.WatchReassignedEmailModel
                    {
                        FriendlyName = supercededWatchAssignment.PersonAssigned.ToString(),
                        WatchAssignment = supercededWatchAssignment,
                        Watchbill = watchbill.Title
                    };

                    Email.EmailInterface.CCEmailMessage
                        .CreateDefault()
                        .To(supercededAddresses.Select(x => new System.Net.Mail.MailAddress(x.Address, supercededWatchAssignment.PersonAssigned.ToString())))
                        .Subject("Watch Reassigned")
                        .HTMLAlternateViewUsingTemplateFromEmbedded("CCServ.Email.Templates.WatchReassignedRemoved_HTML.html", model)
                        .SendWithRetryAndFailure(TimeSpan.FromSeconds(1));
                }

                //Now the new watch assignment.
                var newAddresses = newAssignment.PersonAssigned.EmailAddresses.Where(x => x.IsPreferred);
                if (newAddresses.Any())
                {
                    var model = new Email.Models.WatchReassignedEmailModel
                    {
                        FriendlyName = newAssignment.PersonAssigned.ToString(),
                        WatchAssignment = newAssignment,
                        Watchbill = watchbill.Title
                    };

                    Email.EmailInterface.CCEmailMessage
                        .CreateDefault()
                        .To(newAddresses.Select(x => new System.Net.Mail.MailAddress(x.Address, newAssignment.PersonAssigned.ToString())))
                        .Subject("Watch Reassigned")
                        .HTMLAlternateViewUsingTemplateFromEmbedded("CCServ.Email.Templates.WatchReassignedAdded_HTML.html", model)
                        .SendWithRetryAndFailure(TimeSpan.FromSeconds(1));
                }
            }
        }


    }
}
