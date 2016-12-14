using System;
using System.IO;
using System.Net;
using AtwoodUtils;
using FluentNHibernate.Mapping;
using FluentValidation;

namespace CCServ.Entities
{
    /// <summary>
    /// Describes a single physical address
    /// </summary>
    public class PhysicalAddress
    {

        #region Properties

        /// <summary>
        /// The unique GUID of this physical address.
        /// </summary>
        public virtual Guid Id { get; set; }

        /// <summary>
        /// The street number + route address.
        /// </summary>
        public virtual string Address { get; set; }

        /// <summary>
        /// The city.
        /// </summary>
        public virtual string City { get; set; }

        /// <summary>
        /// The state.
        /// </summary>
        public virtual string State { get; set; }

        /// <summary>
        /// The zip code.
        /// </summary>
        public virtual string ZipCode { get; set; }

        /// <summary>
        /// The country.
        /// </summary>
        public virtual string Country { get; set; }

        /// <summary>
        /// Indicates whether or not the person lives at this address
        /// </summary>
        public virtual bool IsHomeAddress { get; set; }

        /// <summary>
        /// The latitude of this physical address.
        /// </summary>
        public virtual float? Latitude { get; set; }

        /// <summary>
        /// The longitude of this physical address.
        /// </summary>
        public virtual float? Longitude { get; set; }

        #endregion

        #region Overrides

        /// <summary>
        /// Returns the address in this format: 123 Fake Street, Happyville, TX 54321
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "{0}, {1}, {2} {3}".FormatS(Address, City, State, ZipCode);
        }

        /// <summary>
        /// Deep equality comparison
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            var other = obj as PhysicalAddress;
            if (other == null)
                return false;

            return Object.Equals(other.Id, this.Id) &&
                   Object.Equals(other.Address, this.Address) &&
                   Object.Equals(other.City, this.City) &&
                   Object.Equals(other.State, this.State) &&
                   Object.Equals(other.ZipCode, this.ZipCode) &&
                   Object.Equals(other.Country, this.Country) &&
                   Object.Equals(other.IsHomeAddress, this.IsHomeAddress) &&
                   Object.Equals(other.Latitude, this.Latitude) &&
                   Object.Equals(other.Longitude, this.Longitude);
        }

        /// <summary>
        /// hashey codey
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;

                hash = hash * 23 + Utilities.GetSafeHashCode(Id);
                hash = hash * 23 + Utilities.GetSafeHashCode(Address);
                hash = hash * 23 + Utilities.GetSafeHashCode(City);
                hash = hash * 23 + Utilities.GetSafeHashCode(State);
                hash = hash * 23 + Utilities.GetSafeHashCode(ZipCode);
                hash = hash * 23 + Utilities.GetSafeHashCode(Country);
                hash = hash * 23 + Utilities.GetSafeHashCode(IsHomeAddress);
                hash = hash * 23 + Utilities.GetSafeHashCode(Latitude);
                hash = hash * 23 + Utilities.GetSafeHashCode(Longitude);

                return hash;
            }
        }

        #endregion

        #region ctors

        /// <summary>
        /// Creates a new physical address, setting in the Id to a new Guid.
        /// </summary>
        public PhysicalAddress()
        {
            if (Id == default(Guid))
                Id = Guid.NewGuid();
        }

        #endregion

        /// <summary>
        /// Maps a physical address to the database.
        /// </summary>
        public class PhysicalAddressMapping : ClassMap<PhysicalAddress>
        {
            /// <summary>
            /// Maps a physical address to the database.
            /// </summary>
            public PhysicalAddressMapping()
            {
                Id(x => x.Id).GeneratedBy.Assigned();

                Map(x => x.Address).Not.Nullable();
                Map(x => x.City).Not.Nullable();
                Map(x => x.State).Not.Nullable();
                Map(x => x.ZipCode).Not.Nullable();
                Map(x => x.Country);
                Map(x => x.IsHomeAddress).Not.Nullable();
                Map(x => x.Latitude).Nullable();
                Map(x => x.Longitude).Nullable();
            }
        }

        /// <summary>
        /// Validates a physical address
        /// </summary>
        public class PhysicalAddressValidator : AbstractValidator<PhysicalAddress>
        {
            /// <summary>
            /// Validates a physical address
            /// </summary>
            public PhysicalAddressValidator()
            {
                CascadeMode = CascadeMode.StopOnFirstFailure;

                /*RuleFor(x => x.Latitude)
                        //.NotEmpty().WithMessage("Your latitude must not be empty")
                        .Must(x => x >= -90 && x <= 90).WithMessage("Your latitude must be between -90 and 90, inclusive.");

                RuleFor(x => x.Longitude)
                    //.NotEmpty().WithMessage("Your longitude must not be empty")
                    .Must(x => x >= -180 && x <= 180).WithMessage("Your longitude must be between -180 and 180, inclusive.");*/

                RuleFor(x => x.Address)
                    .NotEmpty().WithMessage("Your address must not be empty.")
                    .Length(1, 255).WithMessage("The address must be between 1 and 255 characters.");

                RuleFor(x => x.City)
                    .NotEmpty().WithMessage("Your city must not be empty.")
                    .Length(1, 255).WithMessage("The city must be between 1 and 255 characters.");

                RuleFor(x => x.State)
                    .NotEmpty().WithMessage("Your state must not be empty.")
                    .Length(1, 255).WithMessage("The state must be between 1 and 255 characters.");

                RuleFor(x => x.Country)
                    .Length(0, 255).WithMessage("The country may be no more than 200 characters.");

                RuleFor(x => x.ZipCode)
                    .NotEmpty().WithMessage("You zip code must not be empty.")
                    .Matches(@"^\d{5}(?:[-\s]\d{4})?$").WithMessage("Your zip code was not valid.");
            }
        }

    }

    
}
