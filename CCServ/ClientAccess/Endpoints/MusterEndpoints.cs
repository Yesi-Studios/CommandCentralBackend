using System;
using System.Collections.Generic;
using CCServ.ClientAccess;
using FluentNHibernate.Mapping;
using AtwoodUtils;
using System.Linq;
using NHibernate.Criterion;
using CCServ.Authorization;
using CCServ.Logging;
using CCServ.Entities;
using Newtonsoft.Json.Linq;
using CCServ.Entities.ReferenceLists;

namespace CCServ.ClientAccess.Endpoints
{
    static class MusterEndpoints
    {

        /// <summary>
        /// WARNING!  THIS METHOD IS EXPOSED TO THE CLIENT AND IS NOT INTENDED FOR INTERNAL USE.  AUTHENTICATION, AUTHORIZATION AND VALIDATION MUST BE HANDLED PRIOR TO DB INTERACTION.
        /// <para />
        /// Loads all muster records for a given muster date. This will be converted to a muster date based on the rollover time shift.  Recommend that you submit the date time without a time portion or with the time portion set to midnight - although it doesn't matter.
        /// <para />
        /// Client Parameters: <para />
        ///     musterdate - The date for which to load muster records. Keep in mind, asking for muster records for a time after the roll over time will in fact return the next day's muster records.  This is due to the rollover time shift.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        [EndpointMethod(AllowArgumentLogging = true, AllowResponseLogging = true, RequiresAuthentication = true)]
        private static void LoadMusterRecordsByMusterDay(MessageToken token)
        {
            //Just make sure the client is logged in.  The endpoint's description should've handled this but you never know.
            if (token.AuthenticationSession == null)
                throw new CommandCentralException("You must be logged in to do that.", HttpStatusCodes.AuthenticationFailed);

            token.Args.AssertContainsKeys("musterdate");

            DateTime musterDate;
            if (!(token.Args["musterdate"] is DateTime))
            {
                throw new CommandCentralException("Your 'musterdate' parameter was not in a valid format.", HttpStatusCodes.BadRequest);
            }
            else
            {
                //Here we set the date time to .Date.  This strips off the time component.
                musterDate = ((DateTime)token.Args["musterdate"]).Date;
            }

            using (var session = DataAccess.NHibernateHelper.CreateStatefulSession())
            {
                //Get all records where the day of the year is the given day's muster day and the year the same.
                var records = session.QueryOver<MusterRecord>().Where(x => x.MusterDate == musterDate.Date).List();
                token.SetResult(records.Select(x => new
                {
                    x.Command,
                    x.Department,
                    x.Division,
                    x.DutyStatus,
                    x.HasBeenSubmitted,
                    x.Id,
                    Musteree = x.Musteree == null ? null : x.Musteree,
                    Musterer = x.Musterer == null ? null : x.Musterer,
                    x.MusterStatus,
                    x.MusterDate,
                    x.Paygrade,
                    x.SubmitTime,
                    x.UIC,
                    x.Remarks,
                    x.Designation
                }));
            }
        }

