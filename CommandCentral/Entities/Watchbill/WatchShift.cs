using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentNHibernate.Mapping;
using FluentValidation;
using NHibernate.Type;
using AtwoodUtils;
using CommandCentral.Entities.ReferenceLists;

namespace CommandCentral.Entities.Watchbill
{
    /// <summary>
    /// A watch shift represents a single watch, who is assigned to it, and for what day it is as well as from one time to what time.  And some other things.
    /// </summary>
    public class WatchShift : ICommentable
    {

        #region Properties

        /// <summary>
        /// The unique Id of this watch shift.
        /// </summary>
        public virtual Guid Id { get; set; }

        /// <summary>
        /// The comments for this shift.
        /// </summary>
        public virtual IList<Comment> Comments { get; set; } = new List<Comment>();

        /// <summary>
        /// The watchbill of this shift.
        /// </summary>
        public virtual Watchbill Watchbill { get; set; }

        /// <summary>
        /// A free text field allowing for this shift to be given a title.
        /// </summary>
        public virtual string Title { get; set; }

        /// <summary>
        /// The range, containing the dates that the shift starts and ends.
        /// </summary>
        public virtual TimeRange Range { get; set; } = new TimeRange();

        /// <summary>
        /// The list of all the assignments for this shift.  Only one assignment should be considered the current assignment while the rest should be only historical.
        /// <para />
        /// An empty collection here indicates this shift has not yet been assigned.
        /// </summary>
        public virtual WatchAssignment WatchAssignment { get; set; }

        /// <summary>
        /// The division that must fill this watch shift.
        /// </summary>
        public virtual Division DivisionAssignedTo { get; set; }

        /// <summary>
        /// Indicates the type of this shift:  Is it JOOD, OOD, etc.
        /// </summary>
        public virtual ReferenceLists.Watchbill.WatchShiftType ShiftType { get; set; }

        /// <summary>
        /// The point value for this shift.
        /// </summary>
        public virtual double Points { get; set; }

        #endregion

        #region ctors

        /// <summary>
        /// Creates a new watch shift.
        /// </summary>
        public WatchShift()
        {
        }

        #endregion

        #region Overrides

        /// <summary>
        /// Returns {Title} {Range}
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "{0} ({1})".With(Title, Range);
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
                References(x => x.WatchAssignment).Cascade.All();
                References(x => x.Watchbill);
                References(x => x.DivisionAssignedTo);

                Map(x => x.Title).Not.Nullable();
                Map(x => x.Points).Not.Nullable();
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
                RuleFor(x => x.Points).GreaterThanOrEqualTo(0);
                RuleFor(x => x.ShiftType).NotEmpty();
                RuleFor(x => x.Title).NotEmpty().Length(1, 50);

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
