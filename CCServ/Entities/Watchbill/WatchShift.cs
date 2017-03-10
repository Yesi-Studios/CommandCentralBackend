﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentNHibernate.Mapping;
using FluentValidation;
using NHibernate.Type;
using AtwoodUtils;

namespace CCServ.Entities.Watchbill
{
    /// <summary>
    /// A watch shift represents a single watch, who is assigned to it, and for what day it is as well as from one time to what time.  And some other things.
    /// </summary>
    public class WatchShift
    {

        #region Properties

        /// <summary>
        /// The unique Id of this watchshift.
        /// </summary>
        public virtual Guid Id { get; set; }

        /// <summary>
        /// The watch day that owns this watch shift.
        /// </summary>
        public virtual IList<WatchDay> WatchDays { get; set; }

        /// <summary>
        /// A free text field allowing for this shift to be given a title.
        /// </summary>
        public virtual string Title { get; set; }

        /// <summary>
        /// The range, containing the dates that the shift starts and ends.
        /// </summary>
        public virtual TimeRange Range { get; set; }

        /// <summary>
        /// The watch inputs that have been given for this shift.  This is all the persons that have said they can not stand this shift and their given reasons.
        /// </summary>
        public virtual IList<WatchInput> WatchInputs { get; set; }

        /// <summary>
        /// The list of all the assignments for this shift.  Only one assignment should be considered the current assignment while the rest should be only historical.
        /// <para />
        /// An empty collection here indicates this shift has not yet been assigned.
        /// </summary>
        public virtual IList<WatchAssignment> WatchAssignments { get; set; }

        /// <summary>
        /// Indicates the type of this shift:  Is it JOOD, OOD, etc.
        /// </summary>
        public virtual ReferenceLists.Watchbill.WatchShiftType ShiftType { get; set; }

        #endregion

        #region ctors

        /// <summary>
        /// Creates a new watch shift.
        /// </summary>
        public WatchShift()
        {
            WatchDays = new List<WatchDay>();
            WatchInputs = new List<WatchInput>();
            WatchAssignments = new List<WatchAssignment>();

            Range = new TimeRange();
        }

        #endregion

        /// <summary>
        /// Maps this object to the database.
        /// </summary>
        public class WatchShiftMapping : ClassMap<WatchShift>
        {
            /// <summary>
            /// Maps this object to the database.
            /// </summary>
            public WatchShiftMapping()
            {
                Id(x => x.Id).GeneratedBy.Assigned();

                References(x => x.ShiftType).Not.Nullable();

                HasMany(x => x.WatchAssignments);

                HasManyToMany(x => x.WatchInputs);
                HasManyToMany(x => x.WatchDays);

                Map(x => x.Title).Not.Nullable();
                Component(x => x.Range, x =>
                    {
                        x.Map(y => y.Start).Not.Nullable().CustomType<UtcDateTimeType>();
                        x.Map(y => y.End).Not.Nullable().CustomType<UtcDateTimeType>();
                    });
            }
        }

        /// <summary>
        /// Validates the parent object.
        /// </summary>
        public class WatchShiftValidator : AbstractValidator<WatchShift>
        {
            /// <summary>
            /// Validates the parent object.
            /// </summary>
            public WatchShiftValidator()
            {
                RuleFor(x => x.ShiftType).NotEmpty();
                RuleFor(x => x.Title).NotEmpty().Length(1, 50);
                RuleFor(x => x.WatchInputs).SetCollectionValidator(new WatchInput.WatchInputValidator());

                Custom(watchShift =>
                {
                    if (watchShift.Range.Start == default(DateTime) || watchShift.Range.End == default(DateTime))
                        return new FluentValidation.Results.ValidationFailure(PropertySelector.SelectPropertyFrom<WatchShift>(x => x.Range).Name, "The watch shift's range dates must make sense.  Please.");

                    if (watchShift.Range.Start >= watchShift.Range.End)
                        return new FluentValidation.Results.ValidationFailure(PropertySelector.SelectPropertyFrom<WatchShift>(x => x.Range).Name, "The watch shift's range dates must make sense.  Please.");

                    if (watchShift.Range.End <= watchShift.Range.Start)
                        return new FluentValidation.Results.ValidationFailure(PropertySelector.SelectPropertyFrom<WatchShift>(x => x.Range).Name, "The watch shift's range dates must make sense.  Please.");

                    return null;
                });
            }
        }

    }
}
