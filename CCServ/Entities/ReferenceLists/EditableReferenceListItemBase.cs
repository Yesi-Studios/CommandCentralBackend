using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CCServ.ClientAccess;
using FluentValidation.Results;

namespace CCServ.Entities.ReferenceLists
{
    public abstract class EditableReferenceListItemBase : ReferenceListItemBase
    {

        #region Helper Methods

        /// <summary>
        /// Projected from the IValidatable interface.
        /// </summary>
        /// <returns></returns>
        public abstract ValidationResult Validate();

        public abstract void UpdateOrInsert(Newtonsoft.Json.Linq.JToken item, MessageToken token);

        public abstract void Delete(Guid id, bool forceDelete, MessageToken token);

        #endregion
    }
}
