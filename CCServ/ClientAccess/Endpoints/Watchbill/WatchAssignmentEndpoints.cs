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
using CCServ.Entities;

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
            token.AssertLoggedIn();
            token.Args.AssertContainsKeys("id");

            if (!Guid.TryParse(token.Args["id"] as string, out Guid watchAssignmentId))
                throw new CommandCentralException("Your watchassignmentid parameter's format was invalid.", ErrorTypes.Validation);

            using (var session = DataAccess.NHibernateHelper.CreateStatefulSession())
            {
                using (var transaction = session.BeginTransaction())
                {
                    try
                    {
                        var watchAssignmentFromDB = session.Get<WatchAssignment>(watchAssignmentId) ??
                            throw new CommandCentralException("Your watch assignemnt's id was not valid.  Please consider creating it first.", ErrorTypes.Validation);

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
            token.AssertLoggedIn();
            token.Args.AssertContainsKeys("filters");

            //Get the filters!
            Dictionary<string, object> filters = token.Args["filters"].CastJToken<Dictionary<string, object>>();

            //Make sure all the keys are real
            foreach (var key in filters.Keys)
            {
                if (!typeof(WatchAssignment).GetProperties().Select(x => x.Name).Contains(key, StringComparer.CurrentCultureIgnoreCase))
                    throw new CommandCentralException("One or more properties you tried to search were not real.", ErrorTypes.Validation);
            }

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
                        var query = new WatchAssignment.WatchAssignmentQueryProvider().CreateQuery(DataAccess.QueryTypes.Advanced, convertedFilters);

                        var results = query.GetExecutableQueryOver(session)
                            .TransformUsing(Transformers.DistinctRootEntity)
                            .List()
                            .Select(x =>
                                {
                                    return new
                                    {
                                        x.AcknowledgedBy,
                                        x.AssignedBy,
                                        x.CurrentState,
                                        x.DateAcknowledged,
                                        x.DateAssigned,
                                        x.Id,
                                        x.IsAcknowledged,
                                        x.PersonAssigned,
                                        WatchShift = new
                                        {
                                            x.WatchShift.Id,
                                            x.WatchShift.Range,
                                            x.WatchShift.ShiftType,
                                            x.WatchShift.Title
                                        }
                                    };
                                }); ;

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
        /// Swaps a number of watch assignments.  Watch assignments may only be swapped if there are two.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        [EndpointMethod(AllowArgumentLogging = true, AllowResponseLogging = true, RequiresAuthentication = true)]
        private static void SwapWatchAssignments(MessageToken token)
        {
            token.AssertLoggedIn();
            token.Args.AssertContainsKeys("id1", "id2");

            if (!Guid.TryParse(token.Args["id1"] as string, out Guid id1) ||
                !Guid.TryParse(token.Args["id2"] as string, out Guid id2))
            {
                throw new CommandCentralException("One or more of your ids were in the wrong format.", ErrorTypes.Validation);
            }

            if (id1 == id2)
                throw new CommandCentralException("Both ids may not be equal.", ErrorTypes.Validation);

            using (var session = DataAccess.NHibernateHelper.CreateStatefulSession())
            {
                using (var transaction = session.BeginTransaction())
                {
                    try
                    {
                        var ass1 = session.Get<WatchAssignment>(id1) ??
                            throw new CommandCentralException("Your first watch assignment id was not valid.", ErrorTypes.Validation);

                        var ass2 = session.Get<WatchAssignment>(id2) ??
                            throw new CommandCentralException("Your second watch assignment id was not valid.", ErrorTypes.Validation);

                        if (ass1.WatchShift.WatchAssignment == null || ass1.WatchShift.WatchAssignment == null)
                            throw new CommandCentralException("Both given watch assignments must have shifts which have already been assigned a watch assignment.", ErrorTypes.Validation);

                        if (ass1.CurrentState == WatchAssignmentStates.Completed || ass1.CurrentState == WatchAssignmentStates.Excused || ass1.CurrentState == WatchAssignmentStates.Missed
                            || ass2.CurrentState == WatchAssignmentStates.Completed || ass2.CurrentState == WatchAssignmentStates.Excused || ass2.CurrentState == WatchAssignmentStates.Missed)
                            throw new CommandCentralException("You may not swap assignments if one of the assignments has been completed, excused, or missed.", ErrorTypes.Validation);

                        if (ass1.WatchShift.Watchbill.Id != ass2.WatchShift.Watchbill.Id)
                            throw new CommandCentralException("You may not swap assignments if those assignments are from different watchbills.", ErrorTypes.Validation);

                        if (ass1.PersonAssigned.Id == ass2.PersonAssigned.Id)
                            throw new CommandCentralException("You may not swap shifts if both are assigned to the same person.", ErrorTypes.Validation);

                        //Now we're going to do some permissions checking to ensure that the client is in the chains of command of both persons.
                        var ass1ResolvedPermissions = token.AuthenticationSession.Person.PermissionGroups.Resolve(token.AuthenticationSession.Person, ass1.PersonAssigned);
                        var ass2ResolvedPermissions = token.AuthenticationSession.Person.PermissionGroups.Resolve(token.AuthenticationSession.Person, ass2.PersonAssigned);

                        var ass1HighestLevel = ass1ResolvedPermissions.HighestLevels[ass1.WatchShift.Watchbill.EligibilityGroup.OwningChainOfCommand.ToString()];
                        var ass2HighestLevel = ass2ResolvedPermissions.HighestLevels[ass2.WatchShift.Watchbill.EligibilityGroup.OwningChainOfCommand.ToString()];

                        switch (ass1HighestLevel)
                        {
                            case ChainOfCommandLevels.Command:
                                {
                                    if (!token.AuthenticationSession.Person.IsInSameCommandAs(ass1.PersonAssigned))
                                    {
                                        throw new CommandCentralException("You may not swap watch assignments for a person not in your command.", ErrorTypes.Authorization);
                                    }
                                    break;
                                }
                            case ChainOfCommandLevels.Department:
                                {
                                    if (!token.AuthenticationSession.Person.IsInSameDepartmentAs(ass1.PersonAssigned))
                                    {
                                        throw new CommandCentralException("You may not swap watch assignments for a person not in your department.", ErrorTypes.Authorization);
                                    }
                                    break;
                                }
                            case ChainOfCommandLevels.Division:
                                {
                                    if (!token.AuthenticationSession.Person.IsInSameDivisionAs(ass1.PersonAssigned))
                                    {
                                        throw new CommandCentralException("You may not swap watch assignments for a person not in your division.", ErrorTypes.Authorization);
                                    }
                                    break;
                                }
                            case ChainOfCommandLevels.None:
                            case ChainOfCommandLevels.Self:
                                {
                                    throw new CommandCentralException("You lack sufficient permissions to swap watch assignments.", ErrorTypes.Authorization);
                                }
                            default:
                                {
                                    throw new NotImplementedException("In the highest level chain of command switch in /SwapWatchAssignments (ass1).");
                                }
                        }

                        switch (ass2HighestLevel)
                        {
                            case ChainOfCommandLevels.Command:
                                {
                                    if (!token.AuthenticationSession.Person.IsInSameCommandAs(ass2.PersonAssigned))
                                    {
                                        throw new CommandCentralException("You may not swap watch assignments for a person not in your command.", ErrorTypes.Authorization);
                                    }
                                    break;
                                }
                            case ChainOfCommandLevels.Department:
                                {
                                    if (!token.AuthenticationSession.Person.IsInSameDepartmentAs(ass2.PersonAssigned))
                                    {
                                        throw new CommandCentralException("You may not swap watch assignments for a person not in your department.", ErrorTypes.Authorization);
                                    }
                                    break;
                                }
                            case ChainOfCommandLevels.Division:
                                {
                                    if (!token.AuthenticationSession.Person.IsInSameDivisionAs(ass2.PersonAssigned))
                                    {
                                        throw new CommandCentralException("You may not swap watch assignments for a person not in your division.", ErrorTypes.Authorization);
                                    }
                                    break;
                                }
                            case ChainOfCommandLevels.None:
                            case ChainOfCommandLevels.Self:
                                {
                                    throw new CommandCentralException("You lack sufficient permissions to swap watch assignments.", ErrorTypes.Authorization);
                                }
                            default:
                                {
                                    throw new NotImplementedException("In the highest level chain of command switch in /SwapWatchAssignments (ass2).");
                                }
                        }

                        //After all this freaking validation, it looks like we're ready to do the actual swap.

                        var shift1 = ass1.WatchShift;
                        var shift2 = ass2.WatchShift;

                        ass1.WatchShift = shift2;
                        ass2.WatchShift = shift1;

                        shift1.WatchAssignment = ass2;
                        shift2.WatchAssignment = ass1;

                        //Now let's add some comments.
                        ass1.WatchShift.Comments.Add(new Comment
                        {
                            Creator = null,
                            Id = Guid.NewGuid(),
                            Text = "{0} was assigned to this shift by {1}.  Previously, {2} was assigned to this shift.".FormatS(ass2.PersonAssigned, token.AuthenticationSession.Person, ass1.PersonAssigned)
                        });

                        ass2.WatchShift.Comments.Add(new Comment
                        {
                            Creator = null,
                            Id = Guid.NewGuid(),
                            Text = "{0} was assigned to this shift by {1}.  Previously, {2} was assigned to this shift.".FormatS(ass1.PersonAssigned, token.AuthenticationSession.Person, ass2.PersonAssigned)
                        });

                        session.Update(ass1.WatchShift);
                        session.Update(ass2.WatchShift);

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
        /// Sets the given watch assignment as acknowledged by the client.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        [EndpointMethod(AllowArgumentLogging = true, AllowResponseLogging = true, RequiresAuthentication = true)]
        private static void AcknowledgeWatchAssignment(MessageToken token)
        {
            token.AssertLoggedIn();

            token.Args.AssertContainsKeys("id");

            if (!Guid.TryParse(token.Args["id"] as string, out Guid id))
                throw new CommandCentralException("Your assignment id was not in the right format.", ErrorTypes.Validation);

            using (var session = DataAccess.NHibernateHelper.CreateStatefulSession())
            {
                using (var transaction = session.BeginTransaction())
                {
                    try
                    {
                        var assignment = session.Get<WatchAssignment>(id) ??
                            throw new CommandCentralException("Your assignment id was not valid.", ErrorTypes.Validation);

                        var watchbill = assignment.WatchShift.Watchbill;

                        if (watchbill.CurrentState != WatchbillStatuses.Published)
                            throw new CommandCentralException("You may not acknowledge a watch assignment until the watchbill has been published.", ErrorTypes.Validation);

                        //Now let's confirm that our client is allowed to submit inputs for this person.
                        var resolvedPermissions = token.AuthenticationSession.Person.PermissionGroups
                            .Resolve(token.AuthenticationSession.Person, assignment.PersonAssigned);

                        if (!resolvedPermissions.ChainOfCommandByModule[watchbill.EligibilityGroup.OwningChainOfCommand.ToString()])
                            throw new CommandCentralException("You are not authorized to edit inputs for this person.  " +
                                "If this is your own input and you need to change the date range, " +
                                "please delete the input and then re-create it for the proper range.",
                                ErrorTypes.Authorization);

                        if (assignment.IsAcknowledged)
                            return;

                        assignment.IsAcknowledged = true;
                        assignment.AcknowledgedBy = token.AuthenticationSession.Person;
                        assignment.DateAcknowledged = token.CallTime;

                        session.Update(assignment);

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
            token.AssertLoggedIn();

            token.Args.AssertContainsKeys("watchassignments", "watchbillid");

            var watchAssToken = token.Args["watchassignments"].CastJToken();

            if (watchAssToken.Type != Newtonsoft.Json.Linq.JTokenType.Array)
                throw new CommandCentralException("Your watch assignments parameter must be an array.", ErrorTypes.Validation);

            var watchAssignmentsFromClient = watchAssToken.Select(x =>
            {

                if (!Guid.TryParse(x.Value<string>(nameof(WatchAssignment.PersonAssigned)), out Guid personAssignedId))
                    throw new CommandCentralException("Your person assigned id was in the wrong format.", ErrorTypes.Validation);

                if (!Guid.TryParse(x.Value<string>(nameof(WatchAssignment.WatchShift)), out Guid watchShitId))
                    throw new CommandCentralException("Your person assigned id was in the wrong format.", ErrorTypes.Validation);

                var watchAss = new
                {
                    PersonAssignedId = personAssignedId,
                    WatchShiftId = watchShitId
                };

                return watchAss;
            });

            if (!Guid.TryParse(token.Args["watchbillid"] as string, out Guid watchbillId))
                throw new CommandCentralException("Your watchbill id was in the wrong format.", ErrorTypes.Validation);

            using (var session = DataAccess.NHibernateHelper.CreateStatefulSession())
            {
                using (var transaction = session.BeginTransaction())
                {
                    try
                    {
                        //Let's make sure all the watch assignments are from the same watchbill, and there shifts and people are real.
                        Entities.Watchbill.Watchbill watchbill = session.Get<Entities.Watchbill.Watchbill>(watchbillId) ??
                            throw new CommandCentralException("Your watchbill id was not valid.", ErrorTypes.Validation);

                        bool isCommandCoordinator = token.AuthenticationSession.Person.PermissionGroups
                                .Any(x => x.ChainsOfCommandMemberOf.Contains(watchbill.EligibilityGroup.OwningChainOfCommand)
                                    && x.AccessLevel == ChainOfCommandLevels.Command);

                        if (!isCommandCoordinator)
                            throw new CommandCentralException("You are not allowed to create watch assignments if you are not the watchbill command coordinator.", ErrorTypes.Authorization);

                        if (watchbill.CurrentState != WatchbillStatuses.ClosedForInputs &&
                            watchbill.CurrentState != WatchbillStatuses.Published &&
                            watchbill.CurrentState != WatchbillStatuses.UnderReview)
                            throw new CommandCentralException("You are not allowed to create watch assignments unless the watchbill is in the closed for inputs, published or under review state.", ErrorTypes.Authorization);

                        foreach (var assignment in watchAssignmentsFromClient)
                        {

                            var personAssigned = session.Get<Person>(assignment.PersonAssignedId) ??
                                throw new CommandCentralException("Your person assigned id was not valid.", ErrorTypes.Validation);

                            var watchShift = session.Get<WatchShift>(assignment.WatchShiftId) ??
                                throw new CommandCentralException("Your watch shift id was not valid.", ErrorTypes.Validation);

                            var assignmentToInsert = new WatchAssignment
                            {
                                AssignedBy = token.AuthenticationSession.Person,
                                CurrentState = WatchAssignmentStates.Assigned,
                                DateAssigned = token.CallTime,
                                Id = Guid.NewGuid(),
                                PersonAssigned = personAssigned,
                                WatchShift = watchShift
                            };

                            if (!watchbill.EligibilityGroup.EligiblePersons.Any(person => person.Id == assignmentToInsert.PersonAssigned.Id))
                                throw new CommandCentralException("You may not add this person to shift in this watchbill because they are not eligible for it.", ErrorTypes.Validation);

                            var validationResults = new WatchAssignment.WatchAssignmentValidator().Validate(assignmentToInsert);

                            if (!validationResults.IsValid)
                                throw new AggregateException(validationResults.Errors.Select(error => new CommandCentralException(error.ErrorMessage, ErrorTypes.Validation)));

                            //Now we're going to actually add/insert the watch assignment.
                            Comment comment;

                            //In this case, a new watch assignment is being created for a shift that has never had an assignment before.
                            if (assignmentToInsert.WatchShift.WatchAssignment == null)
                            {
                                comment = new Comment
                                {
                                    Creator = null,
                                    Id = Guid.NewGuid(),
                                    Text = "{0} was assigned to this shift by {1}.".FormatS(assignmentToInsert.PersonAssigned, token.AuthenticationSession.Person),
                                    Time = token.CallTime
                                };
                            }
                            //In this case, a watch change is occurring.
                            else if (assignmentToInsert.WatchShift.WatchAssignment.Id != assignmentToInsert.Id)
                            {
                                comment = new Comment
                                {
                                    Creator = null,
                                    Id = Guid.NewGuid(),
                                    Text = "{0} was assigned to this shift by {1}.  Previously, {2} was assigned to this shift."
                                        .FormatS(assignmentToInsert.PersonAssigned, token.AuthenticationSession.Person, assignmentToInsert.WatchShift.WatchAssignment.PersonAssigned),
                                    Time = token.CallTime
                                };
                            }
                            //Who the fuck knows about this one.
                            else
                            {
                                throw new NotImplementedException("Fell to default case in watch assignment creation.");
                            }


                            assignmentToInsert.WatchShift.WatchAssignment = assignmentToInsert;
                            assignmentToInsert.WatchShift.Comments.Add(comment);

                        }

                        session.Update(watchbill);

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
