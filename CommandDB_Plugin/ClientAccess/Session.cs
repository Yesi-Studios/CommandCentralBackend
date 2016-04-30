using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Collections.Concurrent;
using System.Reflection;
using MySql.Data.MySqlClient;
using MySql.Data.Common;
using UnifiedServiceFramework.Framework;
using AtwoodUtils;
using FluentNHibernate.Mapping;
using CommandCentral.DataAccess;

namespace CommandCentral.ClientAccess
{
    /// <summary>
    /// Describes a single session and provides members for interacting with that session.
    /// </summary>
    public class Session
    {
        #region Properties

        /// <summary>
        /// The ID of the session.  This ID should also be used as the authentication token by the client.
        /// </summary>
        public virtual string ID { get; set; }

        /// <summary>
        /// The time this session was created which is the time the client logged in.
        /// </summary>
        public virtual DateTime LoginTime { get; set; }

        /// <summary>
        /// The person to whom this session belongs.
        /// </summary>
        public virtual Entities.Person Person { get; set; }

        /// <summary>
        /// The time at which the client logged out, thus invalidating this session.
        /// </summary>
        public virtual DateTime LogoutTime { get; set; }

        /// <summary>
        /// Indicates where or not the session is valid.
        /// </summary>
        public virtual bool IsActive { get; set; }

        /// <summary>
        /// The permissions of the user to whom this session belongs.
        /// </summary>
        public virtual List<CommandCentral.Authorization.PermissionGroup> Permissions { get; set; }

        /// <summary>
        /// The last time this session was used, not counting this current time.
        /// </summary>
        public virtual DateTime LastUsedTime { get; set; }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Determines if this session has expired given a max age of inactivity.
        /// </summary>
        /// <param name="maxSessionAge"></param>
        /// <returns></returns>
        public bool IsSessionExpired(TimeSpan maxSessionAge)
        {
            if (DateTime.Now.Subtract(this.LastUsedTime) > maxSessionAge)
                return true;

            return false;
        }

        #endregion

        /// <summary>
        /// Maps a session to the database.
        /// </summary>
        public class SessionMapping : ClassMap<Session>
        {
            /// <summary>
            /// Maps a session to the database.
            /// </summary>
            public SessionMapping()
            {
                Table("sessions");

                Id(x => x.ID).GeneratedBy.Guid();

                Map(x => x.LoginTime).Not.Nullable();
                Map(x => x.LogoutTime).Nullable();
                Map(x => x.IsActive).Not.Nullable();
                Map(x => x.LastUsedTime);
                References(x => x.Person);
                HasManyToMany(x => x.Permissions)
                    .Cascade.All().Inverse();

                Cache.ReadOnly();
            }
        }

    }
}
