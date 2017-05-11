using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using AtwoodUtils;
using FluentValidation;
using System.ComponentModel;

namespace CCServ.ClientAccess.DTOs
{
    /// <summary>
    /// The base type for all DTOs.
    /// </summary>
    public abstract class DTOBase
    {
        /// <summary>
        /// The authentication token is how the client indicates who they are.
        /// </summary>
        [Description("A Guid which represents your authentication session.  This is obtained from the /Login method.")]
        public Guid AuthenticationToken { get; set; }

        /// <summary>
        /// The api key is how the client indicates which application they are calling from.  This is purely for metrics purposes.
        /// </summary>
        [Required]
        [Description("A Guid which represents the application from which you are calling.")]
        public Guid ApiKey { get; set; }

        /// <summary>
        /// Creates a DTO object from the given jObject, using a concrete type's implementation and properties.  In most cases, you should not have any code in your own constructors.
        /// </summary>
        /// <param name="obj"></param>
        public DTOBase(JObject obj)
        {
            //First get all properties of this class.
            var properties = GetType().GetProperties();
            var jProps = obj.Properties();

            //Now look for each one and place them into this.
            foreach (var prop in properties)
            {
                var jProp = jProps.FirstOrDefault(x => x.Name.InsensitiveEquals(prop.Name));

                //This means the property wasn't given to us.
                if (jProp == null)
                {
                    if (prop.GetCustomAttributes(typeof(RequiredAttribute), true).Any())
                    {
                        throw new CommandCentralException("You must send all of the following parameters: {0}"
                            .FormatS(String.Join(", ", properties.Where(x => x.GetCustomAttributes(typeof(RequiredAttribute), true).Any()).Select(x => x.Name))), ErrorTypes.Validation);
                    }

                    prop.SetValue(this, Utilities.GetDefault(prop.PropertyType));
                }
                else
                {
                    prop.SetValue(this, jProp.Value.ToObject(prop.PropertyType));
                }
            }
        }

        /// <summary>
        /// Validates this object.
        /// </summary>
        /// <returns></returns>
        public virtual FluentValidation.Results.ValidationResult Validate()
        {
            var validatorType = GetType().GetNestedTypes().FirstOrDefault(x => Utilities.IsSubclassOfRawGeneric(typeof(AbstractValidator<>), x));

            if (validatorType == null)
                return new FluentValidation.Results.ValidationResult();

            dynamic validator = Activator.CreateInstance(validatorType);

            return validator.Validate(this);
        }
    }
}
