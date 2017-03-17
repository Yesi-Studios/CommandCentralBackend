﻿using System;
using System.Collections.Generic;
using System.Linq;
using CCServ.Authorization;
using CCServ.ClientAccess;
using CCServ.Entities.ReferenceLists;
using CCServ.DataAccess;
using FluentNHibernate.Mapping;
using FluentValidation;
using NHibernate.Transform;
using NHibernate.Criterion;
using NHibernate.Linq;
using AtwoodUtils;
using CCServ.ServiceManagement;
using CCServ.Logging;
using System.Reflection;
using CCServ.DTOs;
using NHibernate.Type;

namespace CCServ.Entities
{
    /// <summary>
    /// Describes a single person and all their properties and data access methods.
    /// </summary>
    public class Person
    {

        #region Properties

        /// <summary>
        /// The person's unique Id.
        /// </summary>
        public virtual Guid Id { get; set; }

        #region Main Properties

        /// <summary>
        /// The person's last name.
        /// </summary>
        public virtual string LastName { get; set; }

        /// <summary>
        /// The person's first name.
        /// </summary>
        public virtual string FirstName { get; set; }

        /// <summary>
        /// The person's middle name.
        /// </summary>
        public virtual string MiddleName { get; set; }

        /// <summary>
        /// The person's SSN.
        /// </summary>
        public virtual string SSN { get; set; }

        /// <summary>
        /// The person's DoD Id which allows us to communicate with other systems about this person.
        /// </summary>
        public virtual string DoDId { get; set; }

        /// <summary>
        /// The person's suffix.
        /// </summary>
        public virtual string Suffix { get; set; }

        /// <summary>
        /// The person's date of birth.
        /// </summary>
        public virtual DateTime? DateOfBirth { get; set; }

        /// <summary>
        /// The person's age.  0 if the date of birth isn't set.
        /// </summary>
        public virtual int Age
        {
            get
            {
                if (DateOfBirth == null || !DateOfBirth.HasValue)
                    return 0;

                if (DateTime.Today.Month < DateOfBirth.Value.Month ||
                    DateTime.Today.Month == DateOfBirth.Value.Month &&
                    DateTime.Today.Day < DateOfBirth.Value.Day)
                {
                    return DateTime.Today.Year - DateOfBirth.Value.Year - 1;
                }

                return DateTime.Today.Year - DateOfBirth.Value.Year;
            }
        }

        /// <summary>
        /// The person's sex.
        /// </summary>
        public virtual Sex Sex { get; set; }

        /// <summary>
        /// The person's remarks.  This is the primary comments section
        /// </summary>
        public virtual string Remarks { get; set; }

        /// <summary>
        /// Stores the person's ethnicity.
        /// </summary>
        public virtual Ethnicity Ethnicity { get; set; }

        /// <summary>
        /// The person's religious preference
        /// </summary>
        public virtual ReligiousPreference ReligiousPreference { get; set; }

        /// <summary>
        /// The person's paygrade (e5, O1, O5, CWO2, GS1,  etc.)
        /// </summary>
        public virtual Paygrade Paygrade { get; set; }

        /// <summary>
        /// The person's Designation (CTI2, CTR1, 1114, Job title)
        /// </summary>
        public virtual Designation Designation { get; set; }

        /// <summary>
        /// The person's division
        /// </summary>
        public virtual Division Division { get; set; }

        /// <summary>
        /// The person's department
        /// </summary>
        public virtual Department Department { get; set; }

        /// <summary>
        /// The person's command
        /// </summary>
        public virtual Command Command { get; set; }

        /// <summary>
        /// The date this person received government travel card training.  Temporary and should be implemented in the training module.
        /// </summary>
        public virtual DateTime? GTCTrainingDate { get; set; }

        /// <summary>
        /// The date on which ADAMS training was completed.  Temporary and should be implemented in the training module.
        /// </summary>
        public virtual DateTime? ADAMSTrainingDate { get; set; }

        /// <summary>
        /// The date on which AWARE training was completed.  Temporary and should be implemented in the training module.
        /// </summary>
        public virtual bool HasCompletedAWARE { get; set; }

        /// <summary>
        /// The user's preferences.
        /// </summary>
        public virtual IDictionary<string, string> UserPreferences { get; set; }

        #endregion

        #region Work Properties

        /// <summary>
        /// The person's primary NEC.
        /// </summary>
        public virtual NEC PrimaryNEC { get; set; }

        /// <summary>
        /// The list of the client's secondary NECs.
        /// </summary>
        public virtual IList<NEC> SecondaryNECs { get; set; }

        /// <summary>
        /// The person's supervisor
        /// </summary>
        public virtual string Supervisor { get; set; }

        /// <summary>
        /// The person's work center.
        /// </summary>
        public virtual string WorkCenter { get; set; }

        /// <summary>
        /// The room in which the person works.
        /// </summary>
        public virtual string WorkRoom { get; set; }

        /// <summary>
        /// A free form text field intended to let the client store the shift of a person - however the client wants to do that.
        /// </summary>
        public virtual string Shift { get; set; }

        /// <summary>
        /// The comments section for the work page
        /// </summary>
        public virtual string WorkRemarks { get; set; }

        /// <summary>
        /// The person's duty status
        /// </summary>
        public virtual DutyStatus DutyStatus { get; set; }

        /// <summary>
        /// The person's UIC
        /// </summary>
        public virtual UIC UIC { get; set; }

        /// <summary>
        /// The date/time that the person arrived at the command.
        /// </summary>
        public virtual DateTime? DateOfArrival { get; set; }

        /// <summary>
        /// The client's job title.
        /// </summary>
        public virtual string JobTitle { get; set; }

        /// <summary>
        /// The date/time of the end of active obligatory service (EAOS) for the person.
        /// </summary>
        public virtual DateTime? EAOS { get; set; }

        /// <summary>
        /// The member's projected rotation date.
        /// </summary>
        public virtual DateTime? PRD { get; set; }

        /// <summary>
        /// The date/time that the client left/will leave the command.
        /// </summary>
        public virtual DateTime? DateOfDeparture { get; set; }

