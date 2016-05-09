using System;
using System.Linq;
using System.Collections.Generic;
using FluentNHibernate.Mapping;
using NHibernate;
using AtwoodUtils;

namespace CommandCentral.ClientAccess
{
    /// <summary>
    /// Implements the primary message token and provides properties on top of those properties in order to facilitate logging it in the database.
    /// </summary>
    public class LoggableMessageToken : MessageToken
    {

        public virtual string LoggableId
        {
            get { return Id.ToString(); }
        }

        public virtual string LoggableAPIKey
        {
            get { return APIKey.Id.ToString(); }
        }

        public virtual string LoggableApplicationName
        {
            get { return APIKey.ApplicationName; }
        }

        public virtual string LoggableRawRequestBody
        {
            get { return RawRequestBody.Truncate(9000); }
        }

        public virtual string LoggableEndpoint
        {
            get { return Endpoint.Name; }
        }

        public virtual string LoggableErrorMessages
        {
            get { return string.Join(",", ErrorMessages); }
        }


    }
}
