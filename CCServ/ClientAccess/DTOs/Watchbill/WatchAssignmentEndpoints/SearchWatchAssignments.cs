using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using FluentValidation;
using CCServ.Entities.Watchbill;

namespace CCServ.ClientAccess.DTOs.Watchbill.WatchAssignmentEndpoints
{
    /// <summary>
    /// The dto for the related endpoint.
    /// </summary>
    public class SearchWatchAssignments : DTOBase
    {
        /// <summary>
        /// The filters to be used during the query of watch assignments.
        /// </summary>
        public Dictionary<string, object> Filters { get; set; }

        /// <summary>
        /// Creates a new dto.
        /// </summary>
        /// <param name="obj"></param>
        public SearchWatchAssignments(JObject obj) : base(obj)
        {
        }

        class Validator : AbstractValidator<SearchWatchAssignments>
        {
            public Validator()
            {
                RuleFor(x => x.Filters).Must(filters =>
                {
                    if (filters == null)
                        return false;

                    foreach (var key in filters.Keys)
                    {
                        if (!typeof(WatchAssignment).GetProperties().Select(x => x.Name).Contains(key, StringComparer.CurrentCultureIgnoreCase))
                            return false;
                    }

                    return true;
                })
                .WithMessage("One or more properties you tried to search were not real.");
            }
        }
    }
}
