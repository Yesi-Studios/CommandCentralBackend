using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentNHibernate.Mapping;

namespace CommandCentral.Authorization
{
    /// <summary>
    /// Describes a single permission group.
    /// </summary>
    public class PermissionGroup
    {
        #region Properties

        /// <summary>
        /// The ID of this permission group.  This should not change after original creation.
        /// </summary>
        public virtual string ID { get; set; }

        /// <summary>
        /// The name of this permission group.
        /// </summary>
        public virtual string Name { get; set; }

        /// <summary>
        /// The list of sub-permissions that describe what rights this permission group grants to what model.
        /// </summary>
        public virtual List<ModelPermission> ModelPermissions { get; set; }

        /// <summary>
        /// Additional list of permissions.  Intended to describe access to other parts of the application as defined by the consumer.
        /// </summary>
        public virtual List<ReferenceLists.SpecialPermission> SpecialPermissions { get; set; }

        /// <summary>
        /// A list of those permissions groups that are subordinate to this permission group.  This is used to determine which groups can promote people into which groups.
        /// </summary>
        public virtual List<PermissionGroup> SubordinatePermissionGroups { get; set; }

        #endregion

        /// <summary>
        /// Maps a permission group to the database
        /// </summary>
        public class PermissionGroupMapping : ClassMap<PermissionGroup>
        {
            /// <summary>
            /// Maps a permission group to the database
            /// </summary>
            public PermissionGroupMapping()
            {
                Table("permission_groups");

                Id(x => x.ID).GeneratedBy.Guid();

                Map(x => x.Name).Not.Nullable().Unique().Length(20);

                HasManyToMany(x => x.ModelPermissions);
                HasManyToMany(x => x.SpecialPermissions);
                HasManyToMany(x => x.SubordinatePermissionGroups);

            }
        }

    }
}
