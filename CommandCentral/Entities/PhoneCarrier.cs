using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AtwoodUtils;
using FluentNHibernate.Mapping;

namespace CommandCentral.Entities
{
    /// <summary>
    /// Describes a single phone carrier which is intended to aid in the sending of text messages.
    /// </summary>
    public class PhoneCarrier
    {

        #region Properties

        /// <summary>
        /// The unique Guid of this phone carrier.
        /// </summary>
        public virtual Guid Id { get; set; }

        /// <summary>
        /// The name of this phone carrier (eg. Verizon, Sprint, etc.)
        /// </summary>
        public virtual string CompanyName { get; set; }

        /// <summary>
        /// The mailing host of this phone carrier (eg. vtext.com).  An '@' symbol should not be included.
        /// </summary>
        public virtual string MailHost { get; set; }

        /// <summary>
        /// Indicates if this phone carrier has prohibitive, additional fees we should be wary of when sending users text messages.
        /// </summary>
        public virtual bool HasAdditionalFees { get; set; }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Returns a usable mail address which includes the given phone number @ this carrier's host address.
        /// </summary>
        /// <param name="phoneNumber"></param>
        /// <returns></returns>
        public virtual System.Net.Mail.MailAddress GetMailAddress(string phoneNumber)
        {
            return new System.Net.Mail.MailAddress("{0}@{1}".FormatS(phoneNumber, MailHost));
        }

        /// <summary>
        /// Returns a usable mail address which includes the given phone number @ this carrier's host address.
        /// </summary>
        /// <param name="phoneNumber"></param>
        /// <returns></returns>
        public virtual System.Net.Mail.MailAddress GetMailAddress(PhoneNumber phoneNumber)
        {
            return new System.Net.Mail.MailAddress("{0}@{1}".FormatS(phoneNumber.Number, MailHost));
        }

        #endregion

        #region Overrides

        /// <summary>
        /// Returns this carrier's company name followed by the mail host.  Name | Host
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "{0} | {1}".FormatS(CompanyName, MailHost);
        }

        #endregion

        /// <summary>
        /// Maps a phone carrier to the database.
        /// </summary>
        public class PhoneCarrierMapping : ClassMap<PhoneCarrier>
        {
            /// <summary>
            /// Maps a phone carrier to the database.
            /// </summary>
            public PhoneCarrierMapping()
            {
                Id(x => x.Id).GeneratedBy.Guid();

                Map(x => x.HasAdditionalFees).Default(false.ToString());
                Map(x => x.CompanyName).Not.Nullable().Length(50).Unique();
                Map(x => x.MailHost).Not.Nullable().Length(50).Unique();
            }
        }
    }
}
