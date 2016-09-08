using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CCServ.ClientAccess;
using FluentNHibernate.Mapping;
using FluentValidation.Results;

namespace CCServ.Entities.ReferenceLists
{
    public class PhoneNumberType : ReferenceListItemBase
    {
        public override void Delete(string entityName, Guid id, bool forceDelete, MessageToken token)
        {
            throw new NotImplementedException();
        }

        public override List<ReferenceListItemBase> Load(string entityName, MessageToken token)
        {
            throw new NotImplementedException();
        }

        public override void UpdateOrInsert(string entityName, ReferenceListItemBase item, MessageToken token)
        {
            throw new NotImplementedException();
        }

        public override ValidationResult Validate()
        {
            throw new NotImplementedException();
        }

        public class PhoneNumberTypeMapping : ClassMap<PhoneNumberType>
        {
            public PhoneNumberTypeMapping()
            {
                Id(x => x.Id).GeneratedBy.Assigned();

                Map(x => x.Value).Not.Nullable().Unique();
                Map(x => x.Description);
            }
        }
    }
}
