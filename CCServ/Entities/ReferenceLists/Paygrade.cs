using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentNHibernate.Mapping;
using FluentValidation;
using FluentValidation.Results;

namespace CCServ.Entities.ReferenceLists
{
    public class Paygrade : ReferenceListItemBase
    {
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

            return other.Id == this.Id && other.Value == this.Value && other.Description == this.Description;
        }

        #endregion

        public override ValidationResult Validate()
        {
            throw new NotImplementedException();
        }

        public class PaygradeMapping : ClassMap<Paygrade>
        {
            public PaygradeMapping()
            {
                Id(x => x.Id).GeneratedBy.Assigned();

                Map(x => x.Value).Not.Nullable().Unique();
                Map(x => x.Description);
            }
        }
    }
}
