using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentNHibernate.Mapping;
using FluentValidation;
using CCServ.Entities.ReferenceLists.Watchbill;
using CCServ.ClientAccess;
using System.Globalization;
using AtwoodUtils;

namespace CCServ.Entities.Watchbill
{
    /// <summary>
    /// Describes a single watchbill, which is a collection of watch days, shifts in those days, and inputs.
    /// </summary>
    public class Watchbill
    {

        #region Properties

        /// <summary>
        /// The unique Id of this watchbill.
        /// </summary>
        public virtual Guid Id { get; set; }

        /// <summary>
        /// The free text title of this watchbill.
        /// </summary>
        public virtual string Title { get; set; }

        /// <summary>
        /// The person who created this watchbill.  This is expected to often be the command watchbill coordinator.
        /// </summary>
        public virtual Person CreatedBy { get; set; }

        /// <summary>
        /// Represents the current state of the watchbill.  Different states should trigger different actions.
        /// </summary>
        public virtual ReferenceLists.Watchbill.WatchbillStatus CurrentState { get; set; }

        /// <summary>
        /// Indicates the last time the state of this watchbill was changed.
        /// </summary>
        public virtual DateTime LastStateChange { get; set; }

        /// <summary>
        /// Contains a reference to the person who caused the last state change.
        /// </summary>
        public virtual Person LastStateChangedBy { get; set; }

        /// <summary>
        /// The collection of all the watch days that make up this watchbill.  Together, they should make en entire watchbill but not necessarily an entire month.
        /// </summary>
        public virtual IList<WatchDay> WatchDays { get; set; }

        /// <summary>
        /// The collection of requirements.  This is how we know who needs to provide inputs and who is available to be on this watchbill.
        /// </summary>
        public virtual IList<WatchInputRequirement> InputRequirements { get; set; }

        /// <summary>
        /// The command at which this watchbill was created.
        /// </summary>
        public virtual ReferenceLists.Command Command { get; set; }

        /// <summary>
        /// This is how the watchbill knows the pool of people to use when assigning inputs, and assigning watches.  
        /// <para />
        /// The elligibilty group also determines the type of watchbill.
        /// </summary>
        public virtual WatchElligibilityGroup ElligibilityGroup { get; set; }

        #endregion

        #region ctors

        /// <summary>
        /// Creates a new watchbill, setting all collection to empty.
        /// </summary>
        public Watchbill()
        {
            WatchDays = new List<WatchDay>();
            InputRequirements = new List<WatchInputRequirement>();
        }

        #endregion

        #region Methods

