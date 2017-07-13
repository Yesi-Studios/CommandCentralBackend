﻿using System;
using System.Collections.Generic;
using CommandCentral.ClientAccess;
using FluentNHibernate.Mapping;
using AtwoodUtils;
using System.Linq;
using NHibernate.Criterion;
using CommandCentral.Authorization;
using CommandCentral.Logging;
using NHibernate.Type;
using CommandCentral.Entities.ReferenceLists;

namespace CommandCentral.Entities
{
    /// <summary>
    /// Describes a single muster record, intended to archive the fact that a person claimed that another person was in a given state at a given time.
    /// </summary>
    public class MusterRecord
    {
        /// <summary>
        /// Indicates that the muster has been finalized.
        /// </summary>
        public static bool IsMusterFinalized { get; set; } = false;

        /// <summary>
        /// The time at which muster should roll over.
        /// </summary>
        public static CustomTypes.Time RolloverTime { get; } = new CustomTypes.Time(20, 0, 0);

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
        /// The person's designation at the time of muster.
        /// </summary>
        public virtual string Designation { get; set; }
        
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
        public virtual DateTime MusterDate { get; set; }

        /// <summary>
        /// Indicates whether or not this record has been submitted yet or if it was auto generated by the application.
        /// </summary>
        public virtual bool HasBeenSubmitted { get; set; }

        /// <summary>
        /// A free text field for clients to put comments in.
        /// </summary>
        public virtual string Remarks { get; set; }

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

        /// <summary>
        /// Deep comparison.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            var other = obj as MusterRecord;
            if (other == null)
                return false;

            return Object.Equals(other.Id, this.Id) &&
                   Object.Equals(other.Musterer == null ? Guid.Empty : other.Musterer.Id, this.Musterer == null ? Guid.Empty : this.Musterer.Id) &&
                   Object.Equals(other.Musteree.Id, this.Musteree.Id) &&
                   Object.Equals(other.Paygrade, this.Paygrade) &&
                   Object.Equals(other.Designation, this.Designation) &&
                   Object.Equals(other.UIC, this.UIC) &&
                   Object.Equals(other.Division, this.Division) &&
                   Object.Equals(other.Department, this.Department) &&
                   Object.Equals(other.Command, this.Command) &&
                   Object.Equals(other.MusterStatus, this.MusterStatus) &&
                   Object.Equals(other.DutyStatus, this.DutyStatus) &&
                   Object.Equals(other.SubmitTime, this.SubmitTime) &&
                   Object.Equals(other.MusterDate, this.MusterDate) &&
                   Object.Equals(other.HasBeenSubmitted, this.HasBeenSubmitted) &&
                   Object.Equals(other.Remarks, this.Remarks);
        }

        /// <summary>
        /// hashey codey
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;

                hash = hash * 23 + Utilities.GetSafeHashCode(Id);
                hash = hash * 23 + Utilities.GetSafeHashCode(Musterer);
                hash = hash * 23 + Utilities.GetSafeHashCode(Musteree);
                hash = hash * 23 + Utilities.GetSafeHashCode(Paygrade);
                hash = hash * 23 + Utilities.GetSafeHashCode(Designation);
                hash = hash * 23 + Utilities.GetSafeHashCode(UIC);
                hash = hash * 23 + Utilities.GetSafeHashCode(Division);
                hash = hash * 23 + Utilities.GetSafeHashCode(Department);
                hash = hash * 23 + Utilities.GetSafeHashCode(Command);
                hash = hash * 23 + Utilities.GetSafeHashCode(MusterStatus);
                hash = hash * 23 + Utilities.GetSafeHashCode(DutyStatus);
                hash = hash * 23 + Utilities.GetSafeHashCode(SubmitTime);
                hash = hash * 23 + Utilities.GetSafeHashCode(MusterDate);
                hash = hash * 23 + Utilities.GetSafeHashCode(HasBeenSubmitted);
                hash = hash * 23 + Utilities.GetSafeHashCode(Remarks);

