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
    /// A pipeline is essentially just a grouping of certain requirements, allowing users to track a group of trainings and their total completion.
    /// </summary>
    public class Pipeline
    {

        #region Properties

        /// <summary>
        /// The unique Id of this pipeline.
        /// </summary>
        public virtual Guid Id { get; set; }

        /// <summary>
        /// The title of this pipeline.
        /// </summary>
        public virtual string Title { get; set; }

        /// <summary>
        /// The description of this pipeline.
        /// </summary>
        public virtual string Description { get; set; }

        /// <summary>
        /// The person who created this pipeline.
        /// </summary>
        public virtual Person Creator { get; set; }

        /// <summary>
        /// The time that this pipeline was created.
        /// </summary>
        public virtual DateTime DateCreated { get; set; }

        /// <summary>
        /// The list of requirements that are encapsulated by this pipeline.
        /// </summary>
        public virtual IList<Requirement> Requirements { get; set; }

        #endregion

        /// <summary>
        /// Maps a pipeline to the database.
        /// </summary>
        public class PipelineMapping : ClassMap<Pipeline>
        {
            /// <summary>
            /// Maps a pipeline to the databaes.
            /// </summary>
            public PipelineMapping()
            {
                Id(x => x.Id).GeneratedBy.Guid();

                References(x => x.Creator).Not.Nullable();

                HasMany(x => x.Requirements);

                Map(x => x.DateCreated).Not.Nullable();
                Map(x => x.Title).Not.Nullable();
                Map(x => x.Description).Length(500).Not.Nullable();
            }
        }

        /// <summary>
        /// Validates a pipeline.
        /// </summary>
        public class PipelineValidator : AbstractValidator<Pipeline>
        {
            /// <summary>
            /// Validates a pipeline.
            /// </summary>
            public PipelineValidator()
            {
                RuleFor(x => x.Id).NotEmpty();

                RuleFor(x => x.Creator).NotEmpty();

                RuleFor(x => x.Requirements).SetCollectionValidator(new Requirement.RequirementValidator());

                RuleFor(x => x.DateCreated).NotEmpty();
                RuleFor(x => x.Title).NotEmpty();
                RuleFor(x => x.Description).Length(1, 500).NotEmpty();
            }
        }

    }
}