        /// <summary>
        /// This method is responsible for advancing the watchbill into different states.  
        /// At each state, different actions must be taken.
        /// </summary>
        /// <param name="desiredState"></param>
        /// <param name="setTime"></param>
        /// <param name="person"></param>
        public virtual void SetState(WatchbillStatus desiredState, DateTime setTime, Person person)
        {
            //Don't allow same changes.
            if (this.CurrentState == desiredState)
            {
                throw new Exception("Can't set the state to its same value.");
            }

            //All the different states that we can go through generate a lot of emails.
            //So, let's create a collection of all the emails to send and then fire them all off when the state changing is done.
            var emailMessagesToSend = new List<Email.EmailInterface.CCEmailMessage>();

            //If we set a watchbill's state to initial, then remove all the assignments from it, 
            //leaving a watchbill with only its days and shifts.
            if (desiredState == WatchbillStatuses.Initial)
            {
                for (int x = this.InputRequirements.Count - 1; x >= 0; x--)
                {
                    this.InputRequirements.RemoveAt(x);
                }

                foreach (var watchDay in this.WatchDays)
                {
                    foreach (var shift in watchDay.WatchShifts)
                    {
                        shift.WatchAssignments.Clear();
                        shift.WatchInputs.Clear();
                    }
                }
            }
            //Inform all the people who need to provide inputs along with all the people who are in its chain of command.
            else if (desiredState == WatchbillStatuses.OpenForInputs)
            {
                if (this.CurrentState == null || this.CurrentState != WatchbillStatuses.Initial)
                    throw new Exception("You may not move to the open for inputs state from anything other than the initial state.");

                foreach (var ellPerson in this.ElligibilityGroup.ElligiblePersons)
                {
                    this.InputRequirements.Add(new WatchInputRequirement
                    {
                        Id = Guid.NewGuid(),
                        Person = ellPerson
                    });

                    var model = new Email.Models.WatchbillInputRequiredEmailModel { FriendlyName = ellPerson.ToString(), Watchbill = this.Title };

                    var addresses = ellPerson.EmailAddresses
                        .Where(x => x.IsPreferred)
                        .Select(x => new System.Net.Mail.MailAddress(x.Address, ellPerson.ToString()));

                    //We don't send these emails with parallel threads because of the updating of the watchbill above.  
                    //I'd basically have to create two loops to make sure we don't get any weird cross-thread behaviors.
                    emailMessagesToSend.Add(Email.EmailInterface.CCEmailMessage
                        .CreateDefault()
                        .To(new System.Net.Mail.MailAddress("sundevilgoalie13@gmail.com"))
                        .CC(addresses)
                        .Subject("Watchbill Inputs Required")
                        .HTMLAlternateViewUsingTemplateFromEmbedded("CCServ.Email.Templates.WatchbillInputRequired_HTML.html", model));
                }

                //We now also need to load all persons in the watchbill's chain of command
                var groups = new Authorization.Groups.PermissionGroup[] { new Authorization.Groups.Definitions.CommandQuarterdeckWatchbill(),
                                    new Authorization.Groups.Definitions.DepartmentQuarterdeckWatchbill(),
                                    new Authorization.Groups.Definitions.DivisionQuarterdeckWatchbill() }
                    .Select(x => x.GroupName)
                    .ToList();

                using (var session = DataAccess.NHibernateHelper.CreateStatefulSession())
                using (var transaction = session.BeginTransaction())
                {

                    try
                    {
                        var queryString = "from Person as person where (";
                        for (var x = 0; x < groups.Count; x++)
                        {
                            queryString += " '{0}' in elements(person.{1}) ".FormatS(groups[x], 
                                PropertySelector.SelectPropertyFrom<Person>(y => y.PermissionGroupNames).Name);
                            if (x + 1 != groups.Count)
                                queryString += " or ";
                        }
                        queryString += " ) and person.Command = :command";
                        var persons = session.CreateQuery(queryString)
                            .SetParameter("command", this.Command)
                            .List<Person>();

                        //Now with these people who are the duty holders.
                        var collateralEmailAddresses = persons.Select(x => 
                                    x.EmailAddresses.Where(y => y.IsPreferred).Select(y => new System.Net.Mail.MailAddress(y.Address, x.ToString())));

                        var model = new Email.Models.WatchbillOpenForInputsEmailModel { Watchbill = this.Title };

                        foreach (var addressGroup in collateralEmailAddresses)
                        {
                            emailMessagesToSend.Add(Email.EmailInterface.CCEmailMessage
                                        .CreateDefault()
                                        .To(new System.Net.Mail.MailAddress("sundevilgoalie13@gmail.com"))
                                        .CC(addressGroup)
                                        .Subject("Watchbill Open For Inputs")
                                        .HTMLAlternateViewUsingTemplateFromEmbedded("CCServ.Email.Templates.WatchbillOpenForInputs_HTML.html", model));
                        }
                        
                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }

                
            }
            //Inform everyone in the chain of command that the watchbill is closed for inputs.
            else if (desiredState == WatchbillStatuses.ClosedForInputs)
            {
                if (this.CurrentState == null || this.CurrentState != WatchbillStatuses.OpenForInputs)
                    throw new Exception("You may not move to the closed for inputs state from anything other than the open for inputs state.");

                //We now also need to load all persons in the watchbill's chain of command.
                var groups = new Authorization.Groups.PermissionGroup[] { new Authorization.Groups.Definitions.CommandQuarterdeckWatchbill(),
                                    new Authorization.Groups.Definitions.DepartmentQuarterdeckWatchbill(),
                                    new Authorization.Groups.Definitions.DivisionQuarterdeckWatchbill() }
                    .Select(x => x.GroupName)
                    .ToList();

                using (var session = DataAccess.NHibernateHelper.CreateStatefulSession())
                using (var transaction = session.BeginTransaction())
                {

                    try
                    {
                        var queryString = "from Person as person where (";
                        for (var x = 0; x < groups.Count; x++)
                        {
                            queryString += " '{0}' in elements(person.{1}) ".FormatS(groups[x],
                                PropertySelector.SelectPropertyFrom<Person>(y => y.PermissionGroupNames).Name);
                            if (x + 1 != groups.Count)
                                queryString += " or ";
                        }
                        queryString += " ) and person.Command = :command";
                        var persons = session.CreateQuery(queryString)
                            .SetParameter("command", this.Command)
                            .List<Person>();

                        //Now with these people who are the duty holders.
                        var collateralEmailAddresses = persons.Select(x =>
                                    x.EmailAddresses.Where(y => y.IsPreferred).Select(y => new System.Net.Mail.MailAddress(y.Address, x.ToString())));

                        var model = new Email.Models.WatchbillClosedForInputsEmailModel { Watchbill = this.Title };

                        foreach (var addressGroup in collateralEmailAddresses)
                        {
                            emailMessagesToSend.Add(Email.EmailInterface.CCEmailMessage
                                .CreateDefault()
                                .To(new System.Net.Mail.MailAddress("sundevilgoalie13@gmail.com"))
                                .CC(addressGroup)
                                .Subject("Watchbill Closed For Inputs")
                                .HTMLAlternateViewUsingTemplateFromEmbedded("CCServ.Email.Templates.WatchbillClosedForInputs_HTML.html", model));                
                        }

                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
            //Make sure there are assignments for each shift.  
            //Inform the chain of command that the watchbill is open for review.
            else if (desiredState == WatchbillStatuses.UnderReview)
            {
                if (this.CurrentState == null || this.CurrentState != WatchbillStatuses.ClosedForInputs)
                    throw new Exception("You may not move to the under review state from anything other than the closed for inputs state.");

                if (!this.WatchDays.All(x => x.WatchShifts.All(y => y.WatchAssignments.Any())))
                {
                    throw new Exception("A watchbill may not move into the 'Under Review' state unless the all watch shifts have been assigned.");
                }
                
                //We now also need to load all persons in the watchbill's chain of command.
                var groups = new Authorization.Groups.PermissionGroup[] { new Authorization.Groups.Definitions.CommandQuarterdeckWatchbill(),
                                    new Authorization.Groups.Definitions.DepartmentQuarterdeckWatchbill(),
                                    new Authorization.Groups.Definitions.DivisionQuarterdeckWatchbill() }
                    .Select(x => x.GroupName)
                    .ToList();

                using (var session = DataAccess.NHibernateHelper.CreateStatefulSession())
                using (var transaction = session.BeginTransaction())
                {

                    try
                    {
                        var queryString = "from Person as person where (";
                        for (var x = 0; x < groups.Count; x++)
                        {
                            queryString += " '{0}' in elements(person.{1}) ".FormatS(groups[x],
                                PropertySelector.SelectPropertyFrom<Person>(y => y.PermissionGroupNames).Name);
                            if (x + 1 != groups.Count)
                                queryString += " or ";
                        }
                        queryString += " ) and person.Command = :command";
                        var persons = session.CreateQuery(queryString)
                            .SetParameter("command", this.Command)
                            .List<Person>();

                        //Now with these people who are the duty holders.
                        var collateralEmailAddresses = persons.Select(x =>
                                    x.EmailAddresses.Where(y => y.IsPreferred).Select(y => new System.Net.Mail.MailAddress(y.Address, x.ToString())));

                        var model = new Email.Models.WatchbillClosedForInputsEmailModel { Watchbill = this.Title };

                        foreach (var addressGroup in collateralEmailAddresses)
                        {
                            emailMessagesToSend.Add(Email.EmailInterface.CCEmailMessage
                                .CreateDefault()
                                .To(new System.Net.Mail.MailAddress("sundevilgoalie13@gmail.com"))
                                .CC(addressGroup)
                                .Subject("Watchbill Under Review")
                                .HTMLAlternateViewUsingTemplateFromEmbedded("CCServ.Email.Templates.WatchbillUnderReview_HTML.html", model));
                        }
                                

                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
            //Move the watchbill into its published state, tell everyone who has watch which watches they have.
            //Tell the chain of command the watchbill is published.
            else if (desiredState == WatchbillStatuses.Published)
            {
                if (this.CurrentState == null || this.CurrentState != WatchbillStatuses.UnderReview)
                    throw new Exception("You may not move to the published state from anything other than the under review state.");

                //Let's send an email to each person who is on watch, informing them of their watches.
                var assignmentsByPerson = this.WatchDays
                    .SelectMany(x => x.WatchShifts.SelectMany(y => y.WatchAssignments))
                    .GroupBy(x => x.PersonAssigned);

                foreach (var assignments in assignmentsByPerson)
                {
                    var model = new Email.Models.WatchAssignedEmailModel { FriendlyName = assignments.Key.ToString(), WatchAssignments = assignments.ToList(), Watchbill = this.Title };

                    var emailAddresses = assignments.Key.EmailAddresses
                        .Where(x => x.IsPreferred)
                        .Select(x => new System.Net.Mail.MailAddress(x.Address, assignments.Key.ToString()));

                    emailMessagesToSend.Add(Email.EmailInterface.CCEmailMessage
                        .CreateDefault()
                        .To(new System.Net.Mail.MailAddress("sundevilgoalie13@gmail.com"))
                        .CC(emailAddresses)
                        .Subject("Watch Assigned")
                        .HTMLAlternateViewUsingTemplateFromEmbedded("CCServ.Email.Templates.WatchAssigned_HTML.html", model));
                }

                //Let's send emails to all the coordinators.
                //We now also need to load all persons in the watchbill's chain of command.
                var groups = new Authorization.Groups.PermissionGroup[] { new Authorization.Groups.Definitions.CommandQuarterdeckWatchbill(),
                                    new Authorization.Groups.Definitions.DepartmentQuarterdeckWatchbill(),
                                    new Authorization.Groups.Definitions.DivisionQuarterdeckWatchbill() }
                    .Select(x => x.GroupName)
                    .ToList();

                using (var session = DataAccess.NHibernateHelper.CreateStatefulSession())
                using (var transaction = session.BeginTransaction())
                {

                    try
                    {
                        var queryString = "from Person as person where (";
                        for (var x = 0; x < groups.Count; x++)
                        {
                            queryString += " '{0}' in elements(person.{1}) ".FormatS(groups[x],
                                PropertySelector.SelectPropertyFrom<Person>(y => y.PermissionGroupNames).Name);
                            if (x + 1 != groups.Count)
                                queryString += " or ";
                        }
                        queryString += " ) and person.Command = :command";
                        var persons = session.CreateQuery(queryString)
                            .SetParameter("command", this.Command)
                            .List<Person>();

                        //Now with these people who are the duty holders, get their preferred email addresses.
                        var collateralEmailAddresses = persons.Select(x =>
                                    x.EmailAddresses.Where(y => y.IsPreferred).Select(y => new System.Net.Mail.MailAddress(y.Address, x.ToString())));

                        var model = new Email.Models.WatchbillPublishedEmailModel { Watchbill = this.Title };

                        foreach (var addressGroup in collateralEmailAddresses)
                        {
                            emailMessagesToSend.Add(Email.EmailInterface.CCEmailMessage
                                .CreateDefault()
                                .To(new System.Net.Mail.MailAddress("sundevilgoalie13@gmail.com"))
                                .CC(addressGroup)
                                .Subject("Watchbill Published")
                                .HTMLAlternateViewUsingTemplateFromEmbedded("CCServ.Email.Templates.WatchbillPublished_HTML.html", model));
                        }

                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
            else
            {
                throw new NotImplementedException("Not implemented default case in the set watchbill state method.");
            }

            //Fire off all the messages.
            Parallel.ForEach(emailMessagesToSend, message =>
            {
                //message.SendWithRetryAndFailure(TimeSpan.FromSeconds(1));
            });

            this.CurrentState = desiredState;
            this.LastStateChange = setTime;
            this.LastStateChangedBy = person;
        }

        public virtual void PopulateWatchbill(Person client, DateTime dateTime)
        {
            //First we need to know how many shifts of each type are in this watchbill.
            //And we need to know how many elligible people in each department there are.
            var shifts = this.WatchDays.SelectMany(x => x.WatchShifts).ToList();
            var elligiblePersonsByDepartment = this.ElligibilityGroup.ElligiblePersons.GroupBy(x => x.Department).ToList();
            var shiftsByDepartmentCount = new Dictionary<ReferenceLists.Department, Dictionary<WatchShiftType, int>>();

            foreach (var shift in shifts)
            {
                foreach (var departmentGroup in elligiblePersonsByDepartment)
                {
                    int n3 = departmentGroup.Key.GetHashCode();

                    if (!shiftsByDepartmentCount.ContainsKey(departmentGroup.Key))
                        shiftsByDepartmentCount.Add(departmentGroup.Key, new Dictionary<WatchShiftType, int>());

                    int n1 = shiftsByDepartmentCount.First().Key.GetHashCode();
                    int n2 = departmentGroup.Key.GetHashCode();

                    if (!shiftsByDepartmentCount[departmentGroup.Key].ContainsKey(shift.ShiftType))
                        shiftsByDepartmentCount[departmentGroup.Key].Add(shift.ShiftType, 0);

                    if (!departmentGroup.Any(x => !shift.ShiftType.RequiredWatchQualifications.Except(x.WatchQualifications).Any()))
                        shiftsByDepartmentCount[departmentGroup.Key][shift.ShiftType]++;
                }
            }

            int i = 0;

            //Next, we're going to walk the shifts we have to fill, and see how many are elligible for each shift from each department.
            //We do this to build our available percentages by department.
        }

        #endregion

        /// <summary>
        /// Maps this object to the database.
        /// </summary>
        public class WatchbillMapping : ClassMap<Watchbill>
        {
            /// <summary>
            /// Maps this object to the database.
            /// </summary>
            public WatchbillMapping()
            {
                Id(x => x.Id).GeneratedBy.Assigned();

                References(x => x.CreatedBy).Not.Nullable();
                References(x => x.CurrentState).Not.Nullable();
                References(x => x.Command).Not.Nullable();
                References(x => x.LastStateChangedBy).Not.Nullable();
                References(x => x.ElligibilityGroup).Not.Nullable();

                HasMany(x => x.WatchDays).Cascade.All();
                HasMany(x => x.InputRequirements).Cascade.AllDeleteOrphan();

                Map(x => x.Title).Not.Nullable();
                Map(x => x.LastStateChange).Not.Nullable();
            }
        }

        /// <summary>
        /// Validates the parent object.
        /// </summary>
        public class WatchbillValidator : AbstractValidator<Watchbill>
        {
            /// <summary>
            /// Validates the parent object.
            /// </summary>
            public WatchbillValidator()
            {
                RuleFor(x => x.Title).NotEmpty().Length(1, 50);

                RuleFor(x => x.CreatedBy).NotEmpty();
                RuleFor(x => x.CurrentState).NotEmpty();
                RuleFor(x => x.Command).NotEmpty();
                RuleFor(x => x.LastStateChange).NotEmpty();
                RuleFor(x => x.LastStateChangedBy).NotEmpty();
                RuleFor(x => x.ElligibilityGroup).NotEmpty();

                RuleFor(x => x.WatchDays).SetCollectionValidator(new WatchDay.WatchDayValidator());
                RuleFor(x => x.InputRequirements).SetCollectionValidator(new WatchInputRequirement.WatchInputRequirementValidator());
            }
        }

    }
}
