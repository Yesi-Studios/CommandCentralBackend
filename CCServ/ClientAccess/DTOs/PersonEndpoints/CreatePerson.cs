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
    /// The DTO for the related endpoint.
    /// </summary>
    public class CreatePerson : DTOBase
    {
        /// <summary>
        /// The object containing the person the client wishes to insert into the database.
        /// </summary>
        public PersonDTO Person { get; set; }

        /// <summary>
        /// Creates a new DTO.
        /// </summary>
        /// <param name="obj"></param>
        public CreatePerson(JObject obj) : base(obj)
        {
        }

        /// <summary>
        /// A DTO containing the subset of person properties needed for creating a person.
        /// </summary>
        public class PersonDTO
        {
            /// <summary>
            /// The first name of the new user.
            /// </summary>
            public string FirstName { get; set; }

            /// <summary>
            /// The last name of the new user.
            /// </summary>
            public string LastName { get; set; }

            /// <summary>
            /// The middle name of the new user.  This is not required.
            /// </summary>
            public string MiddleName { get; set; }

            /// <summary>
            /// The paygrade of the new user.
            /// </summary>
            public Entities.ReferenceLists.Paygrade Paygrade { get; set; }

            /// <summary>
            /// The new user's UIC.
            /// </summary>
            public Entities.ReferenceLists.UIC UIC { get; set; }

            /// <summary>
            /// The designation of the new user.
            /// </summary>
            public Entities.ReferenceLists.Designation Designation { get; set; }

            /// <summary>
            /// The new user's sex.
            /// </summary>
            public Entities.ReferenceLists.Sex Sex { get; set; }

            /// <summary>
            /// The SSN of the new user.
            /// </summary>
            public string SSN { get; set; }

            /// <summary>
            /// The DoD ID of the new user.
            /// </summary>
            public string DoDID { get; set; }

            /// <summary>
            /// The new user's date of birth.
            /// </summary>
            public DateTime DateOfBirth { get; set; }

            /// <summary>
            /// The date of arrival of the new user.
            /// </summary>
            public DateTime DateOfArrival { get; set; }

            /// <summary>
            /// The duty status of the new user.
            /// </summary>
            public Entities.ReferenceLists.DutyStatus DutyStatus { get; set; }

            /// <summary>
            /// The PRD of the new user.
            /// </summary>
            public DateTime PRD { get; set; }
        }
    }
}
