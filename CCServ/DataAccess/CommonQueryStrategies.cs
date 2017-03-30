using AtwoodUtils;
using CCServ.Entities.Watchbill;
using NHibernate.Criterion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Expressions;
using System.Reflection;

namespace CCServ.DataAccess
{
    /// <summary>
    /// Provides some commonly used query strategies.
    /// </summary>
    public static class CommonQueryStrategies
    {
        /// <summary>
        /// Creates a query for any number of Ids, joined by disjunctions.
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="searchValue"></param>
        /// <returns></returns>
        public static ICriterion IdQuery(string propertyName, object searchValue)
        {
            //First we need to get what the client gave us into a list of Guids.
            if (searchValue == null)
                throw new CommandCentralException("You search value must not be null.", HttpStatusCodes.BadRequest);

            if (!(searchValue is string) || !(searchValue is IEnumerable<string>))
                throw new CommandCentralException("When searching for an Id, your search value must be either a string of ids, delineated by white space, semi colones, or commas, or an array of Ids.", HttpStatusCodes.BadRequest);

            List<Guid> ids = new List<Guid>();
            if (searchValue is string)
            {
                foreach (var value in ((string)searchValue).Split(new[] { ',', ';', ' ' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    if (!Guid.TryParse(value.Trim(), out Guid result))
                        throw new CommandCentralException("One of your Id values was not a Guid.  Command Central uses Guids for all Ids.", HttpStatusCodes.BadRequest);

                    ids.Add(result);
                }
            }
            else if (searchValue is IEnumerable<string>)
            {
                foreach (var value in (IEnumerable<string>)searchValue)
                {
                    if (!Guid.TryParse(value.Trim(), out Guid result))
                        throw new CommandCentralException("One of your Id values was not a Guid.  Command Central uses Guids for all Ids.", HttpStatusCodes.BadRequest);

                    ids.Add(result);
                }
            }
            else
            {
                throw new NotImplementedException("Hit default case in IdQuery method.");
            }

            //Now that we have the Guids, let's put them all together as restrictions.


            var disjunction = new Disjunction();

            foreach (var id in ids)
            {
                disjunction.Add(Restrictions.Eq(propertyName, id));
            }

            return disjunction;
        }

        /// <summary>
        /// Given a list of search values, creates a disjunction to look for the values in the given type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="propertyExpression">The property of the parent type to search.</param>
        /// <param name="searchValue"></param>
        /// <returns></returns>
        public static ICriterion ReferenceListValueQuery<T>(Expression<Func<T, object>> propertyExpression, object searchValue)
        {

            //First we need to get what the client gave us into a list of Guids.
            if (searchValue == null)
                throw new CommandCentralException("You search value must not be null.", HttpStatusCodes.BadRequest);

            if (!(searchValue is string) || !(searchValue is IEnumerable<string>))
                throw new CommandCentralException("When searching for a value, your search value must be either a string of values, " +
                    "delineated by white space, semi colones, or commas, or an array of values.", HttpStatusCodes.BadRequest);

            List<string> values = new List<string>();
            if (searchValue is string)
            {
                foreach (var value in ((string)searchValue).Split(new[] { ',', ';', ' ' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    if (String.IsNullOrWhiteSpace(value) || String.IsNullOrWhiteSpace(value.Trim()))
                        throw new CommandCentralException("One of your values was not vallid.", HttpStatusCodes.BadRequest);

                    values.Add(value.Trim());
                }
            }
            else if (searchValue is IEnumerable<string>)
            {
                foreach (var value in (IEnumerable<string>)searchValue)
                {
                    if (String.IsNullOrWhiteSpace(value) || String.IsNullOrWhiteSpace(value.Trim()))
                        throw new CommandCentralException("One of your values was not vallid.", HttpStatusCodes.BadRequest);

                    values.Add(value.Trim());
                }
            }
            else
            {
                throw new NotImplementedException("Hit default case in IdQuery method.");
            }

            var disjunction = new Disjunction();

            foreach (var value in values)
            {
                var selectionExpression = System.Linq.Expressions.Expression.Lambda<Func<T, object>>(System.Linq.Expressions.Expression.Property(propertyExpression, "Id"));

                var method = typeof(QueryOver).GetMethod("Of").MakeGenericMethod(((PropertyInfo)propertyExpression.GetProperty()).PropertyType);

                var subQueryOver = (QueryOver)method.Invoke(null, null);
                var criteria = subQueryOver.DetachedCriteria.Add(Restrictions.On<Entities.ReferenceLists.ReferenceListItemBase>(x => x.Value).IsInsensitiveLike(value, MatchMode.Anywhere)).SetProjection(Projections.Id());

                disjunction.Add(Subqueries.PropertyIn(propertyExpression.GetPropertyName(), criteria));
            }

            return disjunction;
        }

        /// <summary>
        /// Creates a simple boolean query for the given property name.
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="searchValue"></param>
        /// <returns></returns>
        public static ICriterion BooleanQuery(string propertyName, object searchValue)
        {
            bool value;
            try
            {
                value = (bool)searchValue;
            }
            catch (Exception)
            {
                throw new CommandCentralException("An error occurred while parsing your boolean search value.", HttpStatusCodes.BadRequest);
            }

            return Restrictions.Eq(propertyName, value);
        }

        /// <summary>
        /// Creates a query for an array of date time ranges.
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="searchValue"></param>
        /// <returns></returns>
        public static ICriterion DateTimeQuery(string propertyName, object searchValue)
        {
            var values = searchValue as IEnumerable<Dictionary<string, DateTime?>> ?? throw new CommandCentralException("Your date/time criteria must be in an array of dictionaries with " +
                "at least a from/to and a corresponding date.", HttpStatusCodes.BadRequest);

            var disjunction = new Disjunction();

            foreach (var value in values)
            {
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
                    throw new CommandCentralException("You must send at least a 'from' and a 'to' date.", HttpStatusCodes.BadRequest);

                //Do the validation.
                if ((from.HasValue && to.HasValue) && from > to)
                    throw new CommandCentralException("The dates, From:'{0}' and To:'{1}', were invalid.  'From' may not be after 'To'.".FormatS(from, to), HttpStatusCodes.BadRequest);

                if (from == to)
                {
                    disjunction.Add(Restrictions.And(
                            Restrictions.Ge(propertyName, from.Value.Date),
                            Restrictions.Le(propertyName, from.Value.Date.AddHours(24))));
                }
                else if (from == null)
                {
                    disjunction.Add(Restrictions.Le(propertyName, to));
                }
                else if (to == null)
                {
                    disjunction.Add(Restrictions.Ge(propertyName, from));
                }
                else
                {
                    disjunction.Add(Restrictions.And(
                            Restrictions.Ge(propertyName, from),
                            Restrictions.Le(propertyName, to)));
                }
            }

            return disjunction;
        }

        /// <summary>
        /// Creates a string query for a property with a disjunctions for all the search values.
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="searchValue"></param>
        /// <returns></returns>
        public static ICriterion StringQuery(string propertyName, object searchValue)
        {
            //First we need to get what the client gave us into a list of Guids.
            if (searchValue == null)
                throw new CommandCentralException("You search value must not be null.", HttpStatusCodes.BadRequest);

            if (!(searchValue is string) || !(searchValue is IEnumerable<string>))
                throw new CommandCentralException("When searching for a value, your search value must be either a string of values, delineated by white space, semi colones, or commas, or an array of values.", HttpStatusCodes.BadRequest);

            List<string> values = new List<string>();
            if (searchValue is string)
            {
                foreach (var value in ((string)searchValue).Split(new[] { ',', ';', ' ' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    if (String.IsNullOrWhiteSpace(value) || String.IsNullOrWhiteSpace(value.Trim()))
                        throw new CommandCentralException("One of your values was not vallid.", HttpStatusCodes.BadRequest);

                    values.Add(value.Trim());
                }
            }
            else if (searchValue is IEnumerable<string>)
            {
                foreach (var value in (IEnumerable<string>)searchValue)
                {
                    if (String.IsNullOrWhiteSpace(value) || String.IsNullOrWhiteSpace(value.Trim()))
                        throw new CommandCentralException("One of your values was not vallid.", HttpStatusCodes.BadRequest);

                    values.Add(value.Trim());
                }
            }
            else
            {
                throw new NotImplementedException("Hit default case in IdQuery method.");
            }

            var disjunction = new Disjunction();

            foreach (var value in values)
            {
                disjunction.Add(Restrictions.InsensitiveLike(propertyName, value, MatchMode.Anywhere));
            }

            return disjunction;
        }
    }
}
