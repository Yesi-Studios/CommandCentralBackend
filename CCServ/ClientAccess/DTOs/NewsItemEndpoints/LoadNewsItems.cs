using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using FluentValidation;

namespace CCServ.ClientAccess.DTOs.NewsItemEndpoints
{
    /// <summary>
    /// The DTO for the related endpoint.
    /// </summary>
    public class LoadNewsItems : DTOBase
    {

        /// <summary>
        /// The number of news items to load.
        /// </summary>
        [Optional(null)]
        public int? Limit { get; set; }

        /// <summary>
        /// Creates a new dto.
        /// </summary>
        /// <param name="obj"></param>
        public LoadNewsItems(JObject obj) : base(obj)
        {
        }

        class Validator : AbstractValidator<LoadNewsItems>
        {
            public Validator()
            {
                RuleFor(x => x.Limit).Must(limit =>
                {
                    if (limit.HasValue && limit.Value <= 0)
                        return false;

                    return true;
                })
                .WithMessage("Your limit must either be null or greater than zero.");
            }
        }
    }
}
