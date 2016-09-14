﻿using System;
using System.Collections.Generic;
using CCServ.ClientAccess;
using FluentNHibernate.Mapping;
using AtwoodUtils;
using System.Linq;
using NHibernate.Criterion;
using CCServ.Authorization;
using CCServ.Logging;

namespace CCServ.Entities.Muster
{
    /// <summary>
    /// Describes a single muster record, intended to archive the fact that a person claimed that another person was in a given state at a given time.
    /// </summary>
    public class MusterRecord
    {

        

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

        /// <summary>
        /// Indicates whether or not this record has been submitted yet or if it was auto generated by the application.
        /// </summary>
        public virtual bool HasBeenSubmitted { get; set; }

        #endregion

        #region Overrides

        /// <summary>
        /// Returns this.MusterStatus.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return this.MusterStatus;
        }

        #endregion

        #region ctors

        public MusterRecord()
        {
            if (Id == default(Guid))
                Id = Guid.NewGuid();
        }

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

            if (dateTime.InclusiveBetween(dateTime.Date.Subtract(TimeSpan.FromHours(24) - TimeSpan.FromSeconds(Config.Muster.RolloverTime.GetSeconds())), dateTime.Date.AddSeconds(Config.Muster.RolloverTime.GetSeconds())))
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
        /// Gets the current year of the muster.
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public static int GetMusterYear(DateTime dateTime)
        {
            System.Globalization.JulianCalendar julCalendar = new System.Globalization.JulianCalendar();

            if (dateTime.InclusiveBetween(dateTime.Date.Subtract(TimeSpan.FromHours(24) - TimeSpan.FromSeconds(Config.Muster.RolloverTime.GetSeconds())), dateTime.Date.AddSeconds(Config.Muster.RolloverTime.GetSeconds())))
                return julCalendar.GetYear(dateTime);
            else
            {
                //If we're here then we're after the muster rollover hour.  For example, the roll over is 16 and we're on 17.  This means that the muster day is the NEXT day.
                //However, we need to make sure we don't return a date that is out of range for the year.  So if today is the last day of the year, then return 1, because we'll be starting next year's muster.
                if (julCalendar.GetDaysInYear(dateTime.Year) == julCalendar.GetDayOfYear(dateTime))
                    return julCalendar.GetYear(dateTime) + 1; //This makes the assumption that time is infinite.  This might require refactoring if proven otherwise.
                else
                    return julCalendar.GetYear(dateTime);
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
                Id = Guid.NewGuid(),
                Command = null,
                Department = null,
                Division = null,
                DutyStatus = null,
                MusterDayOfYear = GetMusterDay(date),
                Musteree = person,
                Musterer = null,
                MusterStatus = null,
                MusterYear = GetMusterYear(date),
                Paygrade = null,
                UIC = null,
                SubmitTime = default(DateTime)
            };
        }

        /// <summary>
        /// Returns a boolean indicating whether or not the given person (client) can muster the other given person (person).
        /// </summary>
        /// <param name="client"></param>
        /// <param name="person"></param>
        /// <returns></returns>
        public static bool CanClientMusterPerson(Person client, Person person)
        {
            var resolvedPermissions = client.PermissionGroups.Resolve(client, person);
            var highestLevelInMuster = resolvedPermissions.HighestLevels["Muster"];

            switch (highestLevelInMuster)
            {
                case Authorization.Groups.PermissionGroupLevels.Command:
                    {
                        return client.IsInSameCommandAs(person);
                    }
                case Authorization.Groups.PermissionGroupLevels.Department:
                    {
                        return client.IsInSameDepartmentAs(person);
                    }
                case Authorization.Groups.PermissionGroupLevels.Division:
                    {
                        return client.IsInSameDivisionAs(person);
                    }
                case Authorization.Groups.PermissionGroupLevels.Self:
                    {
                        return client.Id == person.Id;
                    }
                case Authorization.Groups.PermissionGroupLevels.None:
                    {
                        return false;
                    }
                default:
                    {
                        throw new NotImplementedException("Fell to the default case in the CanClientMusterPerson switch.");
                    }
            }
        }

