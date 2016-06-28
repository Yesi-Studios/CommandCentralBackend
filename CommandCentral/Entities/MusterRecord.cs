using System;
using System.Collections.Generic;
using CommandCentral.ClientAccess;
using FluentNHibernate.Mapping;
using AtwoodUtils;
using System.Linq;
using NHibernate.Criterion;

namespace CommandCentral.Entities
{
    /// <summary>
    /// Describes a single muster record, intended to archive the fact that a person claimed that another person was in a given state at a given time.
    /// </summary>
    public class MusterRecord
    {

        /// <summary>
        /// The hour at which the muster will roll over, starting a new muster day, regardless of the current muster's status.
        /// </summary>
        private static readonly int _rolloverHour = 16;

        /// <summary>
        /// The hour at which the muster _should_ be completed.  This governs when email are sent and their urgency.
        /// </summary>
        private static readonly int _dueHour = 9;

        #region Properties

        /// <summary>
        /// Unique GUID of this muster record
        /// </summary>
        public virtual Guid Id { get; set; }

        /// <summary>
        /// Musterer - I hate that word
        /// </summary>
        public virtual Person Musterer { get; set; }

        /// <summary>
        /// The Person being mustered by the musterer, which is the person mustering the person that must be mustered. muster.
        /// </summary>
        public virtual Person Musteree { get; set; }

        /// <summary>
        /// The Person being mustered's paygrade. Fucking Mustard.
        /// </summary>
        public virtual string Paygrade { get; set; }

        /// <summary>
        /// The person being mustered's UIC.  Fucking Mustard x 2.
        /// </summary>
        public virtual string UIC { get; set; }

        /// <summary>
        /// The person that is having the muster happen to them's division.
        /// </summary>
        public virtual string Division { get; set; }

        /// <summary>
        /// The individual that is being made accountable for through the process of mustering's department
        /// </summary>
        public virtual string Department { get; set; }

        /// <summary>
        /// The human being chosen to say their name out loud in front of their peers to make sure they are alive and where they should be at that specific time's Command
        /// </summary>
        public virtual string Command { get; set; }

        /// <summary>
        /// The one tiny human being on this planet out of all the other people that signed a contract that has binded him into a life of accountability's muster state.
        /// </summary>
        public virtual string MusterStatus { get; set; }

        /// <summary>
        /// That same person from above's duty status
        /// </summary>
        public virtual string DutyStatus { get; set; }

        /// <summary>
        /// The date and time the person was mustered at.
        /// </summary>
        public virtual DateTime SubmitTime { get; set; }

        /// <summary>
        /// The day of the year for which this muster was made.  Because the "muster day" may not align perfectly with a normal day, this value is tracked separately.
        /// </summary>
        public virtual int MusterDayOfYear { get; set; }

        /// <summary>
        /// The year this muster record is in.
        /// </summary>
        public virtual int MusterYear { get; set; }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Returns the muster day, which is the Julian date shifted by the offset given by the _rolloverHour variable.
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public static int GetMusterDay(DateTime dateTime)
        {
            System.Globalization.JulianCalendar julCalendar = new System.Globalization.JulianCalendar();

            if (dateTime.InclusiveBetween(dateTime.Date.Subtract(TimeSpan.FromHours(24 - _rolloverHour)), dateTime.Date.AddHours(_rolloverHour)))
                return julCalendar.GetDayOfYear(dateTime);
            else
            {
                //If we're here then we're after the muster rollover hour.  For example, the roll over is 16 and we're on 17.  This means that the muster day is the NEXT day.
                //However, we need to make sure we don't return a date that is out of range for the year.  So if today is the last day of the year, then return 1, because we'll be starting next year's muster.
                if (julCalendar.GetDaysInYear(dateTime.Year) == julCalendar.GetDayOfYear(dateTime))
                    return 1;
                else
                    return julCalendar.GetDayOfYear(dateTime) + 1;
            }
        }

        /// <summary>
        /// Creates a new muster status with everything set to null except the musteree and the current day values.
        /// </summary>
        /// <param name="person"></param>
        /// <param name="date">The date time for which to create this muster record.</param>
        /// <returns></returns>
        public static MusterRecord CreateDefaultMusterRecordForPerson(Person person, DateTime date)
        {
            return new MusterRecord
            {
                Command = null,
                Department = null,
                Division = null,
                DutyStatus = null,
                MusterDayOfYear = GetMusterDay(date),
                Musteree = person,
                Musterer = null,
                MusterStatus = null,
                MusterYear = date.Year,
                Paygrade = null,
                UIC = null,
                SubmitTime = default(DateTime)
            };
        }

