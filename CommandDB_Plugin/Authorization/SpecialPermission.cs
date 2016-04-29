using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentNHibernate.Mapping;
using CommandCentral.DataAccess;

namespace CommandCentral
{
    /// <summary>
    /// Describes a single permission.  Single permissions are designed to allow/disallow access to parts of the website.
    /// </summary>
    public class SpecialPermission : CachedModel<SpecialPermission>
    {
        #region Properties

        /// <summary>
        /// A unique ID.  No shit.
        /// </summary>
        public string ID { get; set; }

        /// <summary>
        /// The name of this Special Permission
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// A brief description of this special permission.
        /// </summary>
        public string Description { get; set; }

        #endregion

        /// <summary>
        /// Maps a special permission to the database.
        /// </summary>
        public class SpecialPermissionMapping : ClassMap<SpecialPermission>
        {
            /// <summary>
            /// Maps a special permission to the database.
            /// </summary>
            public SpecialPermissionMapping()
            {
                Table("specialpermissions");

                Id(x => x.ID).GeneratedBy.Guid();

                Map(x => x.Name).Not.Nullable().Unique().Length(20);
                Map(x => x.Description).Nullable().Length(50);
            }
        }
    }

    /*
    /// <summary>
    /// Describes custom permissions that are intended to supplement what the unified service gives us.
    /// </summary>
    public enum SpecialPermissionTypes
    {
        Add_New_User,
        Search_Users,
        Edit_Users,
        Division_Leadership,
        Department_Leadership,
        Command_Leadership,
        Can_Muster_Self,
        Can_Muster_Division,
        Can_Muster_Department,
        Can_Muster_Command,
        Developer,
        Manpower_Admin,
        Conduct_Muster,
        Manage_News,
        Super_User
    }*/


}
