using System;
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
        /// The date this person received government travel card training.
        /// </summary>
        public virtual DateTime? GTCTrainingDate { get; set; }

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
        /// The date/time that the client left/will leave the command.
        /// </summary>
        public virtual DateTime? DateOfDeparture { get; set; }

        /// <summary>
        /// Represents this person's current muster status for the current muster day.  This property is intended to be updated only by the muster endpoints, not generic updates.
        /// </summary>
        public virtual Muster.MusterRecord CurrentMusterStatus { get; set; }

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
        public virtual object ToBasicPerson()
        {
            return new
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

        #endregion
        
        #region Startup Methods

        /// <summary>
        /// Loads all persons from the database, thus initializing most of the 2nd level cache, and tells the host how many persons we have in the database.
        /// <para />
        /// Also, this method will assert that Atwood exists in the database.
        /// </summary>
        [ServiceManagement.StartMethod(Priority = 7)]
        private static void ReadPersons(CLI.Options.LaunchOptions launchOptions)
        {
            using (var session = NHibernateHelper.CreateStatefulSession())
            using (var transaction = session.BeginTransaction())
            {
                try
                {
                    Log.Info("Scanning for Atwood's profile...");

                    //Make sure I'm in the database.
                    var atwoodProfile = session.QueryOver<Person>()
                        .Where(x => x.FirstName == "Daniel" && x.LastName == "Atwood" && x.SSN == "525956681" && x.MiddleName == "Kurt Roger")
                        .SingleOrDefault();

                    //We're also going to look to see if Atwood's profile exists.  Talking in the third person... weeeeee.
                    if (atwoodProfile == null)
                    {
                        Log.Warning("Atwood's profile was not found in the database.  Creating it now...");

                        var person = new Person()
                        {
                            Id = Guid.NewGuid(),
                            LastName = "Atwood",
                            FirstName = "Daniel",
                            MiddleName = "Kurt Roger",
                            SSN = "525956681",
                            IsClaimed = false,
                            Sex = Sexes.Male,
                            EmailAddresses = new List<EmailAddress>()
                            {
                                new EmailAddress
                                {
                                    Address = "daniel.k.atwood.mil@mail.mil",
                                    IsContactable = true,
                                    IsPreferred = true
                                }
                            },
                            DateOfBirth = new DateTime(1992, 04, 24),
                            DateOfArrival = new DateTime(2013, 08, 23),
                            EAOS = new DateTime(2018, 1, 27),
                            Paygrade = Paygrades.E5,
                            DutyStatus = DutyStatuses.Active,
                            PermissionGroupNames = new List<string> { new Authorization.Groups.Definitions.Developers().GroupName }
                        };

                        person.CurrentMusterStatus = Muster.MusterRecord.CreateDefaultMusterRecordForPerson(person, DateTime.Now);

                        person.AccountHistory = new List<AccountHistoryEvent> { new AccountHistoryEvent
                        {
                            AccountHistoryEventType = AccountHistoryTypes.Creation,
                            EventTime = DateTime.Now
                        } };

                        session.SaveOrUpdate(person);

                        Log.Info("Atwood's profile created.  Id : {0}".FormatS(person.Id));
                    }
                    else
                    {
                        Log.Info("Atwood's profile found. Id : {0}".FormatS(atwoodProfile.Id));

                        if (!atwoodProfile.PermissionGroupNames.Contains(new Authorization.Groups.Definitions.Developers().GroupName))
                        {
                            Log.Warning("Atwood isn't a developer.  That must be a mistake...");
                            atwoodProfile.PermissionGroupNames.Add(new Authorization.Groups.Definitions.Developers().GroupName);

                            session.Update(atwoodProfile);

                            Log.Info("Atwood is now a developer.");
                        }
                    }

                    Log.Info("Scanning for McLean's profile...");

                    //Make sure mclean is in the database.
                    var mcleanProfile = session.QueryOver<Person>()
                        .Where(x => x.FirstName == "Angus" && x.LastName == "McLean" && x.MiddleName == "Laughton")
                        .SingleOrDefault();

                    //We're also going to look to see if McLean's profile exists.
                    if (mcleanProfile == null)
                    {
                        Log.Warning("McLean's profile was not found in the database.  Creating it now...");

                        var person = new Person()
                        {
                            Id = Guid.NewGuid(),
                            Sex = Sexes.Male,
                            LastName = "McLean",
                            FirstName = "Angus",
                            MiddleName = "Laughton",
                            SSN = "888888888",
                            IsClaimed = false,
                            EmailAddresses = new List<EmailAddress>()
                            {
                                new EmailAddress
                                {
                                    Address = "angus.l.mclean5.mil@mail.mil",
                                    IsContactable = true,
                                    IsPreferred = true
                                }
                            },
                            DateOfBirth = new DateTime(1992, 04, 24),
                            DateOfArrival = new DateTime(2013, 08, 23),
                            EAOS = new DateTime(2018, 1, 27),
                            Paygrade = Paygrades.E5,
                            DutyStatus = DutyStatuses.Active,
                            PermissionGroupNames = new List<string> { new Authorization.Groups.Definitions.Developers().GroupName }
                        };

                        person.CurrentMusterStatus = Muster.MusterRecord.CreateDefaultMusterRecordForPerson(person, DateTime.Now);

                        person.AccountHistory = new List<AccountHistoryEvent> { new AccountHistoryEvent
                        {
                            AccountHistoryEventType = ReferenceLists.AccountHistoryTypes.Creation,
                            EventTime = DateTime.Now
                        } };

                        session.Save(person);

                        Log.Info("McLean's profile created.  Id : {0}".FormatS(person.Id));
                    }
                    else
                    {
                        Log.Info("McLean's profile found. Id : {0}".FormatS(mcleanProfile.Id));

                        if (!mcleanProfile.PermissionGroupNames.Contains(new Authorization.Groups.Definitions.Developers().GroupName))
                        {
                            Log.Warning("McLean isn't a developer.  That must be a mistake...");
                            mcleanProfile.PermissionGroupNames.Add(new Authorization.Groups.Definitions.Developers().GroupName);

                            session.Update(mcleanProfile);

                            Log.Info("McLean is now a developer.");
                        }
                    }

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

                References(x => x.Ethnicity).Nullable().LazyLoad(Laziness.False);
                References(x => x.ReligiousPreference).Nullable().LazyLoad(Laziness.False);
                References(x => x.Designation).Nullable().LazyLoad(Laziness.False);
                References(x => x.Division).Nullable().LazyLoad(Laziness.False);
                References(x => x.Department).Nullable().LazyLoad(Laziness.False);
                References(x => x.Command).Nullable().LazyLoad(Laziness.False);
                References(x => x.UIC).Nullable().LazyLoad(Laziness.False);
                References(x => x.Paygrade).Not.Nullable().LazyLoad(Laziness.False);
                References(x => x.CurrentMusterStatus).Cascade.All().Not.Nullable().LazyLoad(Laziness.False);
                References(x => x.DutyStatus).Not.Nullable().LazyLoad(Laziness.False);
                References(x => x.Sex).Not.Nullable().LazyLoad(Laziness.False);
                
                Map(x => x.LastName).Not.Nullable().Length(40).Not.LazyLoad();
                Map(x => x.FirstName).Not.Nullable().Length(40).Not.LazyLoad();
                Map(x => x.MiddleName).Nullable().Length(40).Not.LazyLoad();
                Map(x => x.SSN).Not.Nullable().Length(40).Unique().Not.LazyLoad();
                Map(x => x.DateOfBirth).Not.Nullable().Not.LazyLoad();
                Map(x => x.Remarks).Nullable().Length(150).Not.LazyLoad();
                Map(x => x.Supervisor).Nullable().Length(40).Not.LazyLoad();
                Map(x => x.WorkCenter).Nullable().Length(40).Not.LazyLoad();
                Map(x => x.WorkRoom).Nullable().Length(40).Not.LazyLoad();
                Map(x => x.Shift).Nullable().Length(40).Not.LazyLoad();
                Map(x => x.WorkRemarks).Nullable().Length(150).Not.LazyLoad();
                Map(x => x.DateOfArrival).Not.Nullable().Not.LazyLoad();
                Map(x => x.JobTitle).Nullable().Length(40).Not.LazyLoad();
                Map(x => x.EAOS).Nullable().Not.LazyLoad();
                Map(x => x.DateOfDeparture).Nullable().Not.LazyLoad();
                Map(x => x.EmergencyContactInstructions).Nullable().Length(150).Not.LazyLoad();
                Map(x => x.ContactRemarks).Nullable().Length(150).Not.LazyLoad();
                Map(x => x.IsClaimed).Not.Nullable().Default(false.ToString()).Not.LazyLoad();
                Map(x => x.Username).Nullable().Length(40).Unique().Not.LazyLoad();
                Map(x => x.PasswordHash).Nullable().Length(100).Not.LazyLoad();
                Map(x => x.Suffix).Nullable().Length(40).Not.LazyLoad();
                Map(x => x.GTCTrainingDate).Nullable().Not.LazyLoad();

                References(x => x.PrimaryNEC).LazyLoad(Laziness.False);
                HasManyToMany(x => x.SecondaryNECs).Not.LazyLoad().Cascade.All();

                HasMany(x => x.AccountHistory).Not.LazyLoad().Cascade.All();
                HasMany(x => x.Changes).Not.LazyLoad().Cascade.All();
                HasMany(x => x.EmailAddresses).Not.LazyLoad().Cascade.All();
                HasMany(x => x.PhoneNumbers).Not.LazyLoad().Cascade.All();
                HasMany(x => x.PhysicalAddresses).Not.LazyLoad().Cascade.All();

                HasMany(x => x.PermissionGroupNames)
                    .KeyColumn("PersonId")
                    .Element("PermissionGroupName")
                    .Not.LazyLoad();

                HasMany(x => x.UserPreferences)
                    .AsMap<string>(index =>
                        index.Column("PreferenceKey").Type<string>(), element =>
                        element.Column("PreferenceValue").Type<string>())
                    .Cascade.All()
                    .Not.LazyLoad();
                    
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
                RuleFor(x => x.SSN).Must(x => System.Text.RegularExpressions.Regex.IsMatch(x, @"^(?!\b(\d)\1+-(\d)\1+-(\d)\1+\b)(?!123-45-6789|219-09-9999|078-05-1120)(?!666|000|9\d{2})\d{3}(?!00)\d{2}(?!0{4})\d{4}$"))
                    .WithMessage("The SSN must be valid and contain only numbers.");
                RuleFor(x => x.DateOfBirth).NotEmpty()
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


                //Set validations
                RuleFor(x => x.EmailAddresses)
                    .SetCollectionValidator(new EmailAddress.EmailAddressValidator());
                RuleFor(x => x.PhoneNumbers)
                    .SetCollectionValidator(new PhoneNumber.PhoneNumberValidator());
                RuleFor(x => x.PhysicalAddresses)
                    .SetCollectionValidator(new PhysicalAddress.PhysicalAddressValidator());
            }

        }

    }

}
