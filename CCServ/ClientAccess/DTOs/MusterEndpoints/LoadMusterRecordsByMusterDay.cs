using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using FluentValidation;

namespace CCServ.ClientAccess.DTOs.MusterEndpoints
{
    /// <summary>
    /// The DTO for the related endpoint.
    /// </summary>
    public class LoadMusterRecordsByMusterDay : DTOBase
    {

        /// <summary>
        /// The muster date for which the client wants to load the muster.
        /// </summary>
        public DateTime MusterDate { get; set; }

        /// <summary>
        /// Creates a new DTO.
        /// </summary>
        /// <param name="obj"></param>
        public LoadMusterRecordsByMusterDay(JObject obj) : base(obj)
        {
        }

        class Validator : AbstractValidator<LoadMusterRecordsByMusterDay>
        {
            public Validator()
            {
                RuleFor(x => x.MusterDate).LessThanOrEqualTo(DateTime.UtcNow);
            }
        }
    }
}