        public static bool CanClientMusterPerson(Person client, Person person)
        {

        }

        #endregion

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

            token.SetResult(token.CommunicationSession.Get<MusterRecord>(musterRecordId));
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

            //Set the result.
            token.SetResult(token.CommunicationSession.QueryOver<MusterRecord>().Where(x => x.Musteree.Id == mustereeId));
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

            //Set the result.
            token.SetResult(token.CommunicationSession.QueryOver<MusterRecord>().Where(x => x.Musterer.Id == mustererId));
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
            if (!DateTime.TryParse(token.Args["musterdate"] as string, out musterDate))
            {
                token.AddErrorMessage("Your 'musterdate' parameter was not in a valid format.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                return;
            }

            token.SetResult(token.CommunicationSession.QueryOver<MusterRecord>().Where(x => x.MusterDayOfYear == MusterRecord.GetMusterDay(musterDate) && x.MusterYear == musterDate.Year).List());
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
                token.AddErrorMessage("You must be logged in to view muster records.", ErrorTypes.Authentication, System.Net.HttpStatusCode.Unauthorized);
                return;
            }

            //Before we do anything, make sure the client has permission to do muster.
            if (!token.AuthenticationSession.Person.HasSpecialPermissions(Authorization.SpecialPermissions.SubmitMuster))
            {
                token.AddErrorMessage("You are not authorized to submit muster.", ErrorTypes.Authorization, System.Net.HttpStatusCode.Unauthorized);
                return;
            }

