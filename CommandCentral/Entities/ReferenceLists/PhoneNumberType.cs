﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandCentral.ClientAccess;
using FluentNHibernate.Mapping;
using FluentValidation.Results;

namespace CommandCentral.Entities.ReferenceLists
{
    public class PhoneNumberType : ReferenceListItemBase
    {
        /// <summary>
        /// Loads all object or a single object if given an Id.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="token"></param>
        public override List<ReferenceListItemBase> Load(Guid id, MessageToken token)
        {
            using (var session = DataAccess.DataProvider.CreateStatefulSession())
            {
                if (id == default(Guid))
                {
                    return session.QueryOver<PhoneNumberType>()
                        .Cacheable().CacheMode(NHibernate.CacheMode.Normal)
                        .List<ReferenceListItemBase>().ToList();
                }
                else
                {
                    return new[] { (ReferenceListItemBase)session.Get<PhoneNumberType>(id) }.ToList();
                }
            }
        }

        public class PhoneNumberTypeMapping : ClassMap<PhoneNumberType>
        {
            public PhoneNumberTypeMapping()
            {
                Id(x => x.Id).GeneratedBy.Assigned();

                Map(x => x.Value).Not.Nullable().Unique();
                Map(x => x.Description);

                Cache.ReadWrite();
            }
        }
    }
}
