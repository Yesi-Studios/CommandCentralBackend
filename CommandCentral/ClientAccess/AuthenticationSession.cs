using System;
using System.Collections.Generic;
using CommandCentral.Authorization;
using CommandCentral.Entities;
using FluentNHibernate.Mapping;

namespace CommandCentral.ClientAccess
{
    /// <summary>
    /// Describes a single authentication session and provides members for interacting with that session.
    /// </summary>
    public class AuthenticationSession
    {
        /// <summary>
        /// The max age after which a session will have expired and it will become invalid.
        /// </summary>
        public static readonly TimeSpan MaxAge = TimeSpan.FromMinutes(20);

        #region Properties

        /// <summary>
        /// The Id of the session.  This Id should also be used as the authentication token by the client.
        /// </summary>
        public virtual Guid Id { get; set; }

        /// <summary>
        /// The time this session was created which is the time the client logged in.
        /// </summary>
        public virtual DateTime LoginTime { get; set; }

        /// <summary>
        /// The person to whom this session belongs.
        /// </summary>
        public virtual Person Person { get; set; }

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
        public virtual IList<PermissionGroup> Permissions { get; set; }

        /// <summary>
        /// The last time this session was used, not counting this current time.
        /// </summary>
        public virtual DateTime LastUsedTime { get; set; }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Determines if this session has expired given a max age of inactivity.
        /// </summary>
        /// <returns></returns>
        public virtual bool IsExpired()
        {
            if (DateTime.Now.Subtract(LastUsedTime) > MaxAge)
                return true;

            return false;
        }

        #endregion

        /// <summary>
        /// Maps a session to the database.
        /// </summary>
        public class SessionMapping : ClassMap<AuthenticationSession>
        {
            /// <summary>
            /// Maps a session to the database.
            /// </summary>
            public SessionMapping()
            {
                Table("authentication_sessions");

                Id(x => x.Id).GeneratedBy.Guid();

                Map(x => x.LoginTime).Not.Nullable();
                Map(x => x.LogoutTime).Nullable();
                Map(x => x.IsActive).Not.Nullable();
                Map(x => x.LastUsedTime);
                References(x => x.Person);
                HasManyToMany(x => x.Permissions)
                    .Cascade.All().Inverse();

                Cache.ReadWrite();
            }
        }

    }
}
