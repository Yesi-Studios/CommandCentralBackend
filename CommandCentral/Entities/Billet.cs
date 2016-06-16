﻿using System;
using System.Collections.Generic;
using CommandCentral.ClientAccess;
using CommandCentral.Entities.ReferenceLists;
using FluentNHibernate.Mapping;
using AtwoodUtils;

namespace CommandCentral.Entities
{
    /// <summary>
    /// Describes a single billet along with its data access methods and other members.
    /// </summary>
    public class Billet
    {

        #region Properties

        /// <summary>
        /// The unique Id assigned to this Billet
        /// </summary>
        public virtual Guid Id { get; set; }

        /// <summary>
        /// The title of this billet.
        /// </summary>
        public virtual string Title { get; set; }

        /// <summary>
        /// The Id Number of this Billet.  This is also called the Billet Id Number or BIN.
        /// </summary>
        public virtual string IdNumber { get; set; }

        /// <summary>
        /// The suffix code of this Billet.  This is also called the Billet Suffix Code or BSC.
        /// </summary>
        public virtual string SuffixCode { get; set; }

        /// <summary>
        /// A free form text field intended to store notes/remarks about this billet.
        /// </summary>
        public virtual string Remarks { get; set; }

        /// <summary>
        /// The designation assigned to a Billet.  For an enlisted Billet, this is the Rate the Billet is intended for.  For officers, this is their designation.
        /// </summary>
        public virtual string Designation { get; set; }

        /// <summary>
        /// The funding line that pays for this particular billet.
        /// </summary>
        public virtual string Funding { get; set; }

        /// <summary>
        /// The NEC assigned to this billet.
        /// </summary>
        public virtual NEC NEC { get; set; }

        /// <summary>
        /// The UIC assigned to this billet.
        /// </summary>
        public virtual UIC UIC { get; set; }

        #endregion

        #region Overrides

        /// <summary>
        /// Returns the title.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return this.Title;
        }

        /// <summary>
        /// Compares an object to this billet. Guess what it returns if they're the same?  True.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (!(obj is Billet))
                return false;

            var other = (Billet)obj;

            return this.Id == other.Id && string.Equals(this.Title, other.Title) && string.Equals(this.IdNumber, other.IdNumber) && string.Equals(this.SuffixCode, other.SuffixCode)
                && string.Equals(this.Remarks, other.Remarks) && string.Equals(this.Designation, other.Designation) && string.Equals(this.Funding, other.Funding)
                && Object.Equals(this.NEC, other.NEC) && Object.Equals(this.UIC, other.UIC);
        }

        /// <summary>
        /// Returns yo 'ole hashcode.
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;

                hash = hash * 23 + Utilities.GetSafeHashCode(Id);
                hash = hash * 23 + Utilities.GetSafeHashCode(Title);
                hash = hash * 23 + Utilities.GetSafeHashCode(IdNumber);
                hash = hash * 23 + Utilities.GetSafeHashCode(SuffixCode);
                hash = hash * 23 + Utilities.GetSafeHashCode(Remarks);
                hash = hash * 23 + Utilities.GetSafeHashCode(Designation);
                hash = hash * 23 + Utilities.GetSafeHashCode(Funding);
                hash = hash * 23 + Utilities.GetSafeHashCode(NEC);
                hash = hash * 23 + Utilities.GetSafeHashCode(UIC);

                return hash;
            }
        }

        #endregion

        /// <summary>
        /// Maps a billet to the database.
        /// </summary>
        public class BilletMapping : ClassMap<Billet>
        {
            /// <summary>
            /// Maps a billet to the database.
            /// </summary>
            public BilletMapping()
            {
                Id(x => x.Id);

                Map(x => x.Title).Length(40).Not.Nullable();
                Map(x => x.IdNumber).Length(10).Not.Nullable().Unique();
                Map(x => x.SuffixCode).Length(10).Not.Nullable().Unique();
                Map(x => x.Remarks).Length(100).Nullable();
                Map(x => x.Designation).Length(10).Not.Nullable().Unique();
                Map(x => x.Funding).Length(25).Not.Nullable();
                References(x => x.NEC).Not.Nullable();
                References(x => x.UIC).Not.Nullable();
            }
        }

    }
}
