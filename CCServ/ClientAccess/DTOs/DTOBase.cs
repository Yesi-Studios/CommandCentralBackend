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

                //This means the property wasn't given to us.  Let's see if we can't find a default value for it.
                if (jProp == null)
                {
                    var optionalAttribute = (OptionalAttribute)prop.GetCustomAttributes(typeof(OptionalAttribute), true).FirstOrDefault();

                    //The property was not given to us, but the property is NOT optional.  Therefore we need to throw an exception.
                    if (optionalAttribute == null)
                    {
                        throw new CommandCentralException("You must send all of the following parameters: {0}"
                            .FormatS(String.Join(", ", properties.Where(x => !x.GetCustomAttributes(typeof(OptionalAttribute), true).Any()).Select(x => x.Name))), ErrorTypes.Validation);
                    }

                    //Ok, so the property was not given to us, but there is an optional attribute on it.
                    //That's ok, then we just need to grab the default value and apply it.
                    prop.SetValue(this, optionalAttribute.DefaultValue);
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
