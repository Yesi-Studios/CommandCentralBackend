using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentNHibernate.Mapping;
using CommandCentral.DataAccess;
using CommandCentral.ClientAccess;

namespace CommandCentral.Entities
{
    /// <summary>
    /// Describes a single command, such as NIOC GA and all of its departments and divisions.
    /// </summary>
    public class Command : CachedModel<Command>, IExposable
    {
        #region Properties

        /// <summary>
        /// The ID of the command.
        /// </summary>
        public string ID { get; set; }
        /// <summary>
        /// The command's name.
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// A short description of this command.
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// The departments of the command
        /// </summary>
        public List<Department> Departments { get; set; }

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
                Table("commands");

                Id(x => x.ID).GeneratedBy.Guid();

                Map(x => x.Name).Not.Nullable().Unique().Length(20);
                Map(x => x.Description).Nullable().Length(50);

                HasMany(x => x.Departments);
            }
        }

        /// <summary>
        /// The list of endpoints this class exposes.
        /// </summary>
        Dictionary<string, EndpointDescription> IExposable.EndpointDescriptions
        {
            get { throw new NotImplementedException(); }
        }
    }
}
