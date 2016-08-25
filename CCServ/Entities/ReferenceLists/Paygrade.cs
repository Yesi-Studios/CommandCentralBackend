using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using FluentValidation.Results;

namespace CCServ.Entities.ReferenceLists
{
    public class Paygrade : IValidatable
    {
        #region Properties

        public string Value { get; set; }

        public string Description { get; set; }

        #endregion

        #region Overrides

        public override string ToString()
        {
            return Value;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            var other = obj as Paygrade;

            if (other == null)
                return false;

            return other.Value == this.Value && other.Description == this.Description;
        }

        #endregion

        public ValidationResult Validate()
        {
            if (!Paygrades.AllPaygrades.Contains(this))
            {
                return new ValidationResult(new List<ValidationFailure> { new ValidationFailure("Object", "The paygrade did not exist in the paygrades collection.") });
            }

            return new ValidationResult();
        }

        

    }
}
