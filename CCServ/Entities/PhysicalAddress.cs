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

        #region Client Access Methods

        /// <summary>
        /// WARNING!  THIS METHOD IS EXPOSED TO THE CLIENT AND IS NOT INTENDED FOR INTERNAL USE.  AUTHENTICATION, AUTHORIZATION AND VALIDATION MUST BE HANDLED PRIOR TO DB INTERACTION.
        /// <para/>
        /// Acts as an in between for the US Census and the front end.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        [ClientAccess.EndpointMethod(EndpointName = "ResolveAddress", AllowArgumentLogging = true, AllowResponseLogging = true, RequiresAuthentication = false)]
        private static void EndpointMethod_ResolveAddress(ClientAccess.MessageToken token)
        {
            if (!token.Args.ContainsKeys("address", "city", "state", "zip"))
            {
                token.AddErrorMessage("You must send the parameters address, city, state, and zip.", ClientAccess.ErrorTypes.Validation, HttpStatusCode.BadRequest);
                return;
            }

            token.SetResult(ResolveAddressAgainstCensusData(
                token.Args["address"] as string, token.Args["city"] as string, token.Args["state"] as string, token.Args["zip"] as string));
        }

        #endregion

        #region US Census Interaction

        /// <summary>
        /// Executes a query to the US Census data and returns its response as a JObject.
        /// </summary>
        /// <param name="street"></param>
        /// <param name="city"></param>
        /// <param name="state"></param>
        /// <param name="zip"></param>
        /// <returns></returns>
        public static Newtonsoft.Json.Linq.JObject ResolveAddressAgainstCensusData(string street, string city, string state, string zip)
        {
            try
            {
                //This is the query mask for the geocoding service.
                string apiQueryMask = @"https://geocoding.geo.census.gov/geocoder/locations/address?street={0}&city={1}&state={2}&zip={3}&benchmark=4&layers=all&format=json";

                //Set up the request.
                var request = WebRequest.Create(apiQueryMask.FormatS(street, city, state, zip));
                request.ContentType = "application/json; charset=utf-8";

                //Get the response.
                var response = (HttpWebResponse)request.GetResponse();

                //Read the response.
                using (var reader = new StreamReader(response.GetResponseStream()))
                {
                    return reader.ReadToEnd().DeserializeToJObject();
                }
            }
            catch (Exception e)
            {
                Logging.Log.Exception(e, "An error occurred while communicating with and/or parsing the data from the US Census geocoding service.");
                throw;
            }
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

                RuleFor(x => x.Latitude)
                        .NotEmpty().WithMessage("Your latitude must not be empty")
                        .Must(x => x >= -90 && x <= 90).WithMessage("Your latitude must be between -90 and 90, inclusive.");

                RuleFor(x => x.Longitude)
                    .NotEmpty().WithMessage("Your longitude must not be empty")
                    .Must(x => x >= -180 && x <= 180).WithMessage("Your longitude must be between -180 and 180, inclusive.");

                RuleFor(x => x.Address)
                    .NotEmpty().WithMessage("Your address must not be empty.")
                    .Length(1, 255).WithMessage("The address must be between 1 and 255 characters.");

                RuleFor(x => x.City)
                    .NotEmpty().WithMessage("Your city must not be empty.")
                    .Length(1, 255).WithMessage("The city must be between 1 and 255 characters.");

                RuleFor(x => x.State)
                    .NotEmpty().WithMessage("Your state must not be empty.")
                    .Length(1, 255).WithMessage("The state must be between 1 and 255 characters.");

                RuleFor(x => x.ZipCode)
                    .NotEmpty().WithMessage("You zip code must not be empty.")
                    .Matches(@"^\d{5}(?:[-\s]\d{4})?$").WithMessage("Your zip code was not valid.");
            }
        }

    }

    
}
