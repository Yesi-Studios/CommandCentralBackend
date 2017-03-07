using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentNHibernate.Mapping;
using FluentValidation;
using CCServ.Entities.ReferenceLists.Watchbill;
using CCServ.ClientAccess;

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
        /// The collection of all the watch inputs given for shifts within this watchbill.
        /// </summary>
        public virtual IList<WatchInput> WatchInputs { get; set; }

        /// <summary>
        /// The command at which this watchbill was created.
        /// </summary>
        public virtual ReferenceLists.Command Command { get; set; }

        /// <summary>
        /// This is how the watchbill knows the pool of people to use when assigning inputs, and assigning watches.  
        /// <para />
        /// The elligibilty group also determines the type of watchbill.
        /// </summary>
        public virtual ReferenceLists.Watchbill.WatchElligibilityGroup ElligibilityGroup { get; set; }

        #endregion

        #region ctors

        /// <summary>
        /// Creates a new watchbill, setting all collection to empty.
        /// </summary>
        public Watchbill()
        {
            WatchDays = new List<WatchDay>();
            InputRequirements = new List<WatchInputRequirement>();
            WatchInputs = new List<WatchInput>();
        }

        #endregion

        #region Methods

        public virtual void SetState(ReferenceLists.Watchbill.WatchbillStatus desiredState, DateTime setTime, Person person)
        {
            if (this.CurrentState.Id == desiredState.Id)
            {
                throw new Exception("Can't set the state to its same value.");
            }

            if (desiredState == WatchbillStatuses.Initial)
            {
                for (int x = this.InputRequirements.Count - 1; x >= 0; x--)
                {
                    this.InputRequirements.RemoveAt(x);
                }
                this.WatchInputs.Clear();

                foreach (var watchDay in this.WatchDays)
                {
                    foreach (var shift in watchDay.WatchShifts)
                    {
                        shift.WatchAssignments.Clear();
                        shift.WatchInputs.Clear();
                    }
                }
            }
            else if (desiredState == WatchbillStatuses.OpenForInputs)
            {
                foreach (var ellPerson in this.ElligibilityGroup.ElligiblePersons)
                {
                    this.InputRequirements.Add(new WatchInputRequirement
                    {
                        Id = Guid.NewGuid(),
                        Person = ellPerson
                    });

                    //send emails to each of the people for whom we just added a watch input requirement.
                }
            }
            else if (desiredState == WatchbillStatuses.ClosedForInputs)
            {
                throw new NotImplementedException("Not implemented default case in the set watchbill state method.");
            }
            else if (desiredState == WatchbillStatuses.UnderReview)
            {
                throw new NotImplementedException("Not implemented default case in the set watchbill state method.");
            }
            else if (desiredState == WatchbillStatuses.Published)
            {
                throw new NotImplementedException("Not implemented default case in the set watchbill state method.");
            }
            else
            {
                throw new NotImplementedException("Not implemented default case in the set watchbill state method.");
            }

            this.CurrentState = desiredState;
            this.LastStateChange = setTime;
            this.LastStateChangedBy = person;
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
                References(x => x.ElligibilityGroup);

                HasMany(x => x.WatchDays).Cascade.All();
                HasMany(x => x.InputRequirements).Cascade.AllDeleteOrphan();
                HasMany(x => x.WatchInputs);

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

                RuleFor(x => x.WatchDays).SetCollectionValidator(new WatchDay.WatchDayValidator());
                RuleFor(x => x.InputRequirements).SetCollectionValidator(new WatchInputRequirement.WatchInputRequirementValidator());
                RuleFor(x => x.WatchInputs).SetCollectionValidator(new WatchInput.WatchInputValidator());

                When(x => x.CurrentState != ReferenceLists.Watchbill.WatchbillStatuses.Initial, () =>
                {
                    RuleFor(x => x.ElligibilityGroup).NotEmpty().WithMessage("You may not change a watchbill's state from initial without assigning an elligibility group.");
                });
            }
        }

    }
}