        /// <summary>
        /// Finalizes the muster for the current day by taking all of the current muster records from all persons in the database, 
        /// using them to build a report, 
        /// sending an email report, 
        /// saving the report,
        /// and then resetting everyone's current muster record and then archiving the old ones.
        /// </summary>
        /// <param name="creator">The person who initiated the muster finalization.  If null, the system initiated it.</param>
        public static void FinalizeMuster(Person creator)
        {
            if (Config.Muster.IsMusterFinalized)
                throw new Exception("You can't finalize the muster.  It's already been finalized.  A rollover must first occur.");

            //First up, we need everyone and their muster records.  Actually we need a session first.
            using (var session = DataAccess.NHibernateHelper.CreateStatefulSession())
            using (var transaction = session.BeginTransaction())
            {
                try
                {
                    //Set it to true to prevent anyone else trying to finalize the muster while we're processing.
                    Config.Muster.IsMusterFinalized = true;

                    var persons = GetMusterablePersonsQuery(session).List();
                    
                    //Ok we have all the persons and their muster records.  #thatwaseasy
                    foreach (Person person in persons)
                    {
                        person.CurrentMusterStatus.Command = person.Command == null ? "" : person.Command.Value;
                        person.CurrentMusterStatus.Department = person.Department == null ? "" : person.Department.Value;
                        person.CurrentMusterStatus.Division = person.Division == null ? "" : person.Division.Value;
                        person.CurrentMusterStatus.DutyStatus = person.DutyStatus.ToString();
                        if (!person.CurrentMusterStatus.HasBeenSubmitted)
                        {
                            person.CurrentMusterStatus.MusterStatus = ReferenceLists.MusterStatuses.UA.ToString();
                            person.CurrentMusterStatus.SubmitTime = DateTime.Now;
                        }
                        person.CurrentMusterStatus.HasBeenSubmitted = true;
                        person.CurrentMusterStatus.Paygrade = person.Paygrade.ToString();
                        person.CurrentMusterStatus.UIC = person.UIC == null ? "" : person.UIC.Value;

                        session.Save(person);
                    }

                    var model = new Email.Models.MusterReportEmailModel(persons.Select(x => x.CurrentMusterStatus), creator, DateTime.Now)
                    {
                        RollOverTime = Config.Muster.RolloverTime,
                    };

                    //Ok, now we need to send the email.
                    Email.EmailInterface.CCEmailMessage
                        .CreateDefault()
                        .To(Config.Email.DeveloperDistroAddress)
                        .Subject("Muster Report")
                        .HTMLAlternateViewUsingTemplateFromEmbedded("CCServ.Email.Templates.MusterReport_HTML.html", model)
                        .SendWithRetryAndFailure(TimeSpan.FromSeconds(1));

                    transaction.Commit();
                }
                catch (Exception e)
                {
                    transaction.Rollback();

                    //Set to false because we rolled back our changes.
                    Config.Muster.IsMusterFinalized = false;

                    Log.Exception(e, "The finalize muster method failed!  All changes were rolled back. The muster was not finalized!");
                    
                    //Note: we can't re-throw the error because no one is listening for it.  We just need to handle that here.  We're far outside the sync context, just south of the rishi maze.
                }
            }
        }

        /// <summary>
        /// Rolls over the muster.
        /// </summary>
        public static void RolloverMuster(bool finalizeIfNeeded = false)
        {
            if (finalizeIfNeeded)
            {
                if (!Config.Muster.IsMusterFinalized)
                {
                    FinalizeMuster(null);
                }
            }

            if (!Config.Muster.IsMusterFinalized)
                throw new Exception("You can't rollover the muster until it has been finalized.  Consider passing the finalizeIfNeeded flag set to true.");

            //First up, we need everyone and their muster records.  Actually we need a session first.
            using (var session = DataAccess.NHibernateHelper.CreateStatefulSession())
            using (var transaction = session.BeginTransaction())
            {
                try
                {
                    var persons = GetMusterablePersonsQuery(session).List();

                    //Now we need to go through each person and reset their current muster status.
                    foreach (var person in persons)
                    {
                        person.CurrentMusterStatus = CreateDefaultMusterRecordForPerson(person, DateTime.Now);
                        session.Save(person);
                    }

                    Config.Muster.IsMusterFinalized = false;

                    transaction.Commit();
                }
                catch (Exception e)
                {
                    transaction.Rollback();

                    Log.Exception(e, "The rollover muster method failed!  All changes were rolled back. The muster was not advanced!");

                    //Note: we can't rethrow the error because no one is listening for it.  We just need to handle that here.  We're far outside the sync context, just south of the rishi maze.
                }
            }
        }

        /// <summary>
        /// Returns a list of those persons who are currently musterable.  Uses a given session to do the loading.
        /// </summary>
        /// <param name="session"></param>
        /// <returns></returns>
        public static NHibernate.IQueryOver<Person, Person> GetMusterablePersonsQuery(NHibernate.ISession session)
        {
            return session.QueryOver<Person>().Where(x => x.DutyStatus != ReferenceLists.DutyStatuses.Loss);
        }

        #endregion

        

        #region Startup Methods

