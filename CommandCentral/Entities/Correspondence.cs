using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentNHibernate.Mapping;

namespace CommandCentral.Entities
{
    /// <summary>
    /// Defines a single correspondence and maps it to the database.
    /// </summary>
    public class Correspondence
    {
        #region Properties

        /// <summary>
        /// Primary key.
        /// </summary>
        public virtual Guid Id { get; set; }

        /// <summary>
        /// Originator of correspondence.
        /// </summary>
        public virtual Person Originator { get; set; }

        /// <summary>
        /// Date and Time correspondence was created.
        /// </summary>
        public virtual DateTime CreatedTime { get; set; }

        /// <summary>
        /// Subject of correspondence.
        /// </summary>
        public virtual string Subject { get; set; }

        /// <summary>
        /// Status of correspondence (e.g., routed to N1, returned to department, etc.)
        /// </summary>
        public virtual ReferenceLists.CorrespondenceStatus Status { get; set; }

        #endregion

        /// <summary>
        /// Maps a correspondence to the database.
        /// </summary>
        public class CorrespondenceMapping : ClassMap<Correspondence> 
        {
            /// <summary>
            /// Maps a correspondence to the database.
            /// </summary>
            public CorrespondenceMapping() 
            {
                Table("correspondences");

                Id(x => x.Id).GeneratedBy.Guid();

                References(x => x.Originator).Not.Nullable();
                Map(x => x.CreatedTime).Not.Nullable();
                Map(x => x.Subject).Not.Nullable().Length(50);
                References(x => x.Status).Not.Nullable();
            }
        }
    }
}