        /// <summary>
        /// WARNING!  THIS METHOD IS EXPOSED TO THE CLIENT AND IS NOT INTENDED FOR INTERNAL USE.  AUTHENTICATION, AUTHORIZATION AND VALIDATION MUST BE HANDLED PRIOR TO DB INTERACTION.
        /// <para />
        /// Given a dictionary of personIds/MusterStatuses, attempts to submit muster for all persons, failing if a person doesn't exist for the given Id, or if the client can't submit muster for any one of the persons.  If a person has already been mustered for this day, that person is not re-mustered.  All persons who were mustered, their Ids will be returned.
        /// <para />
        /// Options: <para />
        ///     mustersubmissions - A dictionary where the key is the person's Id, and the value is the MusterStatus to assign to this person.  The muster status should be a full muster status object.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        [EndpointMethod(AllowArgumentLogging = true, AllowResponseLogging = true, RequiresAuthentication = true)]
        private static void SubmitMuster(MessageToken token)
        {
            //Just make sure the client is logged in.  The endpoint's description should've handled this but you never know.
            if (token.AuthenticationSession == null)
                throw new CommandCentralException("You must be logged in to do that.", HttpStatusCodes.AuthenticationFailed);

            token.Args.AssertContainsKeys("mustersubmissions");

            if (ServiceManagement.ServiceManager.CurrentConfigState.IsMusterFinalized)
            {
                throw new CommandCentralException("The current muster is closed.  The muster will reopen at {0}.".FormatS(ServiceManagement.ServiceManager.CurrentConfigState.MusterRolloverTime), HttpStatusCodes.Forbidden);
            }

            Dictionary<Guid, JToken> musterSubmissions = null;
            try
            {
                //TODO: why the fuck did I do this?
                musterSubmissions = token.Args["mustersubmissions"].CastJToken<Dictionary<Guid, string>>()
                    .Select(x => new KeyValuePair<Guid, JToken>(x.Key, new { status = x.Value, remarks = "" }.Serialize().DeserializeToJObject()))
                    .ToDictionary(x => x.Key, x => x.Value);
            }
            catch (Exception e)
            {
                throw new CommandCentralException("There was an error while trying to format your 'mustersubmissions' argument.  " +
                    "It should be sent in a JSON dictionary.  Parsing error details: {0}".FormatS(e.Message), HttpStatusCodes.BadRequest);
            }

            //Validate the muster statuses
            if (musterSubmissions.Values.Any(x => !MusterStatuses.AllMusterStatuses.Any(y => y.Value.SafeEquals(x.Value<string>("status")))))
            {
                throw new CommandCentralException("One or more requested muster statuses were not valid.", HttpStatusCodes.BadRequest);
            }

            //This is the session in which we're going to do our muster updates.  We do it separately in case something terrible happens to the currently logged in user.
            //This means that if the currently logged in person updates their own muster then for the rest of this request, their muster will be invalid.  That's ok cause we shouldn't need it.
            using (var session = DataAccess.NHibernateHelper.CreateStatefulSession())
            using (var transaction = session.BeginTransaction())
            {
                //Submit the query to load all the persons.  How fucking easy can this be.  Fuck off NHibernate.  
                var persons = session.QueryOver<Person>().AndRestrictionOn(x => x.Id).IsIn(musterSubmissions.Keys).List();

                //Now we need to make sure the client is allowed to muster the persons the client wants to muster.
                if (persons.Any(x => !MusterRecord.CanClientMusterPerson(token.AuthenticationSession.Person, x)))
                    throw new CommandCentralException("You were not authorized to muster one or more of the persons you tried to muster.", HttpStatusCodes.Unauthorized);

                //Ok, the client is allowed to muster them.  Now we need to set their current muster statuses.
                for (int x = 0; x < persons.Count; x++)
                {
                    //If the client doesn't have a current muster status, and their duty status isn't Loss, then give them a muster status. 
                    if (persons[x].CurrentMusterRecord == null)
                    {
                        if (persons[x].DutyStatus == DutyStatuses.Loss)
                        {
                            throw new CommandCentralException("You may not muster {0} because his/her duty status is set to Loss.".FormatS(persons[x].ToString()), HttpStatusCodes.BadRequest);
                        }
                        else
                        {
                            //If the person's duty status is NOT Loss, then somehow this person never got a muster record.  This isn't good.
                            if (persons[x].CurrentMusterRecord == null)
                                throw new Exception("{0}'s current muster status was null.  This is unexpected.".FormatS(persons[x].ToString()));
                        }
                    }

                    persons[x].CurrentMusterRecord.HasBeenSubmitted = true;
                    persons[x].CurrentMusterRecord.MusterDate = MusterRecord.GetMusterDate(token.CallTime);
                    persons[x].CurrentMusterRecord.Musterer = token.AuthenticationSession.Person;
                    persons[x].CurrentMusterRecord.MusterStatus = MusterStatuses.AllMusterStatuses.First(y => y.Value.SafeEquals(musterSubmissions.First(k => k.Key == persons[x].Id).Value.Value<string>("status"))).Value;
                    persons[x].CurrentMusterRecord.SubmitTime = token.CallTime;
                    persons[x].CurrentMusterRecord.Remarks = musterSubmissions.ElementAt(x).Value.Value<string>("remarks");

                    //And once we're done resetting their current muster status, let's update them.
                    session.Update(persons[x]);
                }

                //And then commit the transaction if it all went well.
                transaction.Commit();
            }
        }

