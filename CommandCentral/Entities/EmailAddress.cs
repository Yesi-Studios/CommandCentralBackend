using System;
using AtwoodUtils;
using System.Linq;
using FluentNHibernate.Mapping;
using FluentValidation;
using System.Collections.Generic;

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

                return elements.Last().SafeEquals("mail.mil");
            }
        }

        #endregion

        #region Overrides

        /// <summary>
        /// Deep comparison.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            var other = obj as EmailAddress;
            if (other == null)
                return false;

            return Object.Equals(other.Address, Address) &&
                   Object.Equals(other.Id, Id) &&
                   Object.Equals(other.IsContactable, IsContactable) &&
                   Object.Equals(other.IsPreferred, IsPreferred);
        }

        /// <summary>
        /// hashey codey
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            unchecked
            {
                var hash = 17;

                hash = hash * 23 + Utilities.GetSafeHashCode(Id);
                hash = hash * 23 + Utilities.GetSafeHashCode(Address);
                hash = hash * 23 + Utilities.GetSafeHashCode(IsContactable);
                hash = hash * 23 + Utilities.GetSafeHashCode(IsPreferred);

                return hash;
            }
        }

        /// <summary>
        /// Returns a string representation.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            var preferences = new List<string>();
            if (IsContactable)
                preferences.Add("C");
            if (IsPreferred)
                preferences.Add("P");
            
            var final = preferences.Any() ? "({0})".With(String.Join("|", preferences)) : "";

            return "{0} {1}".With(Address, final);
        }

        #endregion

        #region ctors

        public EmailAddress()
        {
            if (Id == default(Guid))
                Id = Guid.NewGuid();
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
                Id(x => x.Id).GeneratedBy.Assigned();

                Map(x => x.Address).Not.Nullable().Unique();
                Map(x => x.IsContactable).Not.Nullable();
                Map(x => x.IsPreferred).Not.Nullable();
            }
        }

        /// <summary>
        /// Validates an Email address
        /// </summary>
        public class EmailAddressValidator : AbstractValidator<EmailAddress>
        {
            /// <summary>
            /// Validates an Email address
            /// </summary>
            public EmailAddressValidator()
            {
                RuleFor(x => x.Address).Must(x =>
                    {
                        try
                        {
                            var address = new System.Net.Mail.MailAddress(x);
                            return true;
                        }
                        catch
                        {
                            return false;
                        }
                    });
            }
        }
    }
}
