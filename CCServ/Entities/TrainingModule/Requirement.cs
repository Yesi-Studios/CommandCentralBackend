using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentNHibernate.Mapping;
using FluentValidation;

namespace CCServ.Entities.TrainingModule
{
    /// <summary>
    /// Describes a single training requirement.  This can then be assigned to a person through an Assignment.
    /// </summary>
    public class Requirement
    {

        #region Properties

        /// <summary>
        /// The unique Id of this requirement.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// The title of this requirement.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// The description of this requirement.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// The person who created this training Requirement.
        /// </summary>
        public Person Creator { get; set; }

        /// <summary>
        /// The date/time this training requirement was created.
        /// </summary>
        public DateTime DateCreated { get; set; }

        #endregion

        /// <summary>
        /// Maps a requirement to the database.
        /// </summary>
        public class RequirementMapping : ClassMap<Requirement>
        {
            /// <summary>
            /// Maps a requirement to the database.
            /// </summary>
            public RequirementMapping()
            {
                Id(x => x.Id).GeneratedBy.Guid();

                References(x => x.Creator).LazyLoad(Laziness.False);

                Map(x => x.Title).Not.Nullable().Unique();
                Map(x => x.Description).Length(500).Not.Nullable();
                Map(x => x.DateCreated).Not.Nullable();
            }
        }

        /// <summary>
        /// Validates the properties of a requirement.
        /// </summary>
        public class RequirementValidator : AbstractValidator<Requirement>
        {
            /// <summary>
            /// Validates the properties of a requirement.
            /// </summary>
            public RequirementValidator()
            {
                RuleFor(x => x.Title).NotEmpty().Length(3, 50);
                RuleFor(x => x.Description).NotEmpty().Length(3, 500);
                RuleFor(x => x.DateCreated).NotEmpty();
                RuleFor(x => x.Creator).NotEmpty();
                RuleFor(x => x.Id).NotEmpty();
            }
        }

    }
}
