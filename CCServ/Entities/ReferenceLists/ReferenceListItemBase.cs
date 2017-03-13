using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using CCServ.ClientAccess;
using AtwoodUtils;
using FluentValidation.Results;
using NHibernate.Criterion;
using CCServ.Authorization;
using NHibernate.Transform;
using System.Reflection;

namespace CCServ.Entities.ReferenceLists
{
    /// <summary>
    /// Provides abstracted access to a reference list such as Ranks or Rates.
    /// </summary>
    public abstract class ReferenceListItemBase
    {
        #region Properties

        /// <summary>
        /// The Id of this reference item.
        /// </summary>
        public virtual Guid Id { get; set; }

        /// <summary>
        /// The value of this item.
        /// </summary>
        public virtual string Value { get; set; }

        /// <summary>
        /// A description of this item.
        /// </summary>
        public virtual string Description { get; set; }

        #endregion

        #region Overrides

        /// <summary>
        /// Returns the Value.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return Value;
        }

        /// <summary>
        /// Compares this reference list to another reference list.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
                return false;

            return this == (ReferenceListItemBase)obj;
        }

        /// <summary>
        /// Compares the values of two reference lists.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static bool operator ==(ReferenceListItemBase x, ReferenceListItemBase y)
        {
            if (object.ReferenceEquals(null, x))
                return object.ReferenceEquals(null, y);

            return x.Id == y.Id && x.Value == y.Value && x.Description == y.Description;
        }

        /// <summary>
        /// Compares the values of two reference lists.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static bool operator !=(ReferenceListItemBase x, ReferenceListItemBase y)
        {
            return !(x == y);
        }

        /// <summary>
        /// Gets the hashcode of this object. Seeds are 17 and 23
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;

                hash = hash * 23 + Id.GetHashCode();
                hash = hash * 23 + (string.IsNullOrEmpty(Value) ? "".GetHashCode() : Value.GetHashCode());
                hash = hash * 23 + (string.IsNullOrEmpty(Description) ? "".GetHashCode() : Description.GetHashCode());

                return hash;
            }
        }

        #endregion

        #region Data Access Methods

        public abstract List<ReferenceListItemBase> Load(Guid id, MessageToken token);

        #endregion

    }
}
