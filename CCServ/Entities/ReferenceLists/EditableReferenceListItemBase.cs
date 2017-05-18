using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CCServ.ClientAccess;
using FluentValidation.Results;

namespace CCServ.Entities.ReferenceLists
{
    /// <summary>
    /// This abstract class provides methods for interacting with reference lists that can be edited.
    /// </summary>
    public abstract class EditableReferenceListItemBase : ReferenceListItemBase
    {
        #region Helper Methods

        /// <summary>
        /// Projected from the IValidatable interface.
        /// </summary>
        /// <returns></returns>
        public abstract ValidationResult Validate();

        #endregion
    }
}