                return hash;
            }
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
        /// Returns the muster day, which is the date shifted by the offset given by the _rolloverHour variable.
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public static DateTime GetMusterDate(DateTime dateTime)
        {
            System.Globalization.GregorianCalendar gregCalendar = new System.Globalization.GregorianCalendar(System.Globalization.GregorianCalendarTypes.USEnglish);

            

            if (dateTime.InclusiveBetween(dateTime.Date.Subtract(TimeSpan.FromHours(24) - TimeSpan.FromSeconds(MusterRecord.RolloverTime.GetSeconds())), 
                dateTime.Date.AddSeconds(MusterRecord.RolloverTime.GetSeconds())))
                return dateTime.Date;
            else
            {
                //If we're here then we're after the muster rollover hour.  For example, the roll over is 16 and we're on 17.  This means that the muster day is the NEXT day.
                return dateTime.AddDays(1).Date;
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
                Musteree = person,
                Musterer = null,
                MusterStatus = null,
                MusterDate = GetMusterDate(date).Date,
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
            var resolvedPermissions = client.ResolvePermissions(person);

            return resolvedPermissions.IsInChainOfCommand[ChainsOfCommand.Muster];
        }

        /// <summary>
        /// Finalizes the muster for the current day by taking all of the current muster records from all persons in the database, 
        /// using them to build a report, 
        /// sending an email report, 
        /// saving the report,
        /// and then resetting everyone's current muster record and then archiving the old ones.
        /// </summary>
        /// <param name="token">The token which represents the communication during which the muster was finalized.  If null, the system finalizes the muster.</param>
        public static void FinalizeMuster(MessageToken token = null)
        {
            if (IsMusterFinalized)
                throw new Exception("You can't finalize the muster.  It's already been finalized.  A rollover must first occur.");

            //First up, we need everyone and their muster records.  Actually we need a session first.
            using (var session = DataAccess.DataProvider.CreateStatefulSession())
            using (var transaction = session.BeginTransaction())
            {
                try
                {
                    //Set it to true to prevent anyone else trying to finalize the muster while we're processing.
                    IsMusterFinalized = true;

                    var persons = GetMusterablePersonsQuery(session).List();

                    //Let's make sure that all persons' current muster records are from the same day.
                    if (persons.GroupBy(x => x.CurrentMusterRecord.MusterDate).Count() != 1)
                        throw new Exception("Not all muster records are from the same day during finalization!");

                    //Ok we have all the persons and their muster records.  #thatwaseasy
                    foreach (Person person in persons)
                    {
                        if (person.CurrentMusterRecord == null)
                            throw new Exception("{0}'s current muster status was null.".With(person.ToString()));

                        person.CurrentMusterRecord.Command = person.Command == null ? "" : person.Command.ToString();
                        person.CurrentMusterRecord.Department = person.Department == null ? "" : person.Department.ToString();
                        person.CurrentMusterRecord.Division = person.Division == null ? "" : person.Division.ToString();
                        person.CurrentMusterRecord.DutyStatus = person.DutyStatus.ToString();
                        if (!person.CurrentMusterRecord.HasBeenSubmitted)
                        {
                            person.CurrentMusterRecord.MusterStatus = ReferenceLists.ReferenceListHelper<ReferenceLists.MusterStatus>.Find("UA").ToString();
                            person.CurrentMusterRecord.SubmitTime = DateTime.UtcNow;
                        }
                        person.CurrentMusterRecord.HasBeenSubmitted = true;
                        person.CurrentMusterRecord.Paygrade = person.Paygrade.ToString();
                        person.CurrentMusterRecord.UIC = person.UIC == null ? "" : person.UIC.ToString();
                        person.CurrentMusterRecord.Designation = person.Designation == null ? "" : person.Designation.ToString();

                        session.Save(person);
                    }

                    //Let's first commit the transaction, thus setting all the muster records to their needed states.
                    transaction.Commit();

                    //Then, we'll send the report.
                    new MusterReport(persons.First().CurrentMusterRecord.MusterDate).SendReport(token);
                }
                catch (Exception e)
                {
                    transaction.Rollback();

                    //Set to false because we rolled back our changes.
                    IsMusterFinalized = false;

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
                if (!IsMusterFinalized)
                {
                    FinalizeMuster();
                }
            }

            if (!IsMusterFinalized)
                throw new Exception("You can't rollover the muster until it has been finalized.  Consider passing the finalizeIfNeeded flag set to true.");

            //First up, we need everyone and their muster records.  Actually we need a session first.
            using (var session = DataAccess.DataProvider.CreateStatefulSession())
            using (var transaction = session.BeginTransaction())
            {
                try
                {
                    //In rollover, we're going to rollover ALL records, not just the records of musterable people.  
                    //This is in response to issue https://github.com/Yesi-Studios/CommandCentralBackend/issues/135
                    var persons = session.QueryOver<Person>().List();

                    //Now we need to go through each person and reset their current muster status.
                    foreach (var person in persons)
                    {
                        person.CurrentMusterRecord = CreateDefaultMusterRecordForPerson(person, DateTime.UtcNow);
                        session.Save(person);
                    }

                    IsMusterFinalized = false;

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
            return session.QueryOver<Person>().Where(x => x.DutyStatus != ReferenceLists.ReferenceListHelper<ReferenceLists.DutyStatus>.Find("Loss"));
        }

        #endregion

        #region Startup Methods

        /// <summary>
        /// Registers the roll over method to run at a certain time.
        /// <para />
        /// Also, attempts to scan through the muster records and detect and fix any discrepancies with the system.
        /// <para />
        /// For example, if the person's current muster record is not from today, then advance their muster record.
        /// </summary>
        public static void SetupMuster()
        {
            Log.Info("Detecting current muster state...");

            using (var session = DataAccess.DataProvider.CreateStatefulSession())
            using (var transaction = session.BeginTransaction())
            {
                try
                {
                    var persons = session.QueryOver<Person>()
                        .Fetch(x => x.CurrentMusterRecord).Eager
                        .List();

                    var personsWithIncorrectRecords = persons.Where(x => x.CurrentMusterRecord == null || x.CurrentMusterRecord.MusterDate.Date != GetMusterDate(DateTime.UtcNow)).ToList();

                    if (personsWithIncorrectRecords.Any())
                    {
                        Log.Info("Correcting {0} persons with incorrect muster records...".With(personsWithIncorrectRecords.Count));

                        int current = 0;
                        //Ok so each of these people's current muster records are for the wrong day.
                        //What we need to do is look up that day's muster record for this person and see if they have one.
                        //If they don't have one, the muster needs to get updated.
                        foreach (var person in personsWithIncorrectRecords)
                        {
                            if (person.CurrentMusterRecord == null)
                            {
                                person.CurrentMusterRecord = CreateDefaultMusterRecordForPerson(person, DateTime.UtcNow);
                            }
                            else
                            {
                                //Let's look for all muster records for this person for the date of their record.
                                //We're doing this to see if we have a duplicate record or not.
                                var musterRecordsFromDayInQuestion = session.QueryOver<MusterRecord>()
                                                    .Where(x => x.Musteree == person && x.MusterDate == person.CurrentMusterRecord.MusterDate)
                                                    .List();

                                //Ok now we have a list of all the muster records for this person from the day that is on their current muster record.
                                //If there is only one then that one must be the current record, 
                                //in which case, the current one needs to be archived, and a new one needs to be assigned.
                                if (musterRecordsFromDayInQuestion.Count == 1)
                                {
                                    //We can do that easily by jsut resetting the reference that is the current record.
                                    //The old current record will move into the archives by not being referenced by the person.
                                    person.CurrentMusterRecord = CreateDefaultMusterRecordForPerson(person, DateTime.UtcNow);
                                }
                                else if (musterRecordsFromDayInQuestion.Count == 2)
                                {
                                    //If there are two, then that means that the current record is here, but there's also another one that has already been archived.
                                    //In this case, just reset the current muster record and make it a current one.
                                    //We're going to do that by deleting the current muster record, flushing that delete
                                    //And then resetting the record.
                                    session.Delete(person.CurrentMusterRecord);
                                    session.Flush();
                                    person.CurrentMusterRecord = CreateDefaultMusterRecordForPerson(person, DateTime.UtcNow);
                                }
                                else if (musterRecordsFromDayInQuestion.Count == 0)
                                {
                                    //If we're here then this person doesn't have a muster record.  That's weird.  Give them one.
                                    person.CurrentMusterRecord = CreateDefaultMusterRecordForPerson(person, DateTime.UtcNow);
                                }
                                else
                                {
                                    //Who the fuck knows what happened if we got here.
                                    throw new Exception("A serious issue exists with the muster records from date '{0}' for person '{1}'.  Please resolve these issues.".With(person.CurrentMusterRecord.MusterDate, person.ToString()));
                                }
                            }

                            current++;
                            if (current % 2 == 0)
                            {
                                Log.Info("Fixed {0}% of muster records...".With(Math.Round(((double)current / (double)personsWithIncorrectRecords.Count) * 100, 2)));
                            }

                            session.Save(person);
                        }

                        Log.Info("Muster record clean up completed.");
                    }

                    //Ok, at this point, we know that we have muster records for today.  Let's just tell the host how far along we are.
                    Log.Info("{0}/{1} person(s) have been mustered so far.".With(persons.Count(x => x.DutyStatus != ReferenceListHelper<DutyStatus>.Find("Loss") && x.CurrentMusterRecord.HasBeenSubmitted), persons.Count));
                    Log.Info("Muster finalization status : {0}".With(MusterRecord.IsMusterFinalized ? "Finalized" : "Not Finalized"));
                    Log.Info("Rollover time : {0}".With(MusterRecord.RolloverTime));
                    Log.Info("Current muster date is: {0}".With(GetMusterDate(DateTime.UtcNow).ToString("D")));

                    Log.Info("Registering muster roll over to occur every day at '{0}'".With(MusterRecord.RolloverTime));
                    FluentScheduler.JobManager.AddJob(() => RolloverMuster(true), s => s.ToRunEvery(1).Days().At(MusterRecord.RolloverTime.Hours, MusterRecord.RolloverTime.Minutes));

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

                References(x => x.Musterer);
                References(x => x.Musteree);

                Map(x => x.Paygrade);
                Map(x => x.Division);
                Map(x => x.Department);
                Map(x => x.Command);
                Map(x => x.MusterStatus);
                Map(x => x.DutyStatus);
                Map(x => x.SubmitTime).CustomType<UtcDateTimeType>();
                Map(x => x.MusterDate).Not.Nullable().CustomType<UtcDateTimeType>();
                Map(x => x.HasBeenSubmitted).Not.Nullable();
                Map(x => x.Remarks);
                Map(x => x.Designation);
                Map(x => x.UIC);
            }
        }

    }
}
