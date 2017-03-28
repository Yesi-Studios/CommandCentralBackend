using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentNHibernate.Mapping;
using FluentValidation;
using NHibernate.Type;
using CCServ.DataAccess;
using AtwoodUtils;
using NHibernate.Criterion;

namespace CCServ.Entities.Watchbill
{
    /// <summary>
    /// A watch assignment which ties a person to a watch shift and indicates if the assignment has been completed, or what status it is in.
    /// </summary>
    public class WatchAssignment
    {

        #region Properties

        /// <summary>
        /// The unique Id of this watch assignment.
        /// </summary>
        public virtual Guid Id { get; set; }

        /// <summary>
        /// The watch shift that this assignment assigns the person to.
        /// </summary>
        public virtual WatchShift WatchShift { get; set; }

        /// <summary>
        /// The person that this watch assignment assigns to a watch shift.
        /// </summary>
        public virtual Person PersonAssigned { get; set; }

        /// <summary>
        /// The person who assigned the assigned person to the watch shift.  Basically, the person who created this assignment.
        /// </summary>
        public virtual Person AssignedBy { get; set; }

        /// <summary>
        /// The person who acknowledged this watch assignment.  Either the person assigned or someone who did it on their behalf.
        /// </summary>
        public virtual Person AcknowledgedBy { get; set; }
        
        /// <summary>
        /// The datetime at which this assignment was created.
        /// </summary>
        public virtual DateTime DateAssigned { get; set; }

        /// <summary>
        /// The datetime at which a person acknowledged this watch assignment.
        /// </summary>
        public virtual DateTime? DateAcknowledged { get; set; }

        /// <summary>
        /// Indicates if this watch assignment has been acknowledged.
        /// </summary>
        public virtual bool IsAcknowledged { get; set; }

        /// <summary>
        /// The current state of this watch assignment.
        /// </summary>
        public virtual ReferenceLists.Watchbill.WatchAssignmentState CurrentState { get; set; }

        #endregion

        /// <summary>
        /// Maps this object to the database.
        /// </summary>
        public class WatchAssignmentMapping : ClassMap<WatchAssignment>
        {
            /// <summary>
            /// Maps this object to the database.
            /// </summary>
            public WatchAssignmentMapping()
            {
                Id(x => x.Id).GeneratedBy.Assigned();

                References(x => x.WatchShift).Not.Nullable();
                References(x => x.PersonAssigned).Not.Nullable();
                References(x => x.AssignedBy).Not.Nullable();
                References(x => x.CurrentState).Not.Nullable();
                References(x => x.AcknowledgedBy);

                Map(x => x.DateAssigned).Not.Nullable().CustomType<UtcDateTimeType>();
                Map(x => x.DateAcknowledged).CustomType<UtcDateTimeType>();
                Map(x => x.IsAcknowledged).Default(false.ToString());
            }
        }

        /// <summary>
        /// Validates the parent object.
        /// </summary>
        public class WatchAssignmentValidator : AbstractValidator<WatchAssignment>
        {
            /// <summary>
            /// Validates the parent object.
            /// </summary>
            public WatchAssignmentValidator()
            {
                RuleFor(x => x.WatchShift).NotEmpty();
                RuleFor(x => x.PersonAssigned).NotEmpty();
                RuleFor(x => x.AssignedBy).NotEmpty();
                RuleFor(x => x.CurrentState).NotEmpty();
                RuleFor(x => x.DateAssigned).NotEmpty();

                When(x => x.AcknowledgedBy != null || x.DateAcknowledged.HasValue || x.DateAcknowledged.Value != default(DateTime) || x.IsAcknowledged, () =>
                {
                    RuleFor(x => x.DateAcknowledged).NotEmpty();
                    RuleFor(x => x.DateAcknowledged).Must(x => x.Value != default(DateTime));
                    RuleFor(x => x.IsAcknowledged).Must(x => x == true);
                    RuleFor(x => x.AcknowledgedBy).NotEmpty();
                });
            }
        }

        /// <summary>
        /// Provides searching strategies for the watch assignment object.
        /// </summary>
        public class WatchAssignmentQueryProvider : QueryStrategy<WatchAssignment>
        {
            /// <summary>
            /// Provides searching strategies for the watch assignment object.
            /// </summary>
            public  WatchAssignmentQueryProvider()
            {
                ForProperties(PropertySelector.SelectPropertiesFrom<WatchAssignment>(
                    x => x.Id))
                .AsType(SearchDataTypes.String)
                .CanBeUsedIn(QueryTypes.Advanced)
                .UsingStrategy(token =>
                    {
                        return Restrictions.Eq(token.SearchParameter.Key.Name, token.SearchParameter.Value.ToString());
                    });

                ForProperties(PropertySelector.SelectPropertiesFrom<WatchAssignment>(
                    x => x.CurrentState))
                .AsType(SearchDataTypes.String)
                .CanBeUsedIn(QueryTypes.Advanced)
                .UsingStrategy(token =>
                {
                    return Subqueries.WhereProperty<WatchAssignment>(x => x.WatchShift.Id).In(QueryOver.Of<ReferenceLists.Watchbill.WatchAssignmentState>().WhereRestrictionOn(x => x.Value).IsInsensitiveLike(token.SearchParameter.Value.ToString(), MatchMode.Anywhere).Select(x => x.Id));
                });

                ForProperties(PropertySelector.SelectPropertiesFrom<WatchAssignment>(
                    x => x.PersonAssigned,
                    x => x.AssignedBy,
                    x => x.AcknowledgedBy))
                .AsType(SearchDataTypes.String)
                .CanBeUsedIn(QueryTypes.Advanced)
                .UsingStrategy(token =>
                {
                    throw new NotImplementedException();
                    //TODO
                });

                ForProperties(PropertySelector.SelectPropertiesFrom<WatchAssignment>(
                    x => x.IsAcknowledged))
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

                ForProperties(PropertySelector.SelectPropertiesFrom<WatchAssignment>(
                    x => x.DateAssigned,
                    x => x.DateAcknowledged))
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

            }

        }

    }
}
