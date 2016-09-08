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