        /// <summary>
        /// Represents this person's current muster status for the current muster day.  This property is intended to be updated only by the muster endpoints, not generic updates.
        /// </summary>
        public virtual MusterRecord CurrentMusterRecord { get; set; }

        /// <summary>
        /// The person's watch qualification.
        /// </summary>
        public virtual IList<WatchQualification> WatchQualifications { get; set; }

        /// <summary>
        /// The type of billet this person is assigned to.
        /// </summary>
        public virtual BilletAssignment BilletAssignment { get; set; }

        #endregion

        #region Contacts Properties

        /// <summary>
        /// The email addresses of this person.
        /// </summary>
        public virtual IList<EmailAddress> EmailAddresses { get; set; }

        /// <summary>
        /// The Phone Numbers of this person.
        /// </summary>
        public virtual IList<PhoneNumber> PhoneNumbers { get; set; }

        /// <summary>
        /// The Physical Addresses of this person
        /// </summary>
        public virtual IList<PhysicalAddress> PhysicalAddresses { get; set; }

        /// <summary>
        /// Instructions from the user on what avenues of contact to follow in the case of an emergency.
        /// </summary>
        public virtual string EmergencyContactInstructions { get; set; }

        /// <summary>
        /// A free form text field intended to allow the user to make comments about their contact fields.
        /// </summary>
        public virtual string ContactRemarks { get; set; }

        #endregion

        #region Account

        /// <summary>
        /// A boolean indicating whether or not this account has been claimed.
        /// </summary>
        public virtual bool IsClaimed { get; set; }

        /// <summary>
        /// The client's username.
        /// </summary>
        public virtual string Username { get; set; }

        /// <summary>
        /// The client's hashed password.
        /// </summary>
        public virtual string PasswordHash { get; set; }

        /// <summary>
        /// The list of the person's permissions.  This is not persisted in the database.  Only the names are.
        /// </summary>
        public virtual List<Authorization.Groups.PermissionGroup> PermissionGroups { get; set; }

        /// <summary>
        /// The list of the person's permissions as they are stored in the database.
        /// </summary>
        public virtual IList<string> PermissionGroupNames { get; set; }

        /// <summary>
        /// A list containing account history events, these are events that track things like login, password reset, etc.
        /// </summary>
        public virtual IList<AccountHistoryEvent> AccountHistory { get; set; }

        /// <summary>
        /// A list containing all changes that have every occurred to the profile.
        /// </summary>
        public virtual IList<Change> Changes { get; set; }

        /// <summary>
        /// The list of those events to which this person is subscribed.
        /// </summary>
        public virtual IList<ChangeEventSubscription> SubscribedEvents { get; set; }

        #endregion

        #endregion

        #region Overrides

        /// <summary>
        /// Returns a friendly name for this user in the form: Atwood, Daniel Kurt Roger
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("{0}, {1} {2}", LastName, FirstName, MiddleName);
        }

        #endregion

        #region ctors



        public Person()
        {
            UserPreferences = new Dictionary<string, string>();
        }

        #endregion

        #region Helper Methods
        
        /// <summary>
        /// Returns an object containing two properties: this object's Id and this object's .ToString in a parameter called FriendlyName.  Intended for use with DTOs.
        /// </summary>
        /// <returns></returns>
        public virtual BasicPersonDTO ToBasicPerson()
        {
            return new BasicPersonDTO
            {
                Id = this.Id,
                FriendlyName = this.ToString()
            };
        }
        
        /// <summary>
        /// Returns a boolean indicating if this person is in the same command as the given person.
        /// </summary>
        /// <param name="person"></param>
        /// <returns></returns>
        public virtual bool IsInSameCommandAs(Person person)
        {
            if (person == null || this.Command == null || person.Command == null)
                return false;

            return this.Command.Id == person.Command.Id;
        }

        /// <summary>
        /// Returns a boolean indicating that this person is in the same command and department as the given person.
        /// </summary>
        /// <param name="person"></param>
        /// <returns></returns>
        public virtual bool IsInSameDepartmentAs(Person person)
        {
            if (person == null || this.Department == null || person.Department == null)
                return false;

            return IsInSameCommandAs(person) && this.Department.Id == person.Department.Id;
        }

        /// <summary>
        /// Returns a boolean indicating that this person is in the same command, department, and division as the given person.
        /// </summary>
        /// <param name="person"></param>
        /// <returns></returns>
        public virtual bool IsInSameDivisionAs(Person person)
        {
            if (person == null || this.Division == null || person.Division == null)
                return false;

            return IsInSameDepartmentAs(person) && this.Division.Id == person.Division.Id;
        }

        /// <summary>
        /// Determines if this person is an officer.
        /// </summary>
        /// <returns></returns>
        public virtual bool IsOfficer()
        {
            return this.Paygrade != Paygrades.CON && this.Paygrade.ToString().Contains("O");
        }

        /// <summary>
        /// Determines if this person is enlisted.
        /// </summary>
        /// <returns></returns>
        public virtual bool IsEnlisted()
        {
            return this.Paygrade.ToString().Contains("E") && !IsOfficer();
        }