        /// <summary>
        /// Registers the roll over method to run at a certain time.
        /// </summary>
        [ServiceManagement.StartMethod(Priority = 1)]
        private static void SetupMuster(CLI.Options.LaunchOptions launchOptions)
        {
            Log.Info("Detecting current muster state...");


            using (var session = DataAccess.NHibernateHelper.CreateStatefulSession())
            using (var transaction = session.BeginTransaction())
            {
                try
                {
                    var persons = GetMusterablePersonsQuery(session).List();

                    if (persons.Select(x => x.CurrentMusterStatus).GroupBy(x => x.MusterDayOfYear).Count() != 1)
                    {
                        Log.Warning("Current muster records are not all for the same day!  Cleaning up muster records...");

                        //Ok so these muster records aren't all from the same day.
                        //To fix this we're going to select any muster records that's aren't for today and then try to archive them and the reset the person's profile with a blank muster record.
                        //During the archive, we'll need to ask if the person already has a muster record for that day, if so, we'll throw out the one we have.
                        var recordsInError = persons.Select(x => x.CurrentMusterStatus).Where(x => x.MusterDayOfYear != GetMusterDay(DateTime.Now) && x.MusterYear != GetMusterYear(DateTime.Now));

                        List<MusterRecord> musterRecordsForReset = new List<MusterRecord>();

                        List<Person> personsNeedingNewMusterRecords = new List<Person>();

                        //Ok we have them, now see if they already exist for the user.
                        foreach (var record in recordsInError)
                        {
                            //Query for this date and year for this person.
                            var otherRecordsFromSameDay = session.QueryOver<MusterRecord>()
                                .Where(x => x.MusterDayOfYear == record.MusterDayOfYear && x.MusterYear == record.MusterYear && x.Musteree.Id == record.Musteree.Id && x.Id != record.Id)
                                .List();

                            if (otherRecordsFromSameDay.Count != 0)
                            {
                                //There's already a record in the archive, so let's add this one to the list of muster records to be reset.
                                musterRecordsForReset.Add(record);

                                Log.Warning("A current muster record for the person, '{0}', was found for the day and year, '{1}':'{2}'.".FormatS(record.Musteree.ToString(), record.MusterDayOfYear, record.MusterYear) +
                                    "  While trying to archive that record, another record for that date was found to have already been archived.  The current muster record in question was thrown out.");
                            }
                            else //There is no archive yet, so we'll add it.  We "add" it by just resetting the muster record on the profile entirely.
                            {
                                personsNeedingNewMusterRecords.Add(record.Musteree);

                                Log.Warning("A muster record for the person, '{0}', was found for the day and year, '{1}':'{2}'.".FormatS(record.Musteree.ToString(), record.MusterDayOfYear, record.MusterYear) +
                                    "  The record has been archived and the person's current muster record was reset.");
                            }
                        }

                        //Let's handle the stuff we got and do the updates..  This first one, we need to reset without changing the reference.
                        for (int x = 0; x < musterRecordsForReset.Count; x++)
                        {
                            musterRecordsForReset[x].Command = null;
                            musterRecordsForReset[x].Department = null;
                            musterRecordsForReset[x].Division = null;
                            musterRecordsForReset[x].DutyStatus = null;
                            musterRecordsForReset[x].HasBeenSubmitted = false;
                            musterRecordsForReset[x].MusterDayOfYear = GetMusterDay(DateTime.Now);
                            musterRecordsForReset[x].Musterer = null;
                            musterRecordsForReset[x].MusterStatus = null;
                            musterRecordsForReset[x].MusterYear = GetMusterYear(DateTime.Now);
                            musterRecordsForReset[x].Paygrade = null;
                            musterRecordsForReset[x].SubmitTime = default(DateTime);
                            musterRecordsForReset[x].UIC = null;

                            session.Update(musterRecordsForReset[x]);
                        }

                        //This one though, the person legit needs a whoel new muster record.
                        foreach (var person in personsNeedingNewMusterRecords)
                        {
                            person.CurrentMusterStatus = CreateDefaultMusterRecordForPerson(person, DateTime.Now);

                            session.Update(person);
                        }

                        Log.Info("Muster record clean up completed.");

                    }

                    //Ok, at this point, we know that we have muster records for today.  Let's just tell the host how far along we are.
                    Log.Info("{0}/{1} person(s) have been mustered so far.".FormatS(persons.Count(x => x.CurrentMusterStatus.HasBeenSubmitted), persons.Count));
                    Log.Info("Muster finalization status : {0}".FormatS(Config.Muster.IsMusterFinalized ? "Finalized" : "Not Finalized"));
                    Log.Info("Expected completion time : {0}".FormatS(Config.Muster.DueTime.ToString()));
                    Log.Info("Rollover time : {0}".FormatS(Config.Muster.RolloverTime.ToString()));

                    Log.Info("Registering muster roll over to occur every day at '{0}'".FormatS(Config.Muster.RolloverTime.ToString()));
                    FluentScheduler.JobManager.AddJob(() => RolloverMuster(true), s => s.ToRunEvery(1).Days().At(Config.Muster.RolloverTime.Hours, Config.Muster.RolloverTime.Minutes));


                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
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
                Id(x => x.Id).GeneratedBy.Assigned();

                References(x => x.Musterer).Nullable().LazyLoad(Laziness.False);
                References(x => x.Musteree).Nullable().LazyLoad(Laziness.False);

                Map(x => x.Paygrade).Nullable();
                Map(x => x.Division).Nullable();
                Map(x => x.Department).Nullable();
                Map(x => x.Command).Nullable();
                Map(x => x.MusterStatus).Nullable();
                Map(x => x.DutyStatus).Nullable();
                Map(x => x.SubmitTime).Nullable();
                Map(x => x.MusterDayOfYear).Nullable();
                Map(x => x.MusterYear).Not.Nullable();
                Map(x => x.HasBeenSubmitted).Nullable();
            }
        }

    }
}
