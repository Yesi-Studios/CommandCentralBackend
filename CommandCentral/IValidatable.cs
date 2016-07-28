using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommandCentral
{
    /// <summary>
    /// Forces consumers to implement a validate method.
    /// </summary>
    public interface IValidatable
    {
        /// <summary>
        /// A validation method that all consumers must implement.  Returns a validation result from FluentValidation.
        /// </summary>
        /// <returns></returns>
        FluentValidation.Results.ValidationResult Validate();
    }
}
