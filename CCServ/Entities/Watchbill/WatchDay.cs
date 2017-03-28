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
    /// A watchday acts as a collection of watchshifts and also knows its rank among the other watchdays in its parent watchbill.
    /// </summary>
    public class WatchDay
    {

        #region Properties

        /// <summary>
        /// The unique Id of this watch day.
        /// </summary>
        public virtual Guid Id { get; set; }

        /// <summary>
        /// The watchbill that owns this watchday.
        /// </summary>
        public virtual Watchbill Watchbill { get; set; }

        /// <summary>
        /// The date of this watch day.
        /// <para />
        /// No two watch days should share the same date, but the parent Watchbill is responsible for this enforcement.
        /// </summary>
        public virtual DateTime Date { get; set; }

        /// <summary>
        /// The collection of watch shifts contained in this watch day.  These represent the actual watches... eg: A shift from 0800-1200.
        /// </summary>
        public virtual IList<WatchShift> WatchShifts { get; set; }

        /// <summary>
        /// A free text remarks field so that the watchbill can annotate this day (if it's a holiday, for example).
        /// </summary>
        public virtual string Remarks { get; set; }

        #endregion

        #region ctors

        /// <summary>
        /// Creates a new watch day and initalizations the collections to empty.
        /// </summary>
        public WatchDay()
        {
            WatchShifts = new List<WatchShift>();
        }

        #endregion

        /// <summary>
        /// Maps this object to the database.
        /// </summary>
        public class WatchDayMapping : ClassMap<WatchDay>
        {
            /// <summary>
            /// Maps this object to the database.
            /// </summary>
            public WatchDayMapping()
            {
                Id(x => x.Id).GeneratedBy.Assigned();

                References(x => x.Watchbill).Not.Nullable();

                HasManyToMany(x => x.WatchShifts).Cascade.All();

                Map(x => x.Date).Not.Nullable().CustomType<UtcDateTimeType>();
                Map(x => x.Remarks).Length(1000);
            }
        }

        /// <summary>
        /// Validates the parent object.
        /// </summary>
        public class WatchDayValidator : AbstractValidator<WatchDay>
        {
            /// <summary>
            /// Validates the parent object.
            /// </summary>
            public WatchDayValidator()
            {
                RuleFor(x => x.Watchbill).NotEmpty();
                RuleFor(x => x.Date).NotEmpty();
                RuleFor(x => x.Remarks).Length(0, 1000);

                RuleFor(x => x.WatchShifts).SetCollectionValidator(new WatchShift.WatchShiftValidator());
                Custom(watchDay =>
                    {
                        var shiftsByType = watchDay.WatchShifts.GroupBy(x => x.ShiftType);
                        var conflictsByType = shiftsByType.Select(x =>
                            {
                                var ranges = x.ToList().Select(y => y.Range);

                                return new
                                {
                                    ShiftType = x.Key,
                                    Conflicts = Utilities.FindTimeRangeIntersections(ranges).Distinct()
                                };
                            })
                            .ToList();

                        List<string> errorElements = new List<string>();

                        foreach (var group in conflictsByType)
                        {
                            if (group.Conflicts.Any())
                            {
                                errorElements.Add("{0} shifts: {1}".FormatS(group.ShiftType.Value, String.Join(" ; ", group.Conflicts.Select(x => x.ToString()))));
                            }
                        }

                        if (errorElements.Any())
                        {
                            string str = "One or more shifts with the same type overlap during the watch day ({0}).  {1}"
                                .FormatS(watchDay.Date.ToString("dd MMM"), String.Join(" | ", errorElements));
                            return new FluentValidation.Results.ValidationFailure(PropertySelector.SelectPropertyFrom<WatchDay>(x => x.WatchShifts).Name, str);
                        }

                        return null;
                    });
            }
        }

    }
}