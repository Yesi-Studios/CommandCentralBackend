using System;

using FluentNHibernate.Mapping;

namespace CommandCentral.Entities.ReferenceLists
{
    /// <summary>
    /// Describes a single Division.
    /// </summary>
    public class Division
    {

        #region Properties

        /// <summary>
        /// The Division's unique ID
        /// </summary>
        public virtual Guid Id { get; set; }

        /// <summary>
        /// The value of this Division.  Eg. N75
        /// </summary>
        public virtual string Value { get; set; }

        /// <summary>
        /// A short description of this Division.
        /// </summary>
        public virtual string Description { get; set; }

        #endregion


        /// <summary>
        /// Maps a division to the database.
        /// </summary>
        public class DivisionMapping : ClassMap<Division>
        {
            /// <summary>
            /// Maps a division to the database.
            /// </summary>
            public DivisionMapping()
            {
                Table("divisions");

                Id(x => x.Id).GeneratedBy.Guid();

                Map(x => x.Value).Not.Nullable().Unique().Length(20);
                Map(x => x.Description).Nullable().Length(50);

                Cache.ReadWrite();
            }
        }
    }
}
