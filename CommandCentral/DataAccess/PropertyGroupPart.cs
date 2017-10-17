using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using NHibernate;
using NHibernate.Criterion;
using AtwoodUtils;
using System.Linq.Expressions;

namespace CommandCentral.DataAccess
{
    /// <summary>
    /// Groups a number of properties in order to declare common behavior about them.  
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class PropertyGroupPart<T>
    {

        #region Properties

        /// <summary>
        /// The parent query strategy provider that created this property group.
        /// </summary>
        public QueryStrategyProvider<T> ParentQueryStrategy { get; set; }

        /// <summary>
        /// The members references in this property group.
        /// </summary>
        public HashSet<Expression<Func<T, object>>> Expressions { get; set; } = new HashSet<Expression<Func<T, object>>>();

        /// <summary>
        /// The criteria provider instructs the data layer, given a query term, how to query for this property.
        /// </summary>
        public Func<QueryToken<T>, ICriterion> CriteriaProvider { get; set; }

        /// <summary>
        /// Indicates how this property group should be searched.  As a string, as a datetime, etc.  Mainly used to inform clients how to search this group.
        /// </summary>
        public SearchDataTypes SearchType { get; set; }

        /// <summary>
        /// Indicates what types of searches this property group can be used in.  Practically, this is an enumeration of the types of search algorithms we provide to clients.
        /// </summary>
        public List<QueryTypes> QueryTypesUsedIn { get; set; } = new List<QueryTypes>();

        #endregion

        #region ctors

        /// <summary>
        /// Creates a new property group.
        /// </summary>
        public PropertyGroupPart(QueryStrategyProvider<T> parent, IEnumerable<Expression<Func<T, object>>> expressions)
        {
            if (!expressions.Any())
                throw new ArgumentException("You must have at least one property!");

            foreach (var exp in expressions)
            {
                if (!Expressions.Add(exp))
                {
                    throw new ArgumentException("You may not duplicate values!  Value duplicated: {0}".With(exp.GetPropertyName()));
                }
            }

            ParentQueryStrategy = parent ?? throw new ArgumentException("The parent may not be null.");
        }

        #endregion

        #region Fluent Methods

        /// <summary>
        /// Returns a value indicating if the current property group can be used in the given query type.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public bool CanSearchIn(QueryTypes type)
        {
            return QueryTypesUsedIn.Contains(type);
        }

        /// <summary>
        /// Sets the query strategy to be used, which instructs the data layer, given a query term, how to query for this property.
        /// </summary>
        /// <param name="strat"></param>
        /// <returns></returns>
        public PropertyGroupPart<T> UsingStrategy(Func<QueryToken<T>, ICriterion> strat)
        {
            CriteriaProvider = strat;
            return this;
        }

        /// <summary>
        /// Sets the query types this group can be used in.
        /// </summary>
        /// <param name="usedIn"></param>
        /// <returns></returns>
        public PropertyGroupPart<T> CanBeUsedIn(params QueryTypes[] usedIn)
        {
            QueryTypesUsedIn = usedIn.ToList();
            return this;
        }

        /// <summary>
        /// Sets what type should be used to search this property group.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public PropertyGroupPart<T> AsType(SearchDataTypes type)
        {
            SearchType = type;
            return this;
        }

        #endregion

    }
}
