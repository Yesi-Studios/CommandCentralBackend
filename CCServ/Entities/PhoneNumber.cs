using System;
using CCServ.Entities.ReferenceLists;
using FluentNHibernate.Mapping;
using FluentValidation;
using System.Linq;

namespace CCServ.Entities
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
        public virtual Guid Id { get; set; }

        /// <summary>
        /// The actual phone number of this phone number object.
        /// </summary>
        public virtual string Number { get; set; }

        /// <summary>
        /// Indicates whether or not the person who owns this phone number wants any contact to occur using it.
        /// </summary>
        public virtual bool IsContactable { get; set; }

        /// <summary>
        /// Indicates whether or not the person who owns this phone number prefers contact to occur on it.
        /// </summary>
        public virtual bool IsPreferred { get; set; }

        /// <summary>
        /// The type of this phone. eg. Mobile, Home, Work
        /// </summary>
        public virtual PhoneNumberTypes PhoneType { get; set; }

        #endregion

        #region Helper Methods

        //public virtual System.Net.Mail.mail

        #endregion

        #region Overrides

        /// <summary>
        /// Returns the Number property.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return Number;
        }

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
                Id(x => x.Id).GeneratedBy.Guid();

                Map(x => x.Number).Not.Nullable().Length(15);
                Map(x => x.IsContactable).Not.Nullable();
                Map(x => x.IsPreferred).Not.Nullable();
                Map(x => x.PhoneType).Not.Nullable();
            }
        }

        /// <summary>
        /// Validates the phone number object.
        /// </summary>
        public class PhoneNumberValidator : AbstractValidator<PhoneNumber>
        {
            /// <summary>
            /// Validates the phone number object.
            /// </summary>
            public PhoneNumberValidator()
            {
                RuleFor(x => x.Number).Length(0, 10)
                    .Must(x => x.All(char.IsDigit))
                    .WithMessage("Your phone number must only be 10 digits.");
            }
        }

    }
}
