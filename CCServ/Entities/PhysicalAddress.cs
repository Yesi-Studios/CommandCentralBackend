using System;
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

        #region 

        /// <summary>
        /// Returns the address in this format: 123 Fake Street, Happyville, TX 54321
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "{0}, {2}, {3} {4}".FormatS(Address, City, State, ZipCode);
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

                Map(x => x.Address).Not.Nullable().Length(45);
                Map(x => x.City).Not.Nullable().Length(45);
                Map(x => x.State).Not.Nullable().Length(45);
                Map(x => x.ZipCode).Not.Nullable().Length(45);
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
            public PhysicalAddressValidator()
            {
                //TODO
            }

        }

    }

    
}