        /// <summary>
        /// Gets this person's chain of command.
        /// </summary>
        /// <returns></returns>
        public virtual Dictionary<ChainsOfCommand, Dictionary<ChainOfCommandLevels, List<DTOs.BasicPersonDTO>>> GetChainOfCommand()
        {
            //Our result
            var result = new Dictionary<ChainsOfCommand, Dictionary<ChainOfCommandLevels, List<BasicPersonDTO>>>();

            //Populate the dictionary
            foreach (var chainOfCommand in Enum.GetValues(typeof(ChainsOfCommand)).Cast<ChainsOfCommand>())
            {
                result.Add(chainOfCommand, new Dictionary<ChainOfCommandLevels, List<BasicPersonDTO>>());
                foreach (var level in Enum.GetValues(typeof(ChainOfCommandLevels)).Cast<ChainOfCommandLevels>())
                {
                    result[chainOfCommand].Add(level, new List<BasicPersonDTO>());
                }
            }

            var permissionGroupNamesProperty = PropertySelector.SelectPropertiesFrom<Person>(x => x.PermissionGroupNames).First();

            foreach (var groupLevel in new[] { ChainOfCommandLevels.Command, 
                                          ChainOfCommandLevels.Department, 
                                          ChainOfCommandLevels.Division })
            {
                var permissionGroups = Authorization.Groups.PermissionGroup.AllPermissionGroups
                                        .Where(x => x.AccessLevel == groupLevel)
                                        .ToList();

                using (var session = NHibernateHelper.CreateStatefulSession())
                {
                    var queryString = "from Person as person where (";
                    for (var x = 0; x < permissionGroups.Count(); x++)
                    {
                        queryString += " '{0}' in elements(person.{1}) ".FormatS(permissionGroups[x].GroupName, permissionGroupNamesProperty.Name);
                        if (x + 1 != permissionGroups.Count)
                            queryString += " or ";
                    }
                    queryString += " ) ";

                    NHibernate.IQuery query;

                    switch (groupLevel)
                    {
                        case ChainOfCommandLevels.Command:
                            {
                                if (this.Command == null)
                                    continue;

                                queryString += " and person.Command = :command";
                                query = session.CreateQuery(queryString)
                                    .SetParameter("command", this.Command);
                                break;
                            }
                        case ChainOfCommandLevels.Department:
                            {
                                if (this.Command == null || this.Department == null)
                                    continue;

                                queryString += " and person.Command = :command and person.Department = :department";
                                query = session.CreateQuery(queryString)
                                    .SetParameter("command", this.Command)
                                    .SetParameter("department", this.Department);
                                break;
                            }
                        case ChainOfCommandLevels.Division:
                            {
                                if (this.Command == null || this.Department == null || this.Division == null)
                                    continue;

                                queryString += " and person.Command = :command and person.Department = :department and person.Division = :division";
                                query = session.CreateQuery(queryString)
                                    .SetParameter("command", this.Command)
                                    .SetParameter("department", this.Department)
                                    .SetParameter("division", this.Division);
                                break;
                            }
                        default:
                            {
                                throw new NotImplementedException("Hit default in the chain of command switch.");
                            }
                    }

                    var persons = query.List<Person>();
                    
                    //Go through all the results.
                    foreach (var person in persons)
                    {
                        //Collect the person's highest level permission in each chain of command.
                        var highestLevels = new Dictionary<ChainsOfCommand, ChainOfCommandLevels>();

                        //Here, let's make sure to ignore the developers permission group and the admin permission group.
                        foreach (var group in permissionGroups.Where(x => person.PermissionGroupNames.Contains(x.GroupName, StringComparer.CurrentCultureIgnoreCase)))
                        {
                            if (group.GetType() != typeof(Authorization.Groups.Definitions.Developers) && group.GetType() != typeof(Authorization.Groups.Definitions.Admin))
                            {
                                foreach (var chainOfCommand in group.ChainsOfCommandMemberOf)
                                {
                                    //This is just a check to make sure we're doing this right.
                                    if (group.AccessLevel != groupLevel)
                                        throw new Exception("During the GetChaindOfCommand check, we accessed a group level that was unintended.");

                                    //Now here we need to ask "Is the person in the same access level as the person in question?"
                                    //Meaning, if the access level is division, are they in the same division?
                                    highestLevels[chainOfCommand] = group.AccessLevel;
                                }
                            }

                        }

                        //Now just add them to the corresponding lists.
                        foreach (var highestLevel in highestLevels)
                        {
                            result[highestLevel.Key][highestLevel.Value].Add(person.ToBasicPerson());
                        }
                    }
                }
            }

            return result;
        }

        #endregion

        #region Startup Methods

