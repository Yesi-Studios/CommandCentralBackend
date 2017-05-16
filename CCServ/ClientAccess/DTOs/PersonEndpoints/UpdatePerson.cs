using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation.Results;
using Newtonsoft.Json.Linq;

namespace CCServ.ClientAccess.DTOs.PersonEndpoints
{
    /// <summary>
    /// The dto used for the related endpoint.
    /// </summary>
    public class UpdatePerson : DTOBase
    {

        /// <summary>
        /// The person parameter represents the state of a person the client wishes to set in the database.  This person will be compared to the current database state of the same person.  Any differences will be considered by the permissions system and then comitted.
        /// </summary>
        public Entities.Person Person { get; set; }

        /// <summary>
        /// Creates a new dto.
        /// </summary>
        /// <param name="obj"></param>
        public UpdatePerson(JObject obj) : base(obj)
        {
        }

        /// <summary>
        /// Validates the person parameter by calling the person validation on it.
        /// </summary>
        /// <returns></returns>
        public override ValidationResult Validate()
        {
            return new Entities.Person.PersonValidator().Validate(this.Person);
        }
    }
}
