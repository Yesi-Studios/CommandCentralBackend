using CCServ.ClientAccess;
using System.Linq;
using FluentNHibernate.Mapping;
using System;
using System.Collections.Generic;
using AtwoodUtils;
using FluentValidation;
using NHibernate.Criterion;
using CCServ.Authorization;
using CCServ.Logging;
using FluentValidation.Results;

namespace CCServ.Entities.ReferenceLists
{
    /// <summary>
    /// Describes a single command, such as NIOC GA and all of its departments and divisions.
    /// </summary>
    public class Command
    {
        #region Properties

        /// <summary>
        /// The unique Id of this command.
        /// </summary>
        public virtual Guid Id { get; set; }

        /// <summary>
        /// THe value of this command.
        /// </summary>
        public virtual string Value { get; set; }

        /// <summary>
        /// The command's description.
        /// </summary>
        public virtual string Description { get; set; }

        /// <summary>
        /// The departments of the command
        /// </summary>
        public virtual IList<Department> Departments { get; set; }

        #endregion

        /// <summary>
        /// Maps a command to the database.
        /// </summary>
        public class CommandMapping : ClassMap<Command>
        {
            /// <summary>
            /// Maps a command to the database.
            /// </summary>
            public CommandMapping()
            {
                Id(x => x.Id).GeneratedBy.Assigned();

                Map(x => x.Value).Not.Nullable().Unique();
                Map(x => x.Description);

                HasMany(x => x.Departments).Not.LazyLoad().Cascade.DeleteOrphan();

                Cache.ReadWrite();
            }
        }

        /// <summary>
        /// Validates the Command.
        /// </summary>
        public class CommandValidator : AbstractValidator<Command>
        {
            /// <summary>
            /// Validates the Command.
            /// </summary>
            public CommandValidator()
            {
                RuleFor(x => x.Description).Length(0, 255)
                    .WithMessage("The description of a Command must be no more than 255 characters.");
                RuleFor(x => x.Value).NotEmpty()
                    .WithMessage("The value must not be empty.");
            }
        }

    }
}
