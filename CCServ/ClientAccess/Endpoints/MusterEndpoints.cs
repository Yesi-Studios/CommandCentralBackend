﻿using System;
using System.Collections.Generic;
using CCServ.ClientAccess;
using FluentNHibernate.Mapping;
using AtwoodUtils;
using System.Linq;
using NHibernate.Criterion;
using CCServ.Authorization;
using CCServ.Logging;
using CCServ.Entities.Muster;
using CCServ.Entities;

namespace CCServ.ClientAccess.Endpoints
{
    static class MusterEndpoints
    {
        #region Client Access

        /// <summary>
        /// WARNING!  THIS METHOD IS EXPOSED TO THE CLIENT AND IS NOT INTENDED FOR INTERNAL USE.  AUTHENTICATION, AUTHORIZATION AND VALIDATION MUST BE HANDLED PRIOR TO DB INTERACTION.
        /// <para />
        /// Loads a muster record for a given id and returns null if none exists.
        /// <para />
        /// Client Parameters: <para />
        ///     musterrecordid - the Id of the muster record we want to load.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        [EndpointMethod(EndpointName = "LoadMusterRecord", AllowArgumentLogging = true, AllowResponseLogging = true, RequiresAuthentication = true)]
        private static void EndpointMethod_LoadMusterRecord(MessageToken token)
        {
            if (token.AuthenticationSession == null)
            {
                token.AddErrorMessage("You must be logged in to view muster records.", ErrorTypes.Authentication, System.Net.HttpStatusCode.Unauthorized);
                return;
            }

            if (!token.Args.ContainsKey("musterrecordid"))
            {
                token.AddErrorMessage("You must send a 'musterrecordid' parameter.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                return;
            }

            Guid musterRecordId;
            if (!Guid.TryParse(token.Args["musterrecordid"] as string, out musterRecordId))
            {
                token.AddErrorMessage("Your muster record id was not legitsky.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                return;
            }

            using (var session = DataAccess.NHibernateHelper.CreateStatefulSession())
            {
                var musterRecord = session.Get<MusterRecord>(musterRecordId);

                if (musterRecord == null)
                {
                    token.SetResult(null);
                    return;
                }

                //Create a DTO for the client.
                token.SetResult(new
                {
                    musterRecord.Command,
                    musterRecord.Department,
                    musterRecord.Division,
                    musterRecord.DutyStatus,
                    musterRecord.HasBeenSubmitted,
                    musterRecord.Id,
                    musterRecord.MusterDayOfYear,
                    Musteree = musterRecord.Musteree.ToBasicPerson(),
                    Musterer = musterRecord.Musterer.ToBasicPerson(),
                    musterRecord.MusterStatus,
                    musterRecord.MusterYear,
                    musterRecord.Paygrade,
                    musterRecord.SubmitTime,
                    musterRecord.UIC
                });
            }
        }

        /// <summary>
        /// WARNING!  THIS METHOD IS EXPOSED TO THE CLIENT AND IS NOT INTENDED FOR INTERNAL USE.  AUTHENTICATION, AUTHORIZATION AND VALIDATION MUST BE HANDLED PRIOR TO DB INTERACTION.
        /// <para />
        /// Loads all muster records for a given person in which the person was the Musteree - the one being le mustered.  
        /// <para />
        /// Client Parameters: <para />
        ///     mustereeId - the Id of the person for whom to load muster records where the person is the one being mustered.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        [EndpointMethod(EndpointName = "LoadMusterRecordsByMusteree", AllowArgumentLogging = true, AllowResponseLogging = true, RequiresAuthentication = true)]
        private static void EndpointMethod_LoadMusterRecordsByMusteree(MessageToken token)
        {
            if (token.AuthenticationSession == null)
            {
                token.AddErrorMessage("You must be logged in to view muster records.", ErrorTypes.Authentication, System.Net.HttpStatusCode.Unauthorized);
                return;
            }

            if (!token.Args.ContainsKey("mustereeid"))
            {
                token.AddErrorMessage("You must send a 'mustereeid' parameter.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                return;
            }

            Guid mustereeId;
            if (!Guid.TryParse(token.Args["mustereeid"] as string, out mustereeId))
            {
                token.AddErrorMessage("Your 'mustereeid' parameter was not in a valid format.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                return;
            }

            using (var session = DataAccess.NHibernateHelper.CreateStatefulSession())
            {
                //Set the result.
                token.SetResult(session.QueryOver<MusterRecord>().Where(x => x.Musteree.Id == mustereeId).List().Select(x => new
                {
                    x.Command,
                    x.Department,
                    x.Division,
                    x.DutyStatus,
                    x.HasBeenSubmitted,
                    x.Id,
                    x.MusterDayOfYear,
                    Musteree = x.Musteree.ToBasicPerson(),
                    Musterer = x.Musterer.ToBasicPerson(),
                    x.MusterStatus,
                    x.MusterYear,
                    x.Paygrade,
                    x.SubmitTime,
                    x.UIC
                }));
            }
        }

        /// <summary>
        /// WARNING!  THIS METHOD IS EXPOSED TO THE CLIENT AND IS NOT INTENDED FOR INTERNAL USE.  AUTHENTICATION, AUTHORIZATION AND VALIDATION MUST BE HANDLED PRIOR TO DB INTERACTION.
        /// <para />
        /// Loads all muster records for a given person in which the person was the Musterer - the one doing le mustering.  
        /// <para />
        /// Client Parameters: <para />
        ///     mustererId - the Id of the person for whom to load muster records where the person is the one doing the mustering.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        [EndpointMethod(EndpointName = "LoadMusterRecordsByMusterer", AllowArgumentLogging = true, AllowResponseLogging = true, RequiresAuthentication = true)]
        private static void EndpointMethod_LoadMusterRecordsByMusterer(MessageToken token)
        {
            if (token.AuthenticationSession == null)
            {
                token.AddErrorMessage("You must be logged in to view muster records.", ErrorTypes.Authentication, System.Net.HttpStatusCode.Unauthorized);
                return;
            }

            if (!token.Args.ContainsKey("mustererid"))
            {
                token.AddErrorMessage("You must send a 'mustererid' parameter.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                return;
            }

            Guid mustererId;
            if (!Guid.TryParse(token.Args["mustererid"] as string, out mustererId))
            {
                token.AddErrorMessage("Your 'mustererid' parameter was not in a valid format.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                return;
            }

            using (var session = DataAccess.NHibernateHelper.CreateStatefulSession())
            {
                //Set the result.
                token.SetResult(session.QueryOver<MusterRecord>().Where(x => x.Musterer.Id == mustererId).List().Select(x => new
                {
                    x.Command,
                    x.Department,
                    x.Division,
                    x.DutyStatus,
                    x.HasBeenSubmitted,
                    x.Id,
                    x.MusterDayOfYear,
                    Musteree = x.Musteree.ToBasicPerson(),
                    Musterer = x.Musterer.ToBasicPerson(),
                    x.MusterStatus,
                    x.MusterYear,
                    x.Paygrade,
                    x.SubmitTime,
                    x.UIC
                }));
            }
        }

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
        [EndpointMethod(EndpointName = "LoadMusterRecordsByMusterDay", AllowArgumentLogging = true, AllowResponseLogging = true, RequiresAuthentication = true)]
        private static void EndpointMethod_LoadMusterRecordsByMusterDay(MessageToken token)
        {
            if (token.AuthenticationSession == null)
            {
                token.AddErrorMessage("You must be logged in to view muster records.", ErrorTypes.Authentication, System.Net.HttpStatusCode.Unauthorized);
                return;
            }

            if (!token.Args.ContainsKey("musterdate"))
            {
                token.AddErrorMessage("You must send a 'musterdate' parameter.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                return;
            }

            DateTime musterDate;
            if (!(token.Args["musterdate"] is DateTime))
            {
                token.AddErrorMessage("Your 'musterdate' parameter was not in a valid format.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                return;
            }
            else
            {
                //Here we set the date time to .Date.  This strips off the time component.
                musterDate = ((DateTime)token.Args["musterdate"]).Date;
            }

            using (var session = DataAccess.NHibernateHelper.CreateStatefulSession())
            {
                //Get all records where the day of the year is the given day's muster day and the year the same.
                var records = session.QueryOver<MusterRecord>().Where(x => x.MusterDayOfYear == MusterRecord.GetMusterDay(musterDate) && x.MusterYear == musterDate.Year).List();
                token.SetResult(records.Select(x => new
                {
                    x.Command,
                    x.Department,
                    x.Division,
                    x.DutyStatus,
                    x.HasBeenSubmitted,
                    x.Id,
                    x.MusterDayOfYear,
                    Musteree = x.Musteree.ToBasicPerson(),
                    Musterer = x.Musterer == null ? null : x.Musterer.ToBasicPerson(),
                    x.MusterStatus,
                    x.MusterYear,
                    x.Paygrade,
                    x.SubmitTime,
                    x.UIC
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
        [EndpointMethod(EndpointName = "SubmitMuster", AllowArgumentLogging = true, AllowResponseLogging = true, RequiresAuthentication = true)]
        private static void EndpointMethod_SubmitMuster(MessageToken token)
        {
            if (token.AuthenticationSession == null)
            {
                token.AddErrorMessage("You must be logged in to submit muster records.", ErrorTypes.Authentication, System.Net.HttpStatusCode.Unauthorized);
                return;
            }

            //Did we get what we needed?
            if (!token.Args.ContainsKey("mustersubmissions"))
            {
                token.AddErrorMessage("You must send a 'mustersubmissions' parameter.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                return;
            }

            Dictionary<Guid, string> musterSubmissions = null;
            //When we try to parse the JSON from the request, we'll do it in a try catch because there's no convenient, performant TryParse implementation for this.
            try
            {
                musterSubmissions = token.Args["mustersubmissions"].CastJToken<Dictionary<Guid, string>>();
            }
            catch (Exception e)
            {
                token.AddErrorMessage("There was an error while trying to format your 'mustersubmissions' argument.  It should be sent in a JSON dictionary.  Parsing error details: {0}".FormatS(e.Message), ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                return;
            }

            //Validate the muster statuses
            if (musterSubmissions.Values.Any(x => !Entities.ReferenceLists.MusterStatuses.AllMusterStatuses.Any(y => y.Value.SafeEquals(x))))
            {
                token.AddErrorMessage("One or more requested muster statuses were not valid.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                return;
            }

            //This is the session in which we're going to do our muster updates.  We do it separately in case something terrible happens to the currently logged in user.
            //This means that if the currently logged in person updates their own muster then for the rest of this request, their muster will be invalid.  That's ok cause we shouldn't need it.
            using (var session = DataAccess.NHibernateHelper.CreateStatefulSession())
            using (var transaction = session.BeginTransaction())
            {
                //Tell the session we'll handle commits through the transaction, otherwise every property update on the current muster status will result in an update.
                session.CacheMode = NHibernate.CacheMode.Ignore;
                session.FlushMode = NHibernate.FlushMode.Commit;

                //Submit the query to load all the persons.  How fucking easy can this be.  Fuck off NHibernate.  Fetch the command/dep/div so we can use it without lazy loading.
                var persons = session.QueryOver<Person>().AndRestrictionOn(x => x.Id).IsIn(musterSubmissions.Keys).List();

                //Now we need to make sure the client is allowed to muster the persons the client wants to muster.
                if (persons.Any(x => !MusterRecord.CanClientMusterPerson(token.AuthenticationSession.Person, x)))
                {
                    token.AddErrorMessage("You were not authorized to muster one or more of the persons you tried to muster.", ErrorTypes.Authorization, System.Net.HttpStatusCode.Unauthorized);
                    return;
                }

                //Ok, the client is allowed to muster them.  Now we need to set their current muster statuses.
                for (int x = 0; x < persons.Count; x++)
                {
                    persons[x].CurrentMusterStatus.HasBeenSubmitted = true;
                    persons[x].CurrentMusterStatus.MusterDayOfYear = MusterRecord.GetMusterDay(token.CallTime);
                    persons[x].CurrentMusterStatus.Musterer = token.AuthenticationSession.Person;
                    persons[x].CurrentMusterStatus.MusterStatus = musterSubmissions.ElementAt(x).Value;
                    persons[x].CurrentMusterStatus.MusterYear = MusterRecord.GetMusterYear(token.CallTime);
                    persons[x].CurrentMusterStatus.SubmitTime = token.CallTime;

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
        [EndpointMethod(EndpointName = "LoadMusterablePersonsForToday", AllowArgumentLogging = true, AllowResponseLogging = true, RequiresAuthentication = true)]
        private static void EndpointMethod_LoadMusterablePersonsForToday(MessageToken token)
        {
            if (token.AuthenticationSession == null)
            {
                token.AddErrorMessage("You must be logged in to view muster records.", ErrorTypes.Authentication, System.Net.HttpStatusCode.Unauthorized);
                return;
            }

            //Where we're going to keep all the persons the client can muster.
            List<Person> musterablePersons = new List<Person>();

            var resolvedPermissions = token.AuthenticationSession.Person.PermissionGroups.Resolve(token.AuthenticationSession.Person, null);
            Authorization.Groups.PermissionGroupLevels highestLevelInMuster;
            if (!resolvedPermissions.HighestLevels.TryGetValue("Muster", out highestLevelInMuster))
                highestLevelInMuster = Authorization.Groups.PermissionGroupLevels.None;

            using (var session = DataAccess.NHibernateHelper.CreateStatefulSession())
            {

                //Hold off on submitting the query for now because we need to know who we're looking for. People in the person's command, department or division.
                var queryOver = session.QueryOver<Person>();

                //Switch on the highest level in muster and then add the query accordingly.
                switch (highestLevelInMuster)
                {
                    case Authorization.Groups.PermissionGroupLevels.Command:
                        {
                            queryOver = queryOver.Where(x => x.Command == token.AuthenticationSession.Person.Command);
                            musterablePersons = queryOver.List().ToList();
                            break;
                        }
                    case Authorization.Groups.PermissionGroupLevels.Department:
                        {
                            queryOver = queryOver.Where(x => x.Department == token.AuthenticationSession.Person.Department);
                            musterablePersons = queryOver.List().ToList();
                            break;
                        }
                    case Authorization.Groups.PermissionGroupLevels.Division:
                        {
                            queryOver = queryOver.Where(x => x.Division == token.AuthenticationSession.Person.Division);
                            musterablePersons = queryOver.List().ToList();
                            break;
                        }
                    case Authorization.Groups.PermissionGroupLevels.Self:
                        {
                            //If the client's highest level is 'Self' then they can only muster themselves.
                            musterablePersons.Add(token.AuthenticationSession.Person);
                            break;
                        }
                    case Authorization.Groups.PermissionGroupLevels.None:
                        {
                            //If the client's highest level is none, then they basically got their permissions taken away.
                            break;
                        }
                    default:
                        {
                            throw new Exception("The default case in the highest level switch in the LoadMusterablePersonForToday endpoint was reached with the following case: '{0}'!".FormatS(highestLevelInMuster));
                        }
                }

                //Here we also want to limit the msuterable persons query to only those people   

                //Now that we have the results from the database, let's project them into our results.  This won't be the final DTO, we're going to layer on some additional information for the client to use.
                //Because Atwood is a good code monkey. Oh yes he is.
                var results = musterablePersons.Select(x =>
                {
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
                            x.CurrentMusterStatus.Command,
                            x.CurrentMusterStatus.Department,
                            x.CurrentMusterStatus.Division,
                            x.CurrentMusterStatus.DutyStatus,
                            x.CurrentMusterStatus.HasBeenSubmitted,
                            x.CurrentMusterStatus.Id,
                            x.CurrentMusterStatus.MusterDayOfYear,
                            Musteree = x.CurrentMusterStatus.Musteree.ToBasicPerson(),
                            Musterer = x.CurrentMusterStatus.Musterer == null ? null : x.CurrentMusterStatus.Musterer.ToBasicPerson(),
                            x.CurrentMusterStatus.MusterStatus,
                            x.CurrentMusterStatus.MusterYear,
                            x.CurrentMusterStatus.Paygrade,
                            x.CurrentMusterStatus.SubmitTime,
                            x.CurrentMusterStatus.UIC
                        },
                        CanMuster = MusterRecord.CanClientMusterPerson(token.AuthenticationSession.Person, x),
                        HasBeenMustered = x.CurrentMusterStatus.HasBeenSubmitted
                    };
                });

                //And now build the final DTO that's going out the door.
                token.SetResult(new
                {
                    CurrentYear = MusterRecord.GetMusterYear(token.CallTime),
                    CurrentDay = MusterRecord.GetMusterDay(token.CallTime),
                    Musters = results,
                    RolloverTime = Config.Muster.RolloverTime.ToString(),
                    ExpectedCompletionTime = Config.Muster.DueTime.ToString()
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
        [EndpointMethod(EndpointName = "FinalizeMuster", AllowResponseLogging = true, AllowArgumentLogging = true, RequiresAuthentication = true)]
        private static void EndpointMethod_FinalizeMuster(MessageToken token)
        {
            //Let's make sure we have permission to finalize muster.  You can finalize muster if you're logged in (no shit) and a command level muster... person.
            if (token.AuthenticationSession == null)
            {
                token.AddErrorMessage("You must be logged in to finalize the muster.", ErrorTypes.Authentication, System.Net.HttpStatusCode.Unauthorized);
                return;
            }

            if (!token.AuthenticationSession.Person.PermissionGroups.CanAccessSubmodules(SubModules.AdminTools.ToString()))
            {
                token.AddErrorMessage("You are not authorized to finalize muster.", ErrorTypes.Authorization, System.Net.HttpStatusCode.Unauthorized);
                return;
            }

            //Ok we have permission, let's make sure the muster hasn't already been finalized.
            if (Config.Muster.IsMusterFinalized)
            {
                token.AddErrorMessage("The muster has already been finalized.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                return;
            }

            //So we should be good to finalize the muster.
            MusterRecord.FinalizeMuster(token.AuthenticationSession.Person);
        }

        #endregion
    }
}