            if (!token.Args.ContainsKey("mustersubmissions"))
            {
                token.AddErrorMessage("You must send a 'mustersubmissions' parameter.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                return;
            }


            Dictionary<Guid, string> musterSubmissions = new Dictionary<Guid, string>();
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
            MusterStatuses tempStatus;
            if (musterSubmissions.Values.Any(x => !Enum.TryParse<MusterStatuses>(x, out tempStatus)))
            {
                token.AddErrorMessage("One or more requested muster statuses were not valid.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                return;
            }

            //Submit the query to load all the persons.  How fucking easy can this be.  Fuck off NHibernate.  Fetch the command/dep/div so we can use it without lazy loading.
            var persons = token.CommunicationSession.QueryOver<Person>().AndRestrictionOn(x => x.Id).IsIn(musterSubmissions.Keys)
                .Fetch(x => x.Department).Eager
                .Fetch(x => x.Command).Eager
                .Fetch(x => x.Division).Eager
                .List();

            //Ok we have all the persons, now we need to make sure the client is in their chains of command.  Every.  Single. One.  Oh boy.
            if (persons.Any(x => !token.AuthenticationSession.Person.IsInChainOfCommandOf(x)))
            {
                //If any one person is not in the person's chain of command, the fail out of the whole thing.  We don't care who the person was or how many there were.  Fuck her.  Right in the pussy.
                token.AddErrorMessage("You are not authorized to submit muster for one or more persons in your requested list.", ErrorTypes.Authorization, System.Net.HttpStatusCode.Unauthorized);
                return;
            }

            //Ok, the client is in their chains of command.  Now we can submit muster.
            //First though, we need to find out what muster day we're on.  Looks like there's a method for that!
            int musterDayOfYear = MusterRecord.GetMusterDay(token.CallTime);

            //Ok now we need to get all those people who have already had a muster record submitted for today.
            var alreadyMusteredPersons = token.CommunicationSession.QueryOver<MusterRecord>().Where(x => x.MusterDayOfYear == musterDayOfYear).AndRestrictionOn(x => x.Musteree.Id).IsIn(musterSubmissions.Keys).Select(x => x.Musteree).List<Person>();

            //Now we just need to make a new muster record for anyone not in the above list.
            var personsToSubmit = persons.Except(alreadyMusteredPersons).ToList();
            foreach (var person in personsToSubmit)
            {
                token.CommunicationSession.Save(new MusterRecord
                {
                    Command = person.Command.Value,
                    Department = person.Department.Value,
                    Division = person.Department.Value,
                    DutyStatus = person.DutyStatus.ToString(),
                    MusterDayOfYear = musterDayOfYear,
                    Musteree = person,
                    Musterer= token.AuthenticationSession.Person,
                    MusterStatus = musterSubmissions[person.Id].ToString(),
                    Paygrade = person.Designation.Value,
                    SubmitTime = token.CallTime
                });
            }

            //Ok, well we're done!  Now we just need to tell the client that we finished and tell the client which muster records we inserted.
            token.SetResult(personsToSubmit.Select(x => x.Id).ToList());
        }

        /// <summary>
        /// WARNING!  THIS METHOD IS EXPOSED TO THE CLIENT AND IS NOT INTENDED FOR INTERNAL USE.  AUTHENTICATION, AUTHORIZATION AND VALIDATION MUST BE HANDLED PRIOR TO DB INTERACTION.
        /// <para />
        /// Loads the current day's muster, returning all muster records for today, all persons who still need to be mustered, the current day, and a list of those persons who the client can muster.
        /// <para />
        /// Client Parameters: <para />
        ///     None
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        [EndpointMethod(EndpointName = "LoadTodaysMuster", AllowArgumentLogging = true, AllowResponseLogging = true, RequiresAuthentication = true)]
        private static void EndpointMethod_LoadTodaysMuster(MessageToken token)
        {
            if (token.AuthenticationSession == null)
            {
                token.AddErrorMessage("You must be logged in to view muster records.", ErrorTypes.Authentication, System.Net.HttpStatusCode.Unauthorized);
                return;
            }

            //We need all the current muster records for today.
            var persons = token.CommunicationSession.QueryOver<Person>()
                .Fetch(x => x.CurrentMusterStatus).Eager
                .Fetch(x => x.Command).Eager
                .Fetch(x => x.Department).Eager
                .Fetch(x => x.Division).Eager
                .Fetch(x => x.UIC).Eager
                .List();
               

            //Now we need to know what the muster day is.
            int musterDayOfTheYear = MusterRecord.GetMusterDay(token.CallTime);

            //And now we need all of the muster records for today.
            var todaysMusterRecords = token.CommunicationSession.QueryOver<MusterRecord>().Where(x => x.MusterDayOfYear == musterDayOfTheYear && x.MusterYear == token.CallTime.Year).List();

            //Now we need all those persons that have not yet been mustered.
            var remainingPersons = token.CommunicationSession.QueryOver<Person>().WhereRestrictionOn(x => x.Id).Not.IsIn(todaysMusterRecords.Select(x => x.Musteree.Id).ToList()).List();

            //Now we also need to show the client who they can muster if they can even muster at all.
            List<Guid> musterablePersonIds = new List<Guid>();
            if (token.AuthenticationSession.Person.HasSpecialPermissions(Authorization.SpecialPermissions.SubmitMuster))
            {
                //Here we're going to put together a list of all remaining persons and the musterees.  We'll then determine which ones we can muster.  And we'll do it in one line.  Cause boss.
                musterablePersonIds = todaysMusterRecords.Select(x => x.Musteree).Concat(remainingPersons).Where(x => token.AuthenticationSession.Person.IsInChainOfCommandOf(x)).Select(x => x.Id).ToList();
            }

            //Now all there is to do is give all this shit to the client.
            token.SetResult(new { MusterRecords = todaysMusterRecords, RemainingPersons = remainingPersons, MusterablePersonIds = musterablePersonIds, MusterDayOfTheYear = musterDayOfTheYear});
        }

        #endregion

        /// <summary>
        /// Maps a record to the database.
        /// </summary>
        public class MusterRecordMapping : ClassMap<MusterRecord>
        {
            /// <summary>
            /// Maps a record to the database.
            /// </summary>
            public MusterRecordMapping()
            {
                Id(x => x.Id).GeneratedBy.Guid();

                References(x => x.Musterer).Not.Nullable();
                References(x => x.Musteree).Not.Nullable();

                Map(x => x.Paygrade).Not.Nullable().Length(10);
                Map(x => x.Division).Not.Nullable().Length(10);
                Map(x => x.Department).Not.Nullable().Length(10);
                Map(x => x.Command).Not.Nullable().Length(10);
                Map(x => x.MusterStatus).Not.Nullable().Length(20);
                Map(x => x.DutyStatus).Not.Nullable().Length(20);
                Map(x => x.SubmitTime).Not.Nullable();
                Map(x => x.MusterDayOfYear).Not.Nullable();
            }
        }

    }
}