        /// <summary>
        /// WARNING!  THIS METHOD IS EXPOSED TO THE CLIENT AND IS NOT INTENDED FOR INTERNAL USE.  AUTHENTICATION, AUTHORIZATION AND VALIDATION MUST BE HANDLED PRIOR TO DB INTERACTION.
        /// <para />
        /// Loads the current day's muster and returns to the client the current day and year, the roll over hours we used to determine the muster and all persons the client is allowed to muster.
        /// <para />
        /// Client Parameters: <para />
        ///     None
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        [EndpointMethod(AllowArgumentLogging = true, AllowResponseLogging = true, RequiresAuthentication = true)]
        private static void LoadMusterablePersonsForToday(MessageToken token)
        {
            //Just make sure the client is logged in.  The endpoint's description should've handled this but you never know.
            if (token.AuthenticationSession == null)
                throw new CommandCentralException("You must be logged in to do that.", HttpStatusCodes.AuthenticationFailed);

            //Where we're going to keep all the persons the client can muster.
            List<Person> musterablePersons = new List<Person>();

            var resolvedPermissions = token.AuthenticationSession.Person.PermissionGroups.Resolve(token.AuthenticationSession.Person, null);
            ChainOfCommandLevels highestLevelInMuster;
            if (!resolvedPermissions.HighestLevels.TryGetValue("Muster", out highestLevelInMuster))
                highestLevelInMuster = ChainOfCommandLevels.None;

            using (var session = DataAccess.NHibernateHelper.CreateStatefulSession())
            {
                //Hold off on submitting the query for now because we need to know who we're looking for. People in the person's command, department or division.
                var queryOver = MusterRecord.GetMusterablePersonsQuery(session)
                    .Fetch(x => x.CurrentMusterRecord).Eager;

                //Switch on the highest level in muster and then add the query accordingly.
                switch (highestLevelInMuster)
                {
                    case ChainOfCommandLevels.Command:
                        {
                            queryOver = queryOver.Where(x => x.Command == token.AuthenticationSession.Person.Command);
                            musterablePersons = queryOver.List().ToList();
                            break;
                        }
                    case ChainOfCommandLevels.Department:
                        {
                            queryOver = queryOver.Where(x => x.Department == token.AuthenticationSession.Person.Department);
                            musterablePersons = queryOver.List().ToList();
                            break;
                        }
                    case ChainOfCommandLevels.Division:
                        {
                            queryOver = queryOver.Where(x => x.Division == token.AuthenticationSession.Person.Division);
                            musterablePersons = queryOver.List().ToList();
                            break;
                        }
                    case ChainOfCommandLevels.Self:
                        {
                            //If the client's highest level is 'Self' then they can only muster themselves.
                            musterablePersons.Add(token.AuthenticationSession.Person);
                            break;
                        }
                    case ChainOfCommandLevels.None:
                        {
                            //If the client's highest level is none, then they basically got their permissions taken away.
                            break;
                        }
                    default:
                        {
                            throw new Exception("The default case in the highest level switch in the LoadMusterablePersonForToday endpoint was reached with the following case: '{0}'!".FormatS(highestLevelInMuster));
                        }
                }

                //Here we also want to limit the musterable persons query to only those people   

                //Now that we have the results from the database, let's project them into our results.  This won't be the final DTO, we're going to layer on some additional information for the client to use.
                //Because Atwood is a good code monkey. Oh yes he is.
                var results = musterablePersons.Select(x =>
                {
                    //If the person's current muster status is null, throw an error.  This is not expected.
                    if (x.CurrentMusterRecord == null)
                        throw new Exception("{0}'s current muster status is null!".FormatS(x.ToString()));

                    return new
                    {
                        Id = x.Id,
                        FirstName = x.FirstName,
                        MiddleName = x.MiddleName,
                        LastName = x.LastName,
                        Paygrade = x.Paygrade == null ? "" : x.Paygrade.ToString(),
                        Designation = x.Designation == null ? "" : x.Designation.ToString(), //Designation can be null.
                        Division = x.Division == null ? "" : x.Division.ToString(),
                        Department = x.Department == null ? "" : x.Department.ToString(),
                        Command = x.Command == null ? "" : x.Command.ToString(),
                        UIC = x.UIC == null ? "" : x.UIC.ToString(), //UIC can also be null.
                        FriendlyName = x.ToString(),
                        CurrentMusterStatus = new
                        {
                            x.CurrentMusterRecord.Command,
                            x.CurrentMusterRecord.Department,
                            x.CurrentMusterRecord.Division,
                            x.CurrentMusterRecord.DutyStatus,
                            x.CurrentMusterRecord.HasBeenSubmitted,
                            x.CurrentMusterRecord.Id,
                            Musteree = x.CurrentMusterRecord.Musteree,
                            Musterer = x.CurrentMusterRecord.Musterer == null ? null : x.CurrentMusterRecord.Musterer,
                            x.CurrentMusterRecord.MusterStatus,
                            x.CurrentMusterRecord.MusterDate,
                            x.CurrentMusterRecord.Paygrade,
                            x.CurrentMusterRecord.SubmitTime,
                            x.CurrentMusterRecord.UIC,
                            x.CurrentMusterRecord.Remarks,
                            x.CurrentMusterRecord.Designation
                        },
                        CanMuster = MusterRecord.CanClientMusterPerson(token.AuthenticationSession.Person, x),
                        HasBeenMustered = x.CurrentMusterRecord.HasBeenSubmitted
                    };
                });

                //And now build the final DTO that's going out the door.
                token.SetResult(new
                {
                    MusterFinalized = ServiceManagement.ServiceManager.CurrentConfigState.IsMusterFinalized,
                    CurrentDate = MusterRecord.GetMusterDate(token.CallTime),
                    Musters = results,
                    RolloverTime = ServiceManagement.ServiceManager.CurrentConfigState.MusterRolloverTime.ToString(),
                    ExpectedCompletionTime = ServiceManagement.ServiceManager.CurrentConfigState.MusterDueTime.ToString()
                });
            }
        }