        /// <summary>
        /// Fills the database with random, garbage data.
        /// </summary>
        [StartMethod(Priority = 3)]
        private static void GIGO(CLI.Options.LaunchOptions launchOptions)
        {
            if (launchOptions.GIGO <= 0)
            {
                Log.Info("Skipping GIGO...");
                return;
            }

            Log.Info("Beginning GIGO.  We will now create {0} record(s).".FormatS(launchOptions.GIGO));

            using (var session = NHibernateHelper.CreateStatefulSession())
            using (var transaction = session.BeginTransaction())
            {
                try
                {

                    List<UIC> uics = new List<UIC>();
                    for(int i = 0; i < Utilities.GetRandomNumber(5, 10); i++)
                    {
                        uics.Add(new UIC
                        {
                            Value = Utilities.RandomString(5),
                            Description = Utilities.RandomString(8),
                            Id = Guid.NewGuid()
                        });

                        session.Save(uics.Last());
                        session.Flush();
                    }

                    List<Command> commands = new List<Command>();
                    for (int x = 0; x < Utilities.GetRandomNumber(1, 1); x++)
                    {
                        commands.Add(new Command
                        {
                            Departments = new List<Department>(),
                            Description = Utilities.RandomString(8),
                            Value = Utilities.RandomString(8),
                            Id = Guid.NewGuid()
                        });

                        session.Save(commands.Last());
                        session.Flush();

                        for (int y = 0; y < Utilities.GetRandomNumber(2, 5); y++)
                        {
                            commands.Last().Departments.Add(new Department
                            {
                                Command = commands.Last(),
                                Description = Utilities.RandomString(8),
                                Divisions = new List<Division>(),
                                Id = Guid.NewGuid(),
                                Value = Utilities.RandomString(8)
                            });

                            session.Save(commands.Last().Departments.Last());
                            session.Flush();

                            for (int z = 0; z < Utilities.GetRandomNumber(2, 5); z ++)
                            {
                                commands.Last().Departments.Last().Divisions.Add(new Division
                                {
                                     Department = commands.Last().Departments.Last(),
                                     Description = Utilities.RandomString(8),
                                     Id = Guid.NewGuid(),
                                     Value = Utilities.RandomString(8)
                                });

                                session.Save(commands.Last().Departments.Last().Divisions.Last());
                                session.Flush();
                            }
                        }
                    }

                    int current = 0;
                    for (int x = 0; x < launchOptions.GIGO; x++)
                    {
                        List<string> permissionGroupNames;
                        
                        if (x == 0)
                        {
                            permissionGroupNames = new List<string> { new Authorization.Groups.Definitions.Developers().GroupName };
                        }
                        else
                        {
                            permissionGroupNames = Authorization.Groups.PermissionGroup.AllPermissionGroups
                                    .OrderBy(y => Utilities.GetRandomNumber(0, 100))
                                    .Take(Utilities.GetRandomNumber(0, Authorization.Groups.PermissionGroup.AllPermissionGroups.Count))
                                    .Select(y => y.GroupName)
                                    .ToList();
                        }

                        Command command = commands.ElementAt(Utilities.GetRandomNumber(0, commands.Count - 1));
                        Department department = command.Departments.ElementAt(Utilities.GetRandomNumber(0, command.Departments.Count - 1));
                        Division division = department.Divisions.ElementAt(Utilities.GetRandomNumber(0, department.Divisions.Count - 1));
                        UIC uic = uics.ElementAt(Utilities.GetRandomNumber(0, uics.Count - 1));

                        var person = new Person()
                        {
                            Id = Guid.NewGuid(),
                            LastName = Utilities.RandomString(8),
                            FirstName = Utilities.RandomString(8),
                            MiddleName = Utilities.RandomString(8),
                            Command = command,
                            Department = department,
                            Division = division,
                            UIC = uic,
                            SSN = Utilities.GenerateSSN(),
                            DoDId = Utilities.GenerateDoDId(),
                            IsClaimed = true,
                            Username = "user" + x.ToString(),
                            PasswordHash = ClientAccess.PasswordHash.CreateHash("asdfasdfasdf"),
                            Sex = Sexes.AllSexes.ElementAt(Utilities.GetRandomNumber(0, Sexes.AllSexes.Count - 1)),
                            EmailAddresses = new List<EmailAddress>()
                            {
                                new EmailAddress
                                {
                                    Address = "{0}@{1}.com".FormatS(Utilities.RandomString(6), Utilities.RandomString(6)),
                                    IsContactable = true,
                                    IsPreferred = true
                                }
                            },
                            DateOfBirth = new DateTime(Utilities.GetRandomNumber(1970, 2000), Utilities.GetRandomNumber(1, 12), Utilities.GetRandomNumber(1, 28)),
                            DateOfArrival = new DateTime(Utilities.GetRandomNumber(1970, 2000), Utilities.GetRandomNumber(1, 12), Utilities.GetRandomNumber(1, 28)),
                            EAOS = new DateTime(Utilities.GetRandomNumber(1970, 2000), Utilities.GetRandomNumber(1, 12), Utilities.GetRandomNumber(1, 28)),
                            PRD = new DateTime(Utilities.GetRandomNumber(1970, 2000), Utilities.GetRandomNumber(1, 12), Utilities.GetRandomNumber(1, 28)),
                            Paygrade = Paygrades.AllPaygrades.ElementAt(Utilities.GetRandomNumber(0, Paygrades.AllPaygrades.Count - 1)),
                            DutyStatus = DutyStatuses.AllDutyStatuses.ElementAt(Utilities.GetRandomNumber(0, DutyStatuses.AllDutyStatuses.Count - 1)),
                            PermissionGroupNames = permissionGroupNames
                        };

                        person.CurrentMusterRecord = MusterRecord.CreateDefaultMusterRecordForPerson(person, DateTime.UtcNow);

                        person.AccountHistory = new List<AccountHistoryEvent> { new AccountHistoryEvent
                        {
                            AccountHistoryEventType = AccountHistoryTypes.Creation,
                            EventTime = DateTime.UtcNow
                        } };

                        current++;

                        if (current % 100 == 0)
                        {
                            Log.Info("Completed {0}% of the garbage...".FormatS(Math.Round(((double)current / (double)launchOptions.GIGO) * 100, 2)));
                        }
                        session.Save(person);
                    }

                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    return;
                }
            }
            
        }

