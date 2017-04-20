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
        /// The eligibilty group also determines the type of watchbill.
        /// </summary>
        public virtual WatchEligibilityGroup EligibilityGroup { get; set; }

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

                foreach (var ellPerson in this.EligibilityGroup.EligiblePersons)
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
                message.SendWithRetryAndFailure(TimeSpan.FromSeconds(1));
            });

            this.CurrentState = desiredState;
            this.LastStateChange = setTime;
            this.LastStateChangedBy = person;
        }

        /// <summary>
        /// Populates the current watchbill:
        /// 
        /// First we group all the shifts by their type, then we look at all people that are available for that watch.  
        /// We then determine how many people each department is responsible for supplying based as a percentage of the total people.
        /// This causes some shifts not to get assigned so we assign those using the Hamilton assignment method.
        /// </summary>
        /// <param name="client"></param>
        /// <param name="dateTime"></param>
        public virtual void PopulateWatchbill(Person client, DateTime dateTime)
        {
            if (client == null)
                throw new ArgumentNullException("client");

            //First we need to know how many shifts of each type are in this watchbill.
            //And we need to know how many eligible people in each department there are.
            var shuffledShiftsByType = this.WatchDays.SelectMany(x => x.WatchShifts).Distinct().Shuffle().GroupBy(x => x.ShiftType);

            var minWatchDay = this.WatchDays.Min(x => x.Date);

            foreach (var shiftGroup in shuffledShiftsByType)
            {
                var remainingShifts = new List<WatchShift>(shiftGroup.OrderByDescending(x => x.Points));

                //Get all persons from the el group who have all the required watch qualifications for the current watch type.
                var personsByDepartment = this.EligibilityGroup.EligiblePersons
                    .Where(x => shiftGroup.Key.RequiredWatchQualifications.All(y => x.WatchQualifications.Contains(y)))
                    .GroupBy(x => x.Department);

                var totalPersonsWithQuals = personsByDepartment.Select(x => x.Count()).Sum(x => x);

                var assignedShiftsByDepartment = new Dictionary<ReferenceLists.Department, double>();

                var assignablePersonsByDepartment = personsByDepartment.Select(x =>
                {
                    return new KeyValuePair<ReferenceLists.Department, ConditionalForeverList<Person>>(x.Key, new ConditionalForeverList<Person>(x.ToList().OrderBy(person =>
                    {
                        double points = person.WatchAssignments.Where(z => z.CurrentState == WatchAssignmentStates.Completed).Sum(z =>
                        {
                            int totalMonths = (int)Math.Round(z.WatchShift.WatchDays
                            .Select(watchDay => DateTime.UtcNow.Subtract(watchDay.Date).TotalDays / (365.2425 / 12)).Max());

                            return z.WatchShift.Points / (Math.Pow(1.35, totalMonths) + -1);

                        });

                        return points;
                    })));
                }).ToDictionary(x => x.Key, x => x.Value);

                foreach (var personsGroup in personsByDepartment)
                {
                    //It's important to point out here that the assigned shifts will most likely not fall out as a perfect integer.
                    //We'll handle remaining shifts later.  For now, we just need to assign the whole number value of shifts.
                    var assignedShifts = (double)shiftGroup.Count() * ((double)personsGroup.Count() / (double)totalPersonsWithQuals);
                    assignedShiftsByDepartment.Add(personsGroup.Key, assignedShifts);

                    //From our list of shifts, take as many as we're supposed to assign.
                    var shiftsForThisGroup = remainingShifts.Take((int)assignedShifts).ToList();

                    for (int x = 0; x < shiftsForThisGroup.Count; x++)
                    {
                        //Ok, since we're going to assign it, we can go ahead and remove it.
                        remainingShifts.Remove(shiftsForThisGroup[x]);

                        //Determine who is about to stand this watch.
                        if (!assignablePersonsByDepartment[personsGroup.Key].TryNext(person =>
                        {
                            if (shiftsForThisGroup[x].WatchInputs.Any(input => input.IsConfirmed && input.Person.Id == person.Id))
                                return false;

                            if (person.DateOfArrival < DateTime.UtcNow.AddMonths(-3))
                                return false;

                            return true;

                        }, out Person personToAssign))
                            throw new CommandCentralException("A shift has no person that can stand it!  TODO which shift?", ErrorTypes.Validation);

                        //Create the watch assignment.
                        shiftsForThisGroup[x].WatchAssignments.Add(new WatchAssignment
                        {
                            AssignedBy = client,
                            CurrentState = WatchAssignmentStates.Assigned,
                            DateAssigned = dateTime,
                            Id = Guid.NewGuid(),
                            PersonAssigned = personToAssign,
                            WatchShift = shiftsForThisGroup[x]
                        });
                    }
                }

                //At this step, we run into a bit of a problem.  Because the assigned shifts don't come out as perfect integers, we'll have some shifts left over.
                //I chose to use the Hamilton assignment method with the Hare quota here in order to distrubte the rest of the shifts.
                //https://en.wikipedia.org/wiki/Largest_remainder_method
                var finalAssignments = assignedShiftsByDepartment.OrderByDescending(x => x.Value - Math.Truncate(x.Value)).ToList();
                foreach (var shift in remainingShifts)
                {
                    if (!assignablePersonsByDepartment[finalAssignments.First().Key].TryNext(person =>
                    {

                        if (shift.WatchInputs.Any() && shift.WatchInputs.Any(input => !input.IsConfirmed && input.Person.Id == person.Id))
                            return false;

                        if (person.DateOfArrival.HasValue && minWatchDay < person.DateOfArrival.Value.AddMonths(1))
                            return false;

                        if (person.EAOS.HasValue && minWatchDay > person.EAOS.Value.AddMonths(-1))
                            return false;

                        if (person.DateOfBirth.HasValue && shift.WatchDays.Any(day => day.Date == person.DateOfBirth.Value.Date))
                            return false;

                        return true;
                    }, out Person personToAssign))
                        throw new CommandCentralException("A shift has no person that can stand it!  TODO which shift?", ErrorTypes.Validation);

                    shift.WatchAssignments.Add(new WatchAssignment
                    {
                        AssignedBy = client,
                        CurrentState = WatchAssignmentStates.Assigned,
                        DateAssigned = dateTime,
                        Id = Guid.NewGuid(),
                        PersonAssigned = personToAssign,
                        WatchShift = shift
                    });

                    finalAssignments.Remove(finalAssignments.First());
                }
            }
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
                References(x => x.EligibilityGroup).Not.Nullable();

                HasMany(x => x.WatchDays).Cascade.AllDeleteOrphan().Inverse();
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
                RuleFor(x => x.EligibilityGroup).NotEmpty();

                RuleFor(x => x.WatchDays).SetCollectionValidator(new WatchDay.WatchDayValidator());
                RuleFor(x => x.InputRequirements).SetCollectionValidator(new WatchInputRequirement.WatchInputRequirementValidator());

                Custom(watchbill =>
                {
                    var shiftsByType = watchbill.WatchDays.SelectMany(x => x.WatchShifts).Distinct().GroupBy(x => x.ShiftType);

                    List<string> errorElements = new List<string>();

                    foreach (var group in shiftsByType)
                    {
                        var shifts = group.ToList();
                        foreach (var shift in shifts)
                        {
                            var shiftRange = new Itenso.TimePeriod.TimeRange(shift.Range.Start, shift.Range.End, false);
                            foreach (var otherShift in shifts.Where(x => x.Id != shift.Id))
                            {
                                var otherShiftRange = new Itenso.TimePeriod.TimeRange(otherShift.Range.Start, otherShift.Range.End, false);
                                if (shiftRange.IntersectsWith(otherShiftRange))
                                {
                                    errorElements.Add("{0} shifts: {1}".FormatS(group.Key.ToString(), String.Join(" ; ", otherShiftRange.ToString())));
                                }
                            }
                        }
                    }

                    if (errorElements.Any())
                    {
                        string str = "One or more shifts with the same type overlap:  {0}"
                            .FormatS(String.Join(" | ", errorElements));
                        return new FluentValidation.Results.ValidationFailure(PropertySelector.SelectPropertyFrom<WatchDay>(x => x.WatchShifts).Name, str);
                    }

                    return null;
                });
            }
        }

    }
}
