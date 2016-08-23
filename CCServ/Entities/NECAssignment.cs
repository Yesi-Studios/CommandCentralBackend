using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentNHibernate.Mapping;
using FluentValidation;


namespace CCServ.Entities
{
    /// <summary>
    /// Maps a person to an NEC, with the additional information being if an NEC is a person's primary or secondary.
    /// </summary>
    public class NECAssignment
    {

        /// <summary>
        /// The Id of this assignment.
        /// </summary>
        public virtual Guid Id { get; set; }

        /// <summary>
        /// The NEC referenced by this assignment.
        /// </summary>
        public virtual ReferenceLists.NEC NEC { get; set; }

        /// <summary>
        /// The Person referenced by this assignment.
        /// </summary>
        public virtual Person Person { get; set; }

        /// <summary>
        /// Indicates this NEC is primary, else, it is secondary.
        /// </summary>
        public virtual bool IsPrimary { get; set; }

        /// <summary>
        /// Maps an NEC assignment to the database.
        /// </summary>
        public class NECAssignmentClassMapping : ClassMap<NECAssignment>
        {
            /// <summary>
            /// Maps an NEC assignment to the database.
            /// </summary>
            public NECAssignmentClassMapping()
            {
                Id(x => x.Id).GeneratedBy.Assigned();

                Map(x => x.IsPrimary).Not.Nullable();

                References(x => x.Person).Not.Nullable();
                References(x => x.NEC).Not.Nullable();
            }
        }
    }
}