        /// <summary>
        /// Loads all persons from the database, thus initializing most of the 2nd level cache, and tells the host how many persons we have in the database.
        /// <para />
        /// Also, this method will assert that Atwood exists in the database.
        /// </summary>
        [StartMethod(Priority = 7)]
        private static void ReadPersons(CLI.Options.LaunchOptions launchOptions)
        {
            using (var session = NHibernateHelper.CreateStatefulSession())
            using (var transaction = session.BeginTransaction())
            {
                try
                {
                    //Give the listener the current row count.
                    Log.Info("Found {0} person(s).".FormatS(session.QueryOver<Person>().RowCount()));

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
        /// Maps a person to the database.
        /// </summary>
        public class PersonMapping : ClassMap<Person>
        {
            /// <summary>
            /// Maps a person to the database.
            /// </summary>
            public PersonMapping()
            {
                Id(x => x.Id).GeneratedBy.Assigned();

                References(x => x.Ethnicity).Nullable();
                References(x => x.ReligiousPreference).Nullable();
                References(x => x.Designation).Nullable();
                References(x => x.Division).Nullable();
                References(x => x.Department).Nullable();
                References(x => x.Command).Nullable();
                References(x => x.UIC).Nullable();
                References(x => x.Paygrade).Not.Nullable();
                References(x => x.CurrentMusterRecord).Cascade.All().Nullable();
                References(x => x.DutyStatus).Not.Nullable();
                References(x => x.Sex).Not.Nullable();
                References(x => x.BilletAssignment);

                Map(x => x.LastName).Not.Nullable().Length(40);
                Map(x => x.FirstName).Not.Nullable().Length(40);
                Map(x => x.MiddleName).Nullable().Length(40);
                Map(x => x.SSN).Not.Nullable().Length(40).Unique();
                Map(x => x.DoDId).Unique();
                Map(x => x.DateOfBirth).Not.Nullable();
                Map(x => x.Remarks).Nullable().Length(150);
                Map(x => x.Supervisor).Nullable().Length(40);
                Map(x => x.WorkCenter).Nullable().Length(40);
                Map(x => x.WorkRoom).Nullable().Length(40);
                Map(x => x.Shift).Nullable().Length(40);
                Map(x => x.WorkRemarks).Nullable().Length(150);
                Map(x => x.DateOfArrival).Not.Nullable();
                Map(x => x.JobTitle).Nullable().Length(40);
                Map(x => x.EAOS).CustomType<UtcDateTimeType>();
                Map(x => x.PRD).CustomType<UtcDateTimeType>();
                Map(x => x.DateOfDeparture).Nullable().CustomType<UtcDateTimeType>();
                Map(x => x.EmergencyContactInstructions).Nullable().Length(150);
                Map(x => x.ContactRemarks).Nullable().Length(150);
                Map(x => x.IsClaimed).Not.Nullable().Default(false.ToString());
                Map(x => x.Username).Nullable().Length(40).Unique();
                Map(x => x.PasswordHash).Nullable().Length(100);
                Map(x => x.Suffix).Nullable().Length(40);
                Map(x => x.GTCTrainingDate).Nullable().CustomType<UtcDateTimeType>();
                Map(x => x.ADAMSTrainingDate).Nullable().CustomType<UtcDateTimeType>();
                Map(x => x.HasCompletedAWARE).Not.Nullable().Default(false.ToString());

                References(x => x.PrimaryNEC);
                HasManyToMany(x => x.SecondaryNECs).Cascade.All();

                HasMany(x => x.AccountHistory).Cascade.All();
                HasMany(x => x.Changes).Cascade.All().Inverse();
                HasMany(x => x.EmailAddresses).Cascade.All();
                HasMany(x => x.PhoneNumbers).Cascade.All();
                HasMany(x => x.PhysicalAddresses).Cascade.All();
                HasMany(x => x.SubscribedEvents).Cascade.All();

                HasManyToMany(x => x.WatchQualifications).Cascade.All();

                HasMany(x => x.PermissionGroupNames)
                    .KeyColumn("PersonId")
                    .Element("PermissionGroupName");

                HasMany(x => x.UserPreferences)
                    .AsMap<string>(index =>
                        index.Column("PreferenceKey").Type<string>(), element =>
                        element.Column("PreferenceValue").Type<string>())
                    .Cascade.All();

                Cache.ReadWrite();
            }
        }

        /// <summary>
        /// Validates a person object.
        /// </summary>
        public class PersonValidator : AbstractValidator<Person>
        {
            /// <summary>
            /// Validates a person object.
            /// </summary>
            public PersonValidator()
            {
                RuleFor(x => x.Id).NotEmpty();
                RuleFor(x => x.LastName).NotEmpty().Length(1, 40)
                    .WithMessage("The last name must not be left blank and must not exceed 40 characters.");
                RuleFor(x => x.FirstName).Length(0, 40)
                    .WithMessage("The first name must not exceed 40 characters.");
                RuleFor(x => x.MiddleName).Length(0, 40)
                    .WithMessage("The middle name must not exceed 40 characters.");
                RuleFor(x => x.Suffix).Length(0, 40)
                    .WithMessage("The suffix must not exceed 40 characters.");
                RuleFor(x => x.SSN).NotEmpty().Must(x => System.Text.RegularExpressions.Regex.IsMatch(x, @"^(?!\b(\d)\1+-(\d)\1+-(\d)\1+\b)(?!123-45-6789|219-09-9999|078-05-1120)(?!666|000|9\d{2})\d{3}(?!00)\d{2}(?!0{4})\d{4}$"))
                    .WithMessage("The SSN must be valid and contain only numbers.");
                RuleFor(x => x.DateOfBirth).NotEmpty()
                    .WithMessage("The DOB must not be left blank.");
                RuleFor(x => x.PRD).NotEmpty()
                    .WithMessage("The DOB must not be left blank.");
                RuleFor(x => x.Sex).NotNull()
                    .WithMessage("The sex must not be left blank.");
                RuleFor(x => x.Remarks).Length(0, 150)
                    .WithMessage("Remarks must not exceed 150 characters.");
                RuleFor(x => x.Ethnicity).Must(x =>
                    {
                        if (x == null)
                            return true;

                        Ethnicity ethnicity = NHibernateHelper.CreateStatefulSession().Get<Ethnicity>(x.Id);

                        if (ethnicity == null)
                            return false;

                        return ethnicity.Equals(x);
                    })
                    .WithMessage("The ethnicity wasn't valid.  It must match exactly a list item in the database.");
                RuleFor(x => x.ReligiousPreference).Must(x =>
                    {
                        if (x == null)
                            return true;

                        ReligiousPreference pref = NHibernateHelper.CreateStatefulSession().Get<ReligiousPreference>(x.Id);

                        if (pref == null)
                            return false;

                        return pref.Equals(x);
                    })
                    .WithMessage("The religious preference wasn't valid.  It must match exactly a list item in the database.");
                RuleFor(x => x.Designation).Must(x =>
                    {
                        if (x == null)
                            return true;

                        Designation designation = NHibernateHelper.CreateStatefulSession().Get<Designation>(x.Id);

                        if (designation == null)
                            return false;

                        return designation.Equals(x);
                    })
                    .WithMessage("The designation wasn't valid.  It must match exactly a list item in the database.");
                RuleFor(x => x.Division).Must((person, x) =>
                    {
                        if (x == null)
                            return true;

                        Division division = NHibernateHelper.CreateStatefulSession().Get<Division>(x.Id);

                        if (division == null)
                            return false;

                        return division.Equals(x);
                    })
                    .WithMessage("The division wasn't a valid division.  It must match exactly.");
                RuleFor(x => x.Department).Must(x =>
                    {
                        if (x == null)
                            return true;

                        Department department = NHibernateHelper.CreateStatefulSession().Get<Department>(x.Id);

                        if (department == null)
                            return false;

                        return department.Equals(x);
                    })
                    .WithMessage("The department was invalid.");
                RuleFor(x => x.Command).Must(x =>
                    {
                        if (x == null)
                            return true;

                        Command command = NHibernateHelper.CreateStatefulSession().Get<Command>(x.Id);

                        if (command == null)
                            return false;

                        return command.Equals(x);
                    })
                    .WithMessage("The command was invalid.");
                RuleFor(x => x.PrimaryNEC).Must((person, x) =>
                    {
                        if (x == null)
                            return true;

                        NEC nec = NHibernateHelper.CreateStatefulSession().Get<NEC>(x.Id);

                        if (nec == null)
                            return false;

                        if (!nec.Equals(x))
                            return false;

                        //Now let's also make sure this isn't in the secondary NECs.
                        if (person.SecondaryNECs.Any(y => y.Id == x.Id))
                            return false;

                        return true;
                    })
                    .WithMessage("The primary NEC must not exist in the secondary NECs list.");
                RuleFor(x => x.Supervisor).Length(0, 40)
                    .WithMessage("The supervisor field may not be longer than 40 characters.");
                RuleFor(x => x.WorkCenter).Length(0, 40)
                    .WithMessage("The work center field may not be longer than 40 characters.");
                RuleFor(x => x.WorkRoom).Length(0, 40)
                    .WithMessage("The work room field may not be longer than 40 characters.");
                RuleFor(x => x.Shift).Length(0, 40)
                    .WithMessage("The shift field may not be longer than 40 characters.");
                RuleFor(x => x.WorkRemarks).Length(0, 150)
                    .WithMessage("The work remarks field may not be longer than 150 characters.");
                RuleFor(x => x.UIC).Must(x =>
                    {
                        if (x == null)
                            return true;

                        UIC uic = NHibernateHelper.CreateStatefulSession().Get<UIC>(x.Id);

                        if (uic == null)
                            return false;

                        return uic.Equals(x);
                    })
                    .WithMessage("The UIC was invalid.");
                RuleFor(x => x.JobTitle).Length(0, 40)
                    .WithMessage("The job title may not be longer than 40 characters.");
                RuleFor(x => x.UserPreferences).Must((person, x) =>
                    {
                        return x.Keys.Count <= 20;
                    })
                    .WithMessage("You may not submit more than 20 preference keys.");
                RuleForEach(x => x.UserPreferences).Must((person, x) =>
                    {
                        return x.Value.Length <= 1000;
                    })
                    .WithMessage("No preference value may be more than 1000 characters.");

                When(x => x.IsClaimed, () =>
                {
                    RuleFor(x => x.EmailAddresses).Must((person, x) =>
                    {
                        return x.Any(y => y.IsDodEmailAddress);
                    }).WithMessage("You must have at least one mail.mil address.");
                });

                //Set validations
                RuleFor(x => x.EmailAddresses)
                    .SetCollectionValidator(new EmailAddress.EmailAddressValidator());
                RuleFor(x => x.PhoneNumbers)
                    .SetCollectionValidator(new PhoneNumber.PhoneNumberValidator());
                RuleFor(x => x.PhysicalAddresses)
                    .SetCollectionValidator(new PhysicalAddress.PhysicalAddressValidator());
            }

        }

        /// <summary>
        /// Provides searching strategies for the person object.
        /// </summary>
        public class PersonQueryProvider : QueryStrategy<Person>
        {
            /// <summary>
            /// Provides searching strategies for the person object.
            /// </summary>
            public PersonQueryProvider()
            {
                ForProperties(PropertySelector.SelectPropertiesFrom<Person>(
                    x => x.Id,
                    x => x.SSN,
                    x => x.Suffix,
                    x => x.Remarks,
                    x => x.Supervisor,
                    x => x.WorkCenter,
                    x => x.WorkRoom,
                    x => x.Shift,
                    x => x.WorkRemarks,
                    x => x.JobTitle,
                    x => x.EmergencyContactInstructions,
                    x => x.ContactRemarks,
                    x => x.DoDId))
                .AsType(SearchDataTypes.String)
                .CanBeUsedIn(QueryTypes.Advanced)
                .UsingStrategy(token =>
                {
                    return Restrictions.InsensitiveLike(token.SearchParameter.Key.Name, token.SearchParameter.Value.ToString(), MatchMode.Anywhere);
                });

                ForProperties(PropertySelector.SelectPropertiesFrom<Person>(
                    x => x.LastName,
                    x => x.FirstName,
                    x => x.MiddleName))
                .UsedAsIdentifiers()
                .AsType(SearchDataTypes.String)
                .CanBeUsedIn(QueryTypes.Advanced, QueryTypes.Simple)
                .UsingStrategy(token =>
                {
                    return Restrictions.InsensitiveLike(token.SearchParameter.Key.Name, token.SearchParameter.Value.ToString(), MatchMode.Anywhere);
                });

                ForProperties(PropertySelector.SelectPropertiesFrom<Person>(
                    x => x.HasCompletedAWARE))
                .AsType(SearchDataTypes.Boolean)
                .CanBeUsedIn(QueryTypes.Advanced)
                .UsingStrategy(token =>
                    {
                        bool value;
                        try 
	                    {	        
		                    value = (bool)token.SearchParameter.Value;
	                    }
	                    catch (Exception)
	                    {
                            token.Errors.Add("An error occurred while parsing your boolean search value.");
                            return null;
	                    }

                        return Restrictions.Eq(token.SearchParameter.Key.Name, value);
                    });

                ForProperties(PropertySelector.SelectPropertiesFrom<Person>(
                    x => x.DateOfBirth,
                    x => x.GTCTrainingDate,
                    x => x.ADAMSTrainingDate,
                    x => x.DateOfArrival,
                    x => x.EAOS,
                    x => x.DateOfDeparture,
                    x => x.PRD))
                .AsType(SearchDataTypes.DateTime)
                .CanBeUsedIn(QueryTypes.Advanced)
                .UsingStrategy(token =>
                {
                    //First cast the value given to a JSON array.
                    var value = ((Dictionary<string, DateTime?>)token.SearchParameter.Value);

                    DateTime? from = null;
                    DateTime? to = null;

                    if (value.ContainsKey("From"))
                    {
                        from = value["From"];
                    }

                    if (value.ContainsKey("To"))
                    {
                        to = value["To"];
                    }

                    if (to == null && from == null)
                    {
                        token.Errors.Add("Both dates in your range may not be empty.");
                        return null;
                    }

                    //Do the validation.
                    if ((from.HasValue && to.HasValue) && from > to)
                    {
                        token.Errors.Add("The dates, From:'{0}' and To:'{1}', were invalid.  'From' may not be after 'To'.".FormatS(from, to));
                        return null;
                    }

                    if (from == to)
                    {
                        return Restrictions.And(
                                Restrictions.Ge(token.SearchParameter.Key.Name, from.Value.Date),
                                Restrictions.Le(token.SearchParameter.Key.Name, from.Value.Date.AddHours(24)));
                    }
                    else if (from == null)
                    {
                        return Restrictions.Le(token.SearchParameter.Key.Name, to);
                    }
                    else if (to == null)
                    {
                        return Restrictions.Ge(token.SearchParameter.Key.Name, from);
                    }
                    else
                    {
                        return Restrictions.And(
                                Restrictions.Ge(token.SearchParameter.Key.Name, from),
                                Restrictions.Le(token.SearchParameter.Key.Name, to));
                    }

                });

                ForProperties(PropertySelector.SelectPropertiesFrom<Person>(
                    x => x.Sex))
                .AsType(SearchDataTypes.String)
                .CanBeUsedIn(QueryTypes.Advanced)
                .UsingStrategy(token =>
                {
                    return Subqueries.WhereProperty<Person>(x => x.Sex.Id).In(QueryOver.Of<Sex>().WhereRestrictionOn(x => x.Value).IsInsensitiveLike(token.SearchParameter.Value.ToString(), MatchMode.Anywhere).Select(x => x.Id));
                });

                ForProperties(PropertySelector.SelectPropertiesFrom<Person>(
                    x => x.BilletAssignment))
                .AsType(SearchDataTypes.String)
                .CanBeUsedIn(QueryTypes.Advanced)
                .UsingStrategy(token =>
                {
                    return Subqueries.WhereProperty<Person>(x => x.BilletAssignment.Id).In(QueryOver.Of<BilletAssignment>().WhereRestrictionOn(x => x.Value).IsInsensitiveLike(token.SearchParameter.Value.ToString(), MatchMode.Anywhere).Select(x => x.Id));
                });

                ForProperties(PropertySelector.SelectPropertiesFrom<Person>(
                    x => x.Ethnicity))
                .AsType(SearchDataTypes.String)
                .CanBeUsedIn(QueryTypes.Advanced)
                .UsingStrategy(token =>
                {
                    return Subqueries.WhereProperty<Person>(x => x.Ethnicity.Id).In(QueryOver.Of<Ethnicity>().WhereRestrictionOn(x => x.Value).IsInsensitiveLike(token.SearchParameter.Value.ToString(), MatchMode.Anywhere).Select(x => x.Id));
                });

                ForProperties(PropertySelector.SelectPropertiesFrom<Person>(
                    x => x.ReligiousPreference))
                .AsType(SearchDataTypes.String)
                .CanBeUsedIn(QueryTypes.Advanced)
                .UsingStrategy(token =>
                {
                    return Subqueries.WhereProperty<Person>(x => x.ReligiousPreference.Id).In(QueryOver.Of<ReligiousPreference>().WhereRestrictionOn(x => x.Value).IsInsensitiveLike(token.SearchParameter.Value.ToString(), MatchMode.Anywhere).Select(x => x.Id));
                });

                ForProperties(PropertySelector.SelectPropertiesFrom<Person>(
                    x => x.Paygrade))
                .UsedAsIdentifiers()
                .AsType(SearchDataTypes.String)
                .CanBeUsedIn(QueryTypes.Advanced, QueryTypes.Simple)
                .UsingStrategy(token =>
                {
                    return Subqueries.WhereProperty<Person>(x => x.Paygrade.Id).In(QueryOver.Of<Paygrade>().WhereRestrictionOn(x => x.Value).IsInsensitiveLike(token.SearchParameter.Value.ToString(), MatchMode.Anywhere).Select(x => x.Id));
                });

                ForProperties(PropertySelector.SelectPropertiesFrom<Person>(
                    x => x.Designation))
                .UsedAsIdentifiers()
                .AsType(SearchDataTypes.String)
                .CanBeUsedIn(QueryTypes.Advanced, QueryTypes.Simple)
                .UsingStrategy(token =>
                {
                    return Subqueries.WhereProperty<Person>(x => x.Designation.Id).In(QueryOver.Of<Designation>().WhereRestrictionOn(x => x.Value).IsInsensitiveLike(token.SearchParameter.Value.ToString(), MatchMode.Anywhere).Select(x => x.Id));
                });

                ForProperties(PropertySelector.SelectPropertiesFrom<Person>(
                    x => x.Division))
                .UsedAsIdentifiers()
                .AsType(SearchDataTypes.String)
                .CanBeUsedIn(QueryTypes.Advanced, QueryTypes.Simple)
                .UsingStrategy(token =>
                {
                    return Subqueries.WhereProperty<Person>(x => x.Division.Id).In(QueryOver.Of<Division>().WhereRestrictionOn(x => x.Value).IsInsensitiveLike(token.SearchParameter.Value.ToString(), MatchMode.Anywhere).Select(x => x.Id));
                });

                ForProperties(PropertySelector.SelectPropertiesFrom<Person>(
                    x => x.Department))
                .UsedAsIdentifiers()
                .AsType(SearchDataTypes.String)
                .CanBeUsedIn(QueryTypes.Advanced, QueryTypes.Simple)
                .UsingStrategy(token =>
                {
                    return Subqueries.WhereProperty<Person>(x => x.Department.Id).In(QueryOver.Of<Department>().WhereRestrictionOn(x => x.Value).IsInsensitiveLike(token.SearchParameter.Value.ToString(), MatchMode.Anywhere).Select(x => x.Id));
                });


                ForProperties(PropertySelector.SelectPropertiesFrom<Person>(
                    x => x.Command))
                .UsedAsIdentifiers()
                .AsType(SearchDataTypes.String)
                .CanBeUsedIn(QueryTypes.Advanced, QueryTypes.Simple)
                .UsingStrategy(token =>
                {
                    return Subqueries.WhereProperty<Person>(x => x.Command.Id).In(QueryOver.Of<Command>().WhereRestrictionOn(x => x.Value).IsInsensitiveLike(token.SearchParameter.Value.ToString(), MatchMode.Anywhere).Select(x => x.Id));
                });

                ForProperties(PropertySelector.SelectPropertiesFrom<Person>(
                    x => x.PrimaryNEC))
                .AsType(SearchDataTypes.String)
                .CanBeUsedIn(QueryTypes.Advanced)
                .UsingStrategy(token =>
                {
                    return Subqueries.WhereProperty<Person>(x => x.PrimaryNEC.Id).In(QueryOver.Of<NEC>().WhereRestrictionOn(x => x.Value).IsInsensitiveLike(token.SearchParameter.Value.ToString(), MatchMode.Anywhere).Select(x => x.Id));
                });

                ForProperties(PropertySelector.SelectPropertiesFrom<Person>(
                    x => x.SecondaryNECs))
                .AsType(SearchDataTypes.String)
                .CanBeUsedIn(QueryTypes.Advanced)
                .UsingStrategy(token =>
                {
                    NEC necAlias = null;

                    token.Query = token.Query.JoinAlias(x => x.SecondaryNECs, () => necAlias);

                    return Restrictions.On(() => necAlias.Value).IsInsensitiveLike(token.SearchParameter.Value.ToString(), MatchMode.Anywhere);
                });

                ForProperties(PropertySelector.SelectPropertiesFrom<Person>(
                    x => x.WatchQualifications))
                .AsType(SearchDataTypes.String)
                .CanBeUsedIn(QueryTypes.Advanced)
                .UsingStrategy(token =>
                {
                    WatchQualification qualAlias = null;

                    token.Query = token.Query.JoinAlias(x => x.WatchQualifications, () => qualAlias);

                    return Restrictions.On(() => qualAlias.Value).IsInsensitiveLike(token.SearchParameter.Value.ToString(), MatchMode.Anywhere);
                });

                ForProperties(PropertySelector.SelectPropertiesFrom<Person>(
                    x => x.DutyStatus))
                .AsType(SearchDataTypes.String)
                .CanBeUsedIn(QueryTypes.Advanced)
                .UsingStrategy(token =>
                {
                    return Subqueries.WhereProperty<Person>(x => x.DutyStatus.Id).In(QueryOver.Of<DutyStatus>().WhereRestrictionOn(x => x.Value).IsInsensitiveLike(token.SearchParameter.Value.ToString(), MatchMode.Anywhere).Select(x => x.Id));
                });

                ForProperties(PropertySelector.SelectPropertiesFrom<Person>(
                    x => x.UIC))
                .UsedAsIdentifiers()
                .AsType(SearchDataTypes.String)
                .CanBeUsedIn(QueryTypes.Advanced, QueryTypes.Simple)
                .UsingStrategy(token =>
                {
                    return Subqueries.WhereProperty<Person>(x => x.UIC.Id).In(QueryOver.Of<UIC>().WhereRestrictionOn(x => x.Value).IsInsensitiveLike(token.SearchParameter.Value.ToString(), MatchMode.Anywhere).Select(x => x.Id));
                });

                ForProperties(PropertySelector.SelectPropertiesFrom<Person>(
                    x => x.CurrentMusterRecord))
                .AsType(SearchDataTypes.String)
                .CanBeUsedIn(QueryTypes.Advanced)
                .UsingStrategy(token =>
                {
                    return Subqueries.WhereProperty<Person>(x => x.CurrentMusterRecord.Id).In(QueryOver.Of<MusterRecord>().Where(Restrictions.Disjunction()
                        .Add<MusterRecord>(x => x.Command.IsInsensitiveLike(token.SearchParameter.Value.ToString(), MatchMode.Anywhere))
                        .Add<MusterRecord>(x => x.Department.IsInsensitiveLike(token.SearchParameter.Value.ToString(), MatchMode.Anywhere))
                        .Add<MusterRecord>(x => x.Division.IsInsensitiveLike(token.SearchParameter.Value.ToString(), MatchMode.Anywhere))
                        .Add<MusterRecord>(x => x.DutyStatus.IsInsensitiveLike(token.SearchParameter.Value.ToString(), MatchMode.Anywhere))
                        .Add<MusterRecord>(x => x.MusterStatus.IsInsensitiveLike(token.SearchParameter.Value.ToString(), MatchMode.Anywhere))
                        .Add<MusterRecord>(x => x.Paygrade.IsInsensitiveLike(token.SearchParameter.Value.ToString(), MatchMode.Anywhere))
                        .Add<MusterRecord>(x => x.UIC.IsInsensitiveLike(token.SearchParameter.Value.ToString(), MatchMode.Anywhere)))
                        .And(x => x.MusterDate == MusterRecord.GetMusterDate(DateTime.UtcNow))
                        .Select(x => x.Id));
                });

                ForProperties(PropertySelector.SelectPropertiesFrom<Person>(
                    x => x.EmailAddresses))
                .AsType(SearchDataTypes.String)
                .CanBeUsedIn(QueryTypes.Advanced)
                .UsingStrategy(token =>
                {
                    EmailAddress addressAlias = null;
                    token.Query = token.Query.JoinAlias(x => x.EmailAddresses, () => addressAlias);

                    return Restrictions.On(() => addressAlias.Address).IsInsensitiveLike(token.SearchParameter.Value.ToString(), MatchMode.Anywhere);
                });

                ForProperties(PropertySelector.SelectPropertiesFrom<Person>(
                    x => x.PhysicalAddresses))
                .AsType(SearchDataTypes.String)
                .CanBeUsedIn(QueryTypes.Advanced)
                .UsingStrategy(token =>
                {
                    PhysicalAddress addressAlias = null;
                    token.Query = token.Query.JoinAlias(x => x.PhysicalAddresses, () => addressAlias);

                    return Restrictions.Disjunction().Add(Restrictions.On(() => addressAlias.City).IsInsensitiveLike(token.SearchParameter.Value.ToString(), MatchMode.Anywhere))
                                                     .Add(Restrictions.On(() => addressAlias.State).IsInsensitiveLike(token.SearchParameter.Value.ToString(), MatchMode.Anywhere))
                                                     .Add(Restrictions.On(() => addressAlias.Address).IsInsensitiveLike(token.SearchParameter.Value.ToString(), MatchMode.Anywhere))
                                                     .Add(Restrictions.On(() => addressAlias.ZipCode).IsInsensitiveLike(token.SearchParameter.Value.ToString(), MatchMode.Anywhere));
                });
            }
        }

    }
}