        /// <summary>
        /// WARNING!  THIS METHOD IS EXPOSED TO THE CLIENT AND IS NOT INTENDED FOR INTERNAL USE.  AUTHENTICATION, AUTHORIZATION AND VALIDATION MUST BE HANDLED PRIOR TO DB INTERACTION.
        /// <para />
        /// Finalized the current muster, which sets a flag and prevents all any more muster submissions.
        /// <para />
        /// Client Parameters: <para />
        ///     None
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        [EndpointMethod(AllowResponseLogging = true, AllowArgumentLogging = true, RequiresAuthentication = true)]
        private static void FinalizeMuster(MessageToken token)
        {
            //Just make sure the client is logged in.  The endpoint's description should've handled this but you never know.
            if (token.AuthenticationSession == null)
                throw new CommandCentralException("You must be logged in to do that.", HttpStatusCodes.AuthenticationFailed);

            if (!token.AuthenticationSession.Person.PermissionGroups.CanAccessSubmodules(SubModules.AdminTools.ToString()))
                throw new CommandCentralException("You are not authorized to finalize muster.", HttpStatusCodes.BadRequest);

            //Ok we have permission, let's make sure the muster hasn't already been finalized.
            if (ServiceManagement.ServiceManager.CurrentConfigState.IsMusterFinalized)
                throw new CommandCentralException("The muster has already been finalized.", HttpStatusCodes.Forbidden);

            //So we should be good to finalize the muster.
            MusterRecord.FinalizeMuster(token);
        }
    }
}
