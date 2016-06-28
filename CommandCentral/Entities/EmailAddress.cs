using System;
using AtwoodUtils;
using System.Linq;
using FluentNHibernate.Mapping;
using FluentValidation;

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
        public virtual Guid Id { get; set; }

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
        /// Indicates whether or not this email address is a mail.mil email address.  This is a calculated field, built using the Address field.
        /// </summary>
        public virtual bool IsDodEmailAddress
        {
            get
            {
                var elements = Address.Split(new[] { "@" }, StringSplitOptions.RemoveEmptyEntries);
                if (!elements.Any())
                    return false;

                return elements.Last().SafeEquals(EmailHelper.RequiredDODEmailHost);
            }
        }

        #endregion

        #region Overrides

        /// <summary>
        /// Returns the Address field.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return Address;
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
                Id(x => x.Id).GeneratedBy.Guid();

                Map(x => x.Address).Not.Nullable().Unique();
                Map(x => x.IsContactable).Not.Nullable();
                Map(x => x.IsPreferred).Not.Nullable();
            }
        }

        public class EmailAddressValidator : AbstractValidator<EmailAddress>
        {
            public EmailAddressValidator()
            {
                //TODO
            }
        }

    }
}
