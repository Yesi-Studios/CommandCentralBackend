using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        /// The unique ID of this API Key.  This is also the API Key itself.
        /// </summary>
        public virtual Guid ID { get; set; }

        /// <summary>
        /// The name of the application to which this API Key was unsigned.
        /// </summary>
        public virtual string ApplicationName { get; set; }

        #endregion

        /// <summary>
        /// Provides mapping declarations to the database for the API Key.
        /// </summary>
        public class APIKeyMap : ClassMap<APIKey>
        {
            /// <summary>
            /// Maps the API Key to the database.
            /// </summary>
            public APIKeyMap()
            {
                Table("api_keys");

                Id(x => x.ID).GeneratedBy.Guid();

                Map(x => x.ApplicationName).Unique().Length(40);

                Cache.ReadOnly();
            }
        }

    }
}
