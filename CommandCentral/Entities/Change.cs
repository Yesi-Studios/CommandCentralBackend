using System;
using FluentNHibernate.Mapping;

namespace CommandCentral.Entities
{

    /// <summary>
    /// Describes a single change
    /// </summary>
    public class Change
    {

        #region Properties

        /// <summary>
        /// The Id of this change.
        /// </summary>
        public virtual Guid Id { get; set; }

        /// <summary>
        /// The client who initiated this change.
        /// </summary>
        public virtual Person Editor { get; set; }

        /// <summary>
        /// The name of the object/entity that was changed.
        /// </summary>
        public virtual string ObjectName { get; set; }

        /// <summary>
        /// The Id of the object that was changed.
        /// </summary>
        public virtual string ObjectId { get; set; }

        /// <summary>
        /// The property that was edited.
        /// </summary>
        public virtual string PropertyName { get; set; }

        /// <summary>
        /// The old value of the property.
        /// </summary>
        public virtual string OldValue { get; set; }

        /// <summary>
        /// The new value of the property.
        /// </summary>
        public virtual string NewValue { get; set; }

        /// <summary>
        /// The time this change was made.
        /// </summary>
        public virtual DateTime Time { get; set; }

        /// <summary>
        /// A free text field describing this change.
        /// </summary>
        public virtual string Remarks { get; set; }

        #endregion

        /// <summary>
        /// Maps a change to the database.
        /// </summary>
        public class ChangeMapping : ClassMap<Change>
        {
            /// <summary>
            /// Maps a change to the database.
            /// </summary>
            public ChangeMapping()
            {
                Table("changes");

                Id(x => x.Id).GeneratedBy.Guid();

                References(x => x.Editor);

                Map(x => x.ObjectName).Not.Nullable().Length(20);
                Map(x => x.ObjectId).Not.Nullable().Length(45);
                Map(x => x.PropertyName).Not.Nullable().Length(20);
                Map(x => x.OldValue).Not.Nullable().Length(100);
                Map(x => x.NewValue).Not.Nullable().Length(100);
                Map(x => x.Time).Not.Nullable();
                Map(x => x.Remarks).Nullable().Length(150);

            }
        }

    }
}
