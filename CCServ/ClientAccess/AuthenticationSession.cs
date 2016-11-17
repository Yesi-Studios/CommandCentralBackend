using System;
using System.Collections.Generic;
using CCServ.Authorization;
using CCServ.Entities;
using FluentNHibernate.Mapping;

namespace CCServ.ClientAccess
{
    /// <summary>
    /// Describes a single authentication session and provides members for interacting with that session.
    /// </summary>
    public class AuthenticationSession
    {
        /// <summary>
        /// The max age after which a session will have expired and it will become invalid.
        /// </summary>
        private static readonly TimeSpan _maxAge = TimeSpan.FromMinutes(20);

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
        /// The last time this session was used, not counting this current time.
        /// </summary>
        public virtual DateTime LastUsedTime { get; set; }

        /// <summary>
        /// Shows all message tokens that are assigned to this authentication session.
        /// </summary>
        public virtual IList<MessageToken> MessageTokens { get; set; }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Determines if this session has expired given a max age of inactivity.
        /// </summary>
        /// <returns></returns>
        public virtual bool IsValid()
        {
            if (!IsActive)
                return false;

            if (DateTime.UtcNow.Subtract(LastUsedTime) >= _maxAge)
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
                Id(x => x.Id).GeneratedBy.Guid();

                Map(x => x.LoginTime).Not.Nullable();
                Map(x => x.LogoutTime).Nullable();
                Map(x => x.IsActive).Not.Nullable();
                Map(x => x.LastUsedTime);
                References(x => x.Person).LazyLoad(Laziness.False);
                HasMany(x => x.MessageTokens).Inverse().Cascade.All();

                Cache.ReadWrite();
            }
        }

    }
}
