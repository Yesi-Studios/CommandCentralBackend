using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AtwoodUtils;
using CommandCentral.Authorization;
using CommandCentral.Entities.Watchbill;
using CommandCentral.Entities.ReferenceLists;
using CommandCentral.Entities.ReferenceLists.Watchbill;
using CommandCentral.Entities;

namespace CommandCentral.ClientAccess.Endpoints.Watchbill
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
            token.AssertLoggedIn();
            token.Args.AssertContainsKeys("id");

            if (!Guid.TryParse(token.Args["id"] as string, out var watchbillId))
                throw new CommandCentralException("Your watchbill id parameter's format was invalid.",
                    ErrorTypes.Validation);

            //Now let's go get the watchbill from the database.
            using (var session = DataAccess.DataProvider.CreateStatefulSession())
            {
                var watchbillFromDB = session.Get<Entities.Watchbill.Watchbill>(watchbillId) ??
                                      throw new CommandCentralException(
                                          "Your watchbill's id was not valid.  Please consider creating the watchbill first.",
                                          ErrorTypes.Validation);

                //We're also going to go see who is in the eligibility group and has no watch qualifications that pertain to this watchbill.
                //First we need to know all the possible needed watch qualifications.
                var watchQualifications =
                    watchbillFromDB.WatchShifts.SelectMany(x => x.ShiftType.RequiredWatchQualifications);

                var personsWithoutAtLeastOneQualification = watchbillFromDB.EligibilityGroup.EligiblePersons
                                                                           .Where(person =>
                                                                                      !person.WatchQualifications.Any(
                                                                                          qual =>
                                                                                              watchQualifications
                                                                                                  .Contains(
                                                                                                      qual)));

                //We also need to build some additional information into the watch assignments regarding chain of command for the client and the assigned person.
                List<object> watchShifts = new List<object>();

                ChainOfCommandLevels highestLevel = ChainOfCommandLevels.None;

                foreach (var group in token.AuthenticationSession.Person.PermissionGroups)
                {
                    if (group.ChainsOfCommandParts.Any(x => x.ChainOfCommand == ChainsOfCommand.QuarterdeckWatchbill))
                    {
                        if (group.AccessLevel > highestLevel)
                            highestLevel = group.AccessLevel;
                    }
                }

                if (watchbillFromDB.WatchShifts.Any())
                {
                    foreach (var shift in watchbillFromDB.WatchShifts)
                    {
                        bool isClientResponsibleForAssignment = false;
                        bool IsClientResponsibleForShift = false;

                        switch (highestLevel)
                        {
                            case ChainOfCommandLevels.Command:
                            {
                                IsClientResponsibleForShift =
                                    token.AuthenticationSession.Person.Command ==
                                    shift.DivisionAssignedTo?.Department.Command;
                                isClientResponsibleForAssignment =
                                    token.AuthenticationSession.Person.Command ==
                                    shift.WatchAssignment?.PersonAssigned?.Command;
                                break;
                            }
                            case ChainOfCommandLevels.Department:
                            {
                                IsClientResponsibleForShift =
                                    token.AuthenticationSession.Person.Department ==
                                    shift.DivisionAssignedTo?.Department;
                                isClientResponsibleForAssignment =
                                    token.AuthenticationSession.Person.Department ==
                                    shift.WatchAssignment?.PersonAssigned?.Department;
                                break;
                            }
                            case ChainOfCommandLevels.Division:
                            {
                                IsClientResponsibleForShift =
                                    token.AuthenticationSession.Person.Division == shift.DivisionAssignedTo;
                                isClientResponsibleForAssignment =
                                    token.AuthenticationSession.Person.Division ==
                                    shift.WatchAssignment?.PersonAssigned?.Division;
                                break;
                            }
                            case ChainOfCommandLevels.Self:
                            case ChainOfCommandLevels.None:
                            {
                                break;
                            }
                            default:
                            {
                                throw new NotImplementedException();
                            }
                        }

                        watchShifts.Add(new
                                        {
                                            shift.Id,
                                            shift.Comments,
                                            shift.Points,
                                            shift.Range,
                                            shift.ShiftType,
                                            shift.Title,
                                            IsClientResponsibleFor = IsClientResponsibleForShift,
                                            Watchbill = new {shift.Watchbill.Id},
                                            WatchAssignment = shift.WatchAssignment == null
                                                ? null
                                                : new
                                                  {
                                                      shift.WatchAssignment.AcknowledgedBy,
                                                      shift.WatchAssignment.AssignedBy,
                                                      shift.WatchAssignment.DateAcknowledged,
                                                      shift.WatchAssignment.DateAssigned,
                                                      shift.WatchAssignment.Id,
                                                      shift.WatchAssignment.IsAcknowledged,
                                                      shift.WatchAssignment.NumberOfAlertsSent,
                                                      shift.WatchAssignment.PersonAssigned,
                                                      WatchShift = new {shift.WatchAssignment.WatchShift.Id},
                                                      IsClientResponsibleFor = isClientResponsibleForAssignment
                                                  }
                                        });
                    }
                }

                token.SetResult(new
                                {
                                    watchbillFromDB.CreatedBy,
                                    watchbillFromDB.CurrentState,
                                    watchbillFromDB.EligibilityGroup,
                                    watchbillFromDB.Id,
                                    watchbillFromDB.InputRequirements,
                                    watchbillFromDB.LastStateChange,
                                    watchbillFromDB.LastStateChangedBy,
                                    watchbillFromDB.Title,
                                    watchbillFromDB.Range,
                                    WatchShifts = watchShifts,
                                    watchbillFromDB.WatchInputs,
                                    NotQualledPersons = personsWithoutAtLeastOneQualification,
                                    Analytics = new Dictionary<string, object> {{"MultipleAssignments", null}}
                                });
            }
        }

        /// <summary>
        /// WARNING!  THIS METHOD IS EXPOSED TO THE CLIENT AND IS NOT INTENDED FOR INTERNAL USE.  AUTHENTICATION, AUTHORIZATION AND VALIDATION MUST BE HANDLED PRIOR TO DB INTERACTION.
        /// <para />
        /// Loads all watchbills.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        [EndpointMethod(AllowArgumentLogging = true, AllowResponseLogging = true, RequiresAuthentication = true)]
        private static void LoadWatchbills(MessageToken token)
        {
            token.AssertLoggedIn();

            //Now let's go get the watchbill from the database.
            using (var session = DataAccess.DataProvider.CreateStatefulSession())
            {
                using (var transaction = session.BeginTransaction())
                {
                    try
                    {
                        token.SetResult(session.QueryOver<Entities.Watchbill.Watchbill>().List().Select(watchbill => new
                                                                                                                     {
                                                                                                                         watchbill
                                                                                                                             .Id,
                                                                                                                         watchbill
                                                                                                                             .CreatedBy,
                                                                                                                         watchbill
                                                                                                                             .CurrentState,
                                                                                                                         watchbill
                                                                                                                             .LastStateChange,
                                                                                                                         watchbill
                                                                                                                             .LastStateChangedBy,
                                                                                                                         watchbill
                                                                                                                             .Range,
                                                                                                                         watchbill
                                                                                                                             .Title,
                                                                                                                         watchbill
                                                                                                                             .EligibilityGroup
                                                                                                                     }));

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
            token.AssertLoggedIn();
            token.Args.AssertContainsKeys("title", "eligibilitygroupid", "range");

            var title = token.Args["title"] as string;

            if (!Guid.TryParse(token.Args["eligibilitygroupid"] as string, out var eligibilityGroupId))
                throw new CommandCentralException("Your eligibility group id was in the wrong format.",
                    ErrorTypes.Validation);

            var elGroup = ReferenceListHelper<WatchEligibilityGroup>.Get(eligibilityGroupId) ??
                          throw new CommandCentralException("The eligibility group did not exist.",
                              ErrorTypes.Validation);

            var range = token.Args["range"].CastJToken<TimeRange>();

            NHibernate.NHibernateUtil.Initialize(token.AuthenticationSession.Person.Command);
            var watchbillToInsert = new Entities.Watchbill.Watchbill
                                    {
                                        Command = token.AuthenticationSession.Person.Command,
                                        CreatedBy = token.AuthenticationSession.Person,
                                        CurrentState = ReferenceListHelper<WatchbillStatus>.Find("Initial"),
                                        Id = Guid.NewGuid(),
                                        LastStateChange = token.CallTime,
                                        LastStateChangedBy = token.AuthenticationSession.Person,
                                        Title = title,
                                        EligibilityGroup = elGroup,
                                        Range = range
                                    };

            var validationResult = new Entities.Watchbill.Watchbill.WatchbillValidator().Validate(watchbillToInsert);

            if (!validationResult.IsValid)
                throw new AggregateException(
                    validationResult.Errors.Select(
                        x => new CommandCentralException(x.ErrorMessage, ErrorTypes.Validation)));

            //Let's make sure the client is allowed to make a watchbill with this eligibility group.
            var ellGroup = ReferenceListHelper<WatchEligibilityGroup>.Get(watchbillToInsert.EligibilityGroup.Id) ??
                           throw new CommandCentralException("You failed to provide a proper eligibility group.",
                               ErrorTypes.Validation);

            if (token.AuthenticationSession.Person.ResolvePermissions(null)
                     .HighestLevels[ellGroup.OwningChainOfCommand] != ChainOfCommandLevels.Command)
                throw new CommandCentralException("You are not allowed to edit this watchbill.  " +
                                                  "You must have command level permissions in the related chain of command.",
                    ErrorTypes.Authorization);

            using (var session = DataAccess.DataProvider.CreateStatefulSession())
            {
                using (var transaction = session.BeginTransaction())
                {
                    try
                    {
                        watchbillToInsert.EligibilityGroup = session.Merge(watchbillToInsert.EligibilityGroup);

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
        private static void UpdateWatchbillState(MessageToken token)
        {
            token.AssertLoggedIn();
            token.Args.AssertContainsKeys("id", "stateid");

            if (!Guid.TryParse(token.Args["id"] as string, out var watchbillId))
                throw new CommandCentralException("Your watchbill id was not in the right format",
                    ErrorTypes.Validation);

            if (!Guid.TryParse(token.Args["stateid"] as string, out Guid stateId))
                throw new CommandCentralException("Your state id was not in the right format", ErrorTypes.Validation);

            //Now let's go get the watchbill from the database.
            using (var session = DataAccess.DataProvider.CreateStatefulSession())
            {
                using (var transaction = session.BeginTransaction())
                {
                    try
                    {
                        var watchbillFromDB = session.Get<Entities.Watchbill.Watchbill>(watchbillId) ??
                                              throw new CommandCentralException(
                                                  "Your watchbill's id was not valid.  Please consider creating the watchbill first.",
                                                  ErrorTypes.Validation);

                        //Ok so there's a watchbill.  Let's get the ell group to determine the permissions.
                        if (token.AuthenticationSession.Person.ResolvePermissions(null)
                                 .HighestLevels[watchbillFromDB.EligibilityGroup.OwningChainOfCommand] !=
                            ChainOfCommandLevels.Command)
                            throw new CommandCentralException("You are not allowed to edit this watchbill.  " +
                                                              "You must have command level permissions in the related chain of command.",
                                ErrorTypes.Authorization);

                        var clientState = session.Get<WatchbillStatus>(stateId) ??
                                          throw new CommandCentralException(
                                              "Your state id did not reference a real state.", ErrorTypes.Validation);

                        //If the state is different, we need to move the state as well.  There's a method for that.
                        if (watchbillFromDB.CurrentState != clientState)
                        {
                            //It looks like the client is trying to change the state.
                            watchbillFromDB.SetState(clientState, token.CallTime, token.AuthenticationSession.Person,
                                session);
                        }

                        var validationResult =
                            new Entities.Watchbill.Watchbill.WatchbillValidator().Validate(watchbillFromDB);

                        if (!validationResult.IsValid)
                            throw new AggregateException(
                                validationResult.Errors.Select(
                                    x => new CommandCentralException(x.ErrorMessage,
                                        ErrorTypes.Validation)));

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
            token.AssertLoggedIn();
            token.Args.AssertContainsKeys("id");

            if (!Guid.TryParse(token.Args["id"] as string, out Guid watchbillId))
                throw new CommandCentralException("Your id was in the wrong format.", ErrorTypes.Validation);

            //Now let's go get the watchbill from the database.
            using (var session = DataAccess.DataProvider.CreateStatefulSession())
            {
                using (var transaction = session.BeginTransaction())
                {
                    try
                    {
                        var watchbillFromDB = session.Get<Entities.Watchbill.Watchbill>(watchbillId) ??
                                              throw new CommandCentralException(
                                                  "Your watchbill's id was not valid.  Please consider creating the watchbill first.",
                                                  ErrorTypes.Validation);

                        //Ok so there's a watchbill.  Let's get the ell group to determine the permissions.
                        if (token.AuthenticationSession.Person.ResolvePermissions(null)
                                 .HighestLevels[watchbillFromDB.EligibilityGroup.OwningChainOfCommand] !=
                            ChainOfCommandLevels.Command)
                            throw new CommandCentralException("You are not allowed to edit this watchbill.  " +
                                                              "You must have command level permissions in the related chain of command.",
                                ErrorTypes.Authorization);

                        //Check the state.
                        if (watchbillFromDB.CurrentState != ReferenceListHelper<WatchbillStatus>.Find("Initial"))
                            throw new CommandCentralException(
                                "You may not delete a watchbill that is not in the initial state.  Please consider changing its state first.",
                                ErrorTypes.Validation);

                        session.Delete(watchbillFromDB);

                        if (FluentScheduler.JobManager.RunningSchedules.Any(
                            x => x.Name == watchbillFromDB.Id.ToString()))
                            FluentScheduler.JobManager.RemoveJob(watchbillFromDB.Id.ToString());

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
        /// Loads recommendations for watches on the watchbill.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        [EndpointMethod(AllowArgumentLogging = true, AllowResponseLogging = true, RequiresAuthentication = true)]
        private static void GetWatchRecommendations(MessageToken token)
        {
            token.AssertLoggedIn();
            token.Args.AssertContainsKeys("watchbillid");

            if (!Guid.TryParse(token.Args["watchbillid"] as string, out Guid watchbillId))
                throw new CommandCentralException("You watchbill id was in the wrong format.", ErrorTypes.Validation);

            using (var session = DataAccess.DataProvider.CreateStatefulSession())
            {
                var watchbill = session.Get<Entities.Watchbill.Watchbill>(watchbillId) ??
                                throw new CommandCentralException("Your watchbill id was not valid.",
                                    ErrorTypes.Validation);

                var set = new HashSet<WatchShiftType>();

                foreach (var shift in watchbill.WatchShifts)
                    set.Add(shift.ShiftType);

                Dictionary<WatchShiftType, object> result = new Dictionary<WatchShiftType, object>();

                var permissions = token.AuthenticationSession.Person.ResolvePermissions(null);

                foreach (var type in set)
                {
                    var temp = watchbill.EligibilityGroup.EligiblePersons
                                        .Where(person => type.RequiredWatchQualifications.All(
                                                   watchQual =>
                                                       person.WatchQualifications.Contains(watchQual)));

                    switch (permissions.HighestLevels[watchbill.EligibilityGroup.OwningChainOfCommand])
                    {
                        case ChainOfCommandLevels.Command:
                        {
                            temp = temp.Where(x => x.IsInSameCommandAs(token.AuthenticationSession.Person));

                            break;
                        }
                        case ChainOfCommandLevels.Department:
                        {
                            temp = temp.Where(x => x.IsInSameDepartmentAs(token.AuthenticationSession.Person));

                            break;
                        }
                        case ChainOfCommandLevels.Division:
                        {
                            temp = temp.Where(x => x.IsInSameDivisionAs(token.AuthenticationSession.Person));

                            break;
                        }
                        case ChainOfCommandLevels.None:
                        case ChainOfCommandLevels.Self:
                        {
                            throw new CommandCentralException("You aren't allowed to see watch recommendations.",
                                ErrorTypes.Authorization);
                        }
                        default:
                            throw new NotImplementedException();
                    }

                    var selection = temp.GroupBy(x => x.Division).Select(x =>
                                                                         {
                                                                             return new
                                                                                    {
                                                                                        Division = x.Key.Id,
                                                                                        RecommendationsByPerson = x
                                                                                            .ToList().Select(person =>
                                                                                                             {
                                                                                                                 double
                                                                                                                     points
                                                                                                                         = person
                                                                                                                             .WatchAssignments
                                                                                                                             .Sum(
                                                                                                                                 z =>
                                                                                                                                 {
                                                                                                                                     int
                                                                                                                                         totalMonths
                                                                                                                                             = (
                                                                                                                                                 int
                                                                                                                                             ) Math
                                                                                                                                                 .Round(
                                                                                                                                                     DateTime
                                                                                                                                                         .UtcNow
                                                                                                                                                         .Subtract(
                                                                                                                                                             z.WatchShift
                                                                                                                                                              .Range
                                                                                                                                                              .Start)
                                                                                                                                                         .TotalDays /
                                                                                                                                                     (365.2425 /
                                                                                                                                                      12
                                                                                                                                                     ));

                                                                                                                                     return
                                                                                                                                         z.WatchShift
                                                                                                                                          .Points /
                                                                                                                                         (Math
                                                                                                                                              .Pow(
                                                                                                                                                  1.35,
                                                                                                                                                  totalMonths) +
                                                                                                                                          -1
                                                                                                                                         );
                                                                                                                                 });

                                                                                                                 var
                                                                                                                     watchInputs
                                                                                                                         = watchbill
                                                                                                                             .WatchInputs
                                                                                                                             .Where(
                                                                                                                                 input =>
                                                                                                                                     input
                                                                                                                                         .IsConfirmed &&
                                                                                                                                     input
                                                                                                                                         .Person
                                                                                                                                         .Id ==
                                                                                                                                     person
                                                                                                                                         .Id)
                                                                                                                             .ToList();

                                                                                                                 var
                                                                                                                     mostRecentWatch
                                                                                                                         = person
                                                                                                                             .WatchAssignments
                                                                                                                             .OrderByDescending(
                                                                                                                                 ass =>
                                                                                                                                     ass
                                                                                                                                         .WatchShift
                                                                                                                                         .Range
                                                                                                                                         .Start)
                                                                                                                             .FirstOrDefault();

                                                                                                                 return
                                                                                                                     new
                                                                                                                     {
                                                                                                                         Person
                                                                                                                         = person,
                                                                                                                         Points
                                                                                                                         = points,
                                                                                                                         WatchInputs
                                                                                                                         = watchInputs,
                                                                                                                         MostRecentWatchAssignment
                                                                                                                         = (
                                                                                                                             mostRecentWatch !=
                                                                                                                             null
                                                                                                                         )
                                                                                                                             ? new
                                                                                                                               {
                                                                                                                                   mostRecentWatch
                                                                                                                                       .AcknowledgedBy,
                                                                                                                                   mostRecentWatch
                                                                                                                                       .AssignedBy,
                                                                                                                                   mostRecentWatch
                                                                                                                                       .DateAcknowledged,
                                                                                                                                   mostRecentWatch
                                                                                                                                       .DateAssigned,
                                                                                                                                   mostRecentWatch
                                                                                                                                       .Id,
                                                                                                                                   mostRecentWatch
                                                                                                                                       .IsAcknowledged,
                                                                                                                                   mostRecentWatch
                                                                                                                                       .NumberOfAlertsSent,
                                                                                                                                   mostRecentWatch
                                                                                                                                       .PersonAssigned,
                                                                                                                                   WatchShift
                                                                                                                                   = new
                                                                                                                                     {
                                                                                                                                         mostRecentWatch
                                                                                                                                             .WatchShift
                                                                                                                                             .Id,
                                                                                                                                         mostRecentWatch
                                                                                                                                             .WatchShift
                                                                                                                                             .Comments,
                                                                                                                                         DivisionId
                                                                                                                                         = mostRecentWatch
                                                                                                                                             .WatchShift
                                                                                                                                             .DivisionAssignedTo
                                                                                                                                             ?.Id,
                                                                                                                                         mostRecentWatch
                                                                                                                                             .WatchShift
                                                                                                                                             .Points,
                                                                                                                                         mostRecentWatch
                                                                                                                                             .WatchShift
                                                                                                                                             .Range,
                                                                                                                                         mostRecentWatch
                                                                                                                                             .WatchShift
                                                                                                                                             .ShiftType,
                                                                                                                                         mostRecentWatch
                                                                                                                                             .WatchShift
                                                                                                                                             .Title
                                                                                                                                     }
                                                                                                                               }
                                                                                                                             : null
                                                                                                                     };
                                                                                                             }).ToList()
                                                                                    };
                                                                         }).SelectMany(
                        x => x.RecommendationsByPerson.Select(
                            y => new {y.Person, y.Points, y.WatchInputs}));

                    result[type] = selection;
                }

                token.SetResult(result);
            }
        }
    }
}