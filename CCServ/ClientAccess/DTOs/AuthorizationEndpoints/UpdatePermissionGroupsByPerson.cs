using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using FluentValidation;

namespace CCServ.ClientAccess.DTOs.AuthorizationEndpoints
{
    /// <summary>
    /// DTO for the endpoint.
    /// </summary>
    public class UpdatePermissionGroupsByPerson : DTOBase
    {
        /// <summary>
        /// The id of the person the client wants to change.
        /// </summary>
        public Guid PersonId { get; set; }

        /// <summary>
        /// The list of permission groups the client wants to set the user's permission to.
        /// </summary>
        public List<string> PermissionGroups { get; set; }

        /// <summary>
        /// Creates a new dto.
        /// </summary>
        /// <param name="obj"></param>
        public UpdatePermissionGroupsByPerson(JObject obj) : base(obj)
        {
        }

        class Validator : AbstractValidator<UpdatePermissionGroupsByPerson>
        {
            public Validator()
            {
                RuleForEach(x => x.PermissionGroups).Must((obj, element) => CCServ.Authorization.Groups.PermissionGroup.AllPermissionGroups.Any(x => x.GroupName == element))
                    .WithMessage("One or more of your permission groups were not valid.");
            }
        }
    }
}
