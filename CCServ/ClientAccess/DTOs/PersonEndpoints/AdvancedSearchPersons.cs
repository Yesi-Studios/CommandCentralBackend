using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using FluentValidation;

namespace CCServ.ClientAccess.DTOs.PersonEndpoints
{
    /// <summary>
    /// The dto used for the related endpoint.
    /// </summary>
    public class AdvancedSearchPersons : DTOBase
    {

        /// <summary>
        /// The properties to search in and the values to search for.  Search/Query strategies vary by property.
        /// </summary>
        public Dictionary<string, object> Filters { get; set; }

        /// <summary>
        /// The fields the client would like be returned.
        /// </summary>
        public List<string> ReturnFields { get; set; }

        /// <summary>
        /// The optional search level at which the client wants to search.  Omitting this will result in a search in those fields that do not require permissions to search in.  Valid values are Command, Department, Division.
        /// </summary>
        [Optional(null)]
        public string SearchLevel { get; set; }

        /// <summary>
        /// This optional flag instructs the service to return members with LOSS duty statuses (true) or not to return them (false). Default = false.
        /// </summary>
        [Optional(false)]
        public bool ShowHidden { get; set; }

        /// <summary>
        /// The latitude at which to center a geo-query.  A geo query uses the centerlat, centerlong, and radius properties to limit the results based on users' physical addresses.  If one property is included, all others must also be included.
        /// </summary>
        [Optional(null)]
        public double? CenterLat { get; set; }

        /// <summary>
        /// The longitude at which to center a geo query.  A geo query uses the centerlat, centerlong, and radius properties to limit the results based on users' physical addresses.  If one property is included, all others must also be included.
        /// </summary>
        [Optional(null)]
        public double? CenterLong { get; set; }

        /// <summary>
        /// The distance around which to look during a geo query.  A geo query uses the centerlat, centerlong, and radius properties to limit the results based on users' physical addresses.  If one property is included, all others must also be included.
        /// </summary>
        [Optional(null)]
        public double? Radius { get; set; }

        /// <summary>
        /// Creates a new dto.
        /// </summary>
        /// <param name="obj"></param>
        public AdvancedSearchPersons(JObject obj) : base(obj)
        {
        }

        class Validator : AbstractValidator<AdvancedSearchPersons>
        {
            public Validator()
            {
                When(x => x.CenterLat.HasValue || x.CenterLong.HasValue || x.Radius.HasValue, () =>
                {
                    RuleFor(x => x.Radius).Must(x => x.HasValue);
                    RuleFor(x => x.CenterLat).Must(x => x.HasValue);
                    RuleFor(x => x.CenterLong).Must(x => x.HasValue);
                });
            }
        }
    }
}
