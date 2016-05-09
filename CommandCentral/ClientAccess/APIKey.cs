using System;
using FluentNHibernate.Mapping;

namespace CommandCentral.ClientAccess
{
    /// <summary>
    /// Describes a single API Key.
    /// </summary>
    public class APIKey
    {

        #region Properties

        /// <summary>
        /// The unique Id of this API Key.  This is also the API Key itself.
        /// </summary>
        public virtual Guid Id { get; set; }

        /// <summary>
        /// The name of the application to which this API Key was unsigned.
        /// </summary>
        public virtual string ApplicationName { get; set; }

        #endregion

        /// <summary>
        /// Provides mapping declarations to the database for the API Key.
        /// </summary>
        public class ApiKeyMap : ClassMap<APIKey>
        {
            /// <summary>
            /// Maps the API Key to the database.
            /// </summary>
            public ApiKeyMap()
            {
                Table("api_keys");

                Id(x => x.Id).GeneratedBy.Guid();

                Map(x => x.ApplicationName).Unique().Length(40);

                Cache.ReadWrite();
            }
        }

    }
}
