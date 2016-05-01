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

namespace CommandCentral.Entities
{
    /// <summary>
    /// Describes a single email address along with its data access methods
    /// </summary>
    public class EmailAddress
    {

        #region Properties

        /// <summary>
        /// The unique GUID of this Email Address
        /// </summary>
        public virtual string ID { get; set; }

        /// <summary>
        /// The person that owns this email address.
        /// </summary>
        public virtual Person Owner { get; set; }

        /// <summary>
        /// The actual email address of this object.
        /// </summary>
        public virtual string Address { get; set; }

        /// <summary>
        /// Indicates whether or not a person wants to be contacted using this email address.
        /// </summary>
        public virtual bool IsContactable { get; set; }

        /// <summary>
        /// Indicates whether or not the client prefers to be contacted using this email address.
        /// </summary>
        public virtual bool IsPreferred { get; set; }

        /// <summary>
        /// Indicates whether or not this email address is a mail.mil email address.  This is a calulated field, built using the Address field.
        /// </summary>
        public bool IsDODEmailAddress
        {
            get
            {
                var elements = this.Address.Split(new[] { "@" }, StringSplitOptions.RemoveEmptyEntries);
                if (!elements.Any())
                    return false;

                return elements.Last().SafeEquals(EmailHelper.RequiredDODEmailHost);
            }
        }

        #endregion

        /// <summary>
        /// Maps an email address to the database.
        /// </summary>
        public class EmailAddressMapping : ClassMap<EmailAddress>
        {
            /// <summary>
            /// Maps an email address to the database.
            /// </summary>
            public EmailAddressMapping()
            {
                Table("emailaddresses");

                Id(x => x.ID).GeneratedBy.Guid();

                References(x => x.Owner).Not.Nullable();
                Map(x => x.Address).Not.Nullable().Unique();
                Map(x => x.IsContactable).Not.Nullable();
                Map(x => x.IsPreferred).Not.Nullable();
            }
        }

    }
}
