﻿using System;
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

namespace CommandCentral.Entities
{
    /// <summary>
    /// Describes a single Phone number along with its data access members and properties
    /// </summary>
    public class PhoneNumber
    {
        #region Properties

        /// <summary>
        /// The unique GUID of this phone number.
        /// </summary>
        public virtual Guid ID { get; set; }

        /// <summary>
        /// The  person to whom this phone number belongs.
        /// </summary>
        public virtual Person Owner { get; set; }

        /// <summary>
        /// The actual phone number of this phone number object.
        /// </summary>
        public virtual string Number { get; set; }

        /// <summary>
        /// The carrier of this phone number, eg.  Verizon, etc.
        /// </summary>
        public virtual string Carrier { get; set; }

        /// <summary>
        /// The carrier's SMS email address.
        /// </summary>
        public virtual string CarrierMailAddress
        {
            get
            {
                if (this.Carrier == null)
                    return null;
                else
                    return TextMessageHelper.PhoneCarrierMailDomainMappings[this.Carrier];
            }
        }

        /// <summary>
        /// Indicates whether or not the person who owns this phone number wants any contact to occur using it.
        /// </summary>
        public virtual bool IsContactable { get; set; }

        /// <summary>
        /// Indicates whether or not the person who owns this phone number prefers contact to occur on it.
        /// </summary>
        public virtual bool IsPreferred { get; set; }

        /// <summary>
        /// The type of this phone. eg. Cell, Home, Work
        /// </summary>
        public virtual ReferenceLists.PhoneNumberType PhoneType { get; set; }

        #endregion

        /// <summary>
        /// Maps a single phone number to the database.
        /// </summary>
        public class PhoneNumberMapping : ClassMap<PhoneNumber>
        {
            /// <summary>
            /// Maps a single phone number to the database.
            /// </summary>
            public PhoneNumberMapping()
            {
                Table("phone_numbers");

                Id(x => x.ID).GeneratedBy.Guid();

                References(x => x.Owner);
                References(x => x.PhoneType);

                Map(x => x.Number).Not.Nullable().Length(15);
                Map(x => x.Carrier).Nullable().Length(20);
                Map(x => x.IsContactable).Not.Nullable();
                Map(x => x.IsPreferred).Not.Nullable();


            }
        }

    }
}
