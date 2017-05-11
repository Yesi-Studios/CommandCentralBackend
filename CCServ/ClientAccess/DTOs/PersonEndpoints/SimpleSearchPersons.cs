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
    public class SimpleSearchPersons : DTOBase
    {

        /// <summary>
        /// The search term string.  This string will be split by white space and/or commas in order to retrieve the individual search terms.
        /// </summary>
        public string SearchTerm { get; set; }

        /// <summary>
        /// This optional flag instructs the service to return members with LOSS duty statuses (true) or not to return them (false). Default = false.
        /// </summary>
        [Optional(false)]
        public bool ShowHidden { get; set; }


        /// <summary>
        /// Creates a new dto.
        /// </summary>
        /// <param name="obj"></param>
        public SimpleSearchPersons(JObject obj) : base(obj)
        {
        }

        class Validator : AbstractValidator<SimpleSearchPersons>
        {
            public Validator()
            {
                RuleFor(x => x.SearchTerm).NotEmpty()
                    .WithMessage("You must send a search term. A blank term isn't valid. Sorry :(");
            }
        }
    }
}
