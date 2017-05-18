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
            token.AssertLoggedIn();
            token.Args.AssertContainsKeys("id");

            if (!Guid.TryParse(token.Args["id"] as string, out Guid watchbillId))
                throw new CommandCentralException("Your watchbill id parameter's format was invalid.", ErrorTypes.Validation);

            bool doPopulation = false;
            if (token.Args.ContainsKey("dopopulation"))
            {
                bool? test = token.Args["dopopulation"] as bool?;
                if (test.HasValue)
                    doPopulation = test.Value;
                else
                    throw new CommandCentralException("Your 'dopopulation' parameter was in an invalid format.", ErrorTypes.Validation);
            }

            //Now let's go get the watchbill from the database.
            using (var session = DataAccess.NHibernateHelper.CreateStatefulSession())
            {
                var watchbillFromDB = session.Get<Entities.Watchbill.Watchbill>(watchbillId) ??
                            throw new CommandCentralException("Your watchbill's id was not valid.  Please consider creating the watchbill first.", ErrorTypes.Validation);

                if (doPopulation)
                {
                    //Make sure the client is allowed to.  It's not actually a security issue if the client does the population,
                    //but we may as well restrict it because the population method is very expensive.
                    if (!token.AuthenticationSession.Person.PermissionGroups.Any(x => x.ChainsOfCommandMemberOf.Contains(watchbillFromDB.EligibilityGroup.OwningChainOfCommand) && x.AccessLevel == ChainOfCommandLevels.Command))
                        throw new CommandCentralException("You are not allowed to edit this watchbill.  You must have command level permissions in the related chain of command.", ErrorTypes.Authorization);

                    //And make sure we're at a state where population can occur.
                    if (watchbillFromDB.CurrentState != Entities.ReferenceLists.Watchbill.WatchbillStatuses.ClosedForInputs)
                        throw new CommandCentralException("You may not populate this watchbill - a watchbill must be in the Closed for Inputs state in order to populate it.", ErrorTypes.Validation);

                    watchbillFromDB.PopulateWatchbill(token.AuthenticationSession.Person, token.CallTime);
                }

                //We're also going to go see who is in the eligibility group and has no watch qualifications that pertain to this watchbill.
                //First we need to know all the possible needed watch qualifications.
                var watchQualifications = watchbillFromDB.WatchShifts.SelectMany(x => x.ShiftType.RequiredWatchQualifications);

                var personsWithoutAtLeastOneQualification = watchbillFromDB.EligibilityGroup.EligiblePersons
                    .Where(person => !person.WatchQualifications.Any(qual => watchQualifications.Contains(qual)));

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
                    WatchShifts = watchbillFromDB.WatchShifts.Select(shift =>
                    {
                        return new WatchShift
                        {
                            Comments = shift.Comments,
                            Id = shift.Id,
                            Points = shift.Points,
                            Range = shift.Range,
                            ShiftType = shift.ShiftType,
                            Title = shift.Title,
                            WatchAssignment = shift.WatchAssignment,
                            Watchbill = new Entities.Watchbill.Watchbill { Id = shift.Watchbill.Id }
                        };
                    }),
                    watchbillFromDB.WatchInputs,
                    NotQualledPersons = personsWithoutAtLeastOneQualification
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
            token.AssertLoggedIn();
            token.Args.AssertContainsKeys("title", "eligibilitygroupid", "range");

            var title = token.Args["title"] as string;

            if (!Guid.TryParse(token.Args["eligibilitygroupid"] as string, out Guid eligibilityGroupId))
                throw new CommandCentralException("Your eligibility group id was in the wrong format.", ErrorTypes.Validation);

            var elGroup = Entities.ReferenceLists.Watchbill.WatchEligibilityGroups.AllWatchEligibilityGroups
                .FirstOrDefault(x => Guid.Equals(x.Id, eligibilityGroupId)) ??
                throw new CommandCentralException("The eligibility group did not exist.", ErrorTypes.Validation);

            var range = token.Args["range"].CastJToken<TimeRange>();

            NHibernate.NHibernateUtil.Initialize(token.AuthenticationSession.Person.Command);
            Entities.Watchbill.Watchbill watchbillToInsert = new Entities.Watchbill.Watchbill
            {
                Command = token.AuthenticationSession.Person.Command,
                CreatedBy = token.AuthenticationSession.Person,
                CurrentState = Entities.ReferenceLists.Watchbill.WatchbillStatuses.Initial,
                Id = Guid.NewGuid(),
                LastStateChange = token.CallTime,
                LastStateChangedBy = token.AuthenticationSession.Person,
                Title = title,
                EligibilityGroup = elGroup,
                Range = range
            };

            var validationResult = new Entities.Watchbill.Watchbill.WatchbillValidator().Validate(watchbillToInsert);

            if (!validationResult.IsValid)
                throw new AggregateException(validationResult.Errors.Select(x => new CommandCentralException(x.ErrorMessage, ErrorTypes.Validation)));

            //Let's make sure the client is allowed to make a watchbill with this eligibility group.
            var ellGroup = Entities.ReferenceLists.Watchbill.WatchEligibilityGroups.AllWatchEligibilityGroups.FirstOrDefault(x => Guid.Equals(x.Id, watchbillToInsert.EligibilityGroup.Id)) ??
                throw new CommandCentralException("You failed to provide a proper eligibilty group.", ErrorTypes.Validation);

            if (!token.AuthenticationSession.Person.PermissionGroups.Any(x => x.ChainsOfCommandMemberOf.Contains(ellGroup.OwningChainOfCommand) && x.AccessLevel == ChainOfCommandLevels.Command))
                throw new CommandCentralException("You are not allowed to create a watchbill tied to that eligibility group.  " +
                    "You must have command level permissions in the related chain of command.", ErrorTypes.Authorization);

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
        private static void UpdateWatchbillState(MessageToken token)
        {
            token.AssertLoggedIn();
            token.Args.AssertContainsKeys("id", "stateid");

            if (!Guid.TryParse(token.Args["id"] as string, out Guid watchbillId))
                throw new CommandCentralException("Your watchbill id was not in the right format", ErrorTypes.Validation);

            if (!Guid.TryParse(token.Args["stateid"] as string, out Guid stateId))
                throw new CommandCentralException("Your state id was not in the right format", ErrorTypes.Validation);

            //Now let's go get the watchbill from the database.
            using (var session = DataAccess.NHibernateHelper.CreateStatefulSession())
            {
                using (var transaction = session.BeginTransaction())
                {
                    try
                    {
                        var watchbillFromDB = session.Get<Entities.Watchbill.Watchbill>(watchbillId) ??
                            throw new CommandCentralException("Your watchbill's id was not valid.  Please consider creating the watchbill first.", ErrorTypes.Validation);

                        //Ok so there's a watchbill.  Let's get the ell group to determine the permissions.
                        if (!token.AuthenticationSession.Person.PermissionGroups.Any(x => x.ChainsOfCommandMemberOf.Contains(watchbillFromDB.EligibilityGroup.OwningChainOfCommand) && x.AccessLevel == ChainOfCommandLevels.Command))
                            throw new CommandCentralException("You are not allowed to edit this watchbill.  " +
                                "You must have command level permissions in the related chain of command.", ErrorTypes.Authorization);

                        var clientState = session.Get<Entities.ReferenceLists.Watchbill.WatchbillStatus>(stateId) ??
                            throw new CommandCentralException("Your state id did not reference a real state.", ErrorTypes.Validation);

                        //If the state is different, we need to move the state as well.  There's a method for that.
                        if (watchbillFromDB.CurrentState != clientState)
                        {
                            //It looks like the client is trying to change the state.
                            watchbillFromDB.SetState(clientState, token.CallTime, token.AuthenticationSession.Person, session);
                        }

                        var validationResult = new Entities.Watchbill.Watchbill.WatchbillValidator().Validate(watchbillFromDB);

                        if (!validationResult.IsValid)
                            throw new AggregateException(validationResult.Errors.Select(x => new CommandCentralException(x.ErrorMessage, ErrorTypes.Validation)));

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
            using (var session = DataAccess.NHibernateHelper.CreateStatefulSession())
            {
                using (var transaction = session.BeginTransaction())
                {
                    try
                    {
                        var watchbillFromDB = session.Get<Entities.Watchbill.Watchbill>(watchbillId) ??
                            throw new CommandCentralException("Your watchbill's id was not valid.  Please consider creating the watchbill first.", ErrorTypes.Validation);

                        //Ok so there's a watchbill.  Let's get the ell group to determine the permissions.
                        if (!token.AuthenticationSession.Person.PermissionGroups.Any(x => x.ChainsOfCommandMemberOf.Contains(watchbillFromDB.EligibilityGroup.OwningChainOfCommand) && x.AccessLevel == ChainOfCommandLevels.Command))
                            throw new CommandCentralException("You are not allowed to edit this watchbill.  " +
                                "You must have command level permissions in the related chain of command.", ErrorTypes.Authorization);

                        //Check the state.
                        if (watchbillFromDB.CurrentState != Entities.ReferenceLists.Watchbill.WatchbillStatuses.Initial)
                            throw new CommandCentralException("You may not delete a watchbill that is not in the initial state.  Please consider changing its state first.", ErrorTypes.Validation);

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
