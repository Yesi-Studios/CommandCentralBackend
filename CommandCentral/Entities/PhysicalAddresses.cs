﻿using System;
using FluentNHibernate.Mapping;

namespace CommandCentral.Entities
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
        /// The person who owns this physical address.
        /// </summary>
        public virtual Person Owner { get; set; }

        /// <summary>
        /// The street number.
        /// </summary>
        public virtual string StreetNumber { get; set; }

        /// <summary>
        /// The Route...
        /// </summary>
        public virtual string Route { get; set; }

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
                Table("physicaladdresses");

                Id(x => x.Id);

                References(x => x.Owner).Not.Nullable();

                Map(x => x.StreetNumber).Not.Nullable().Length(45);
                Map(x => x.Route).Not.Nullable().Length(45);
                Map(x => x.City).Not.Nullable().Length(45);
                Map(x => x.State).Not.Nullable().Length(45);
                Map(x => x.ZipCode).Not.Nullable().Length(45);
                Map(x => x.Country).Not.Nullable().Length(45);
                Map(x => x.IsHomeAddress).Not.Nullable();
                Map(x => x.Latitude).Not.Nullable();
                Map(x => x.Longitude).Not.Nullable();

            }
        }

    }

    
}
