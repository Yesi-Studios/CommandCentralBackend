﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CCServ.ClientAccess;
using FluentNHibernate.Mapping;

namespace CCServ.Entities.ReferenceLists
{
    /// <summary>
    /// Defines a single account history type and its mapping in the database.
    /// </summary>
    public class AccountHistoryType : ReferenceListItemBase
    {
        /// <summary>
        /// Loads all object or a single object if given an Id.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="token"></param>
        public override void Load(Guid id, MessageToken token)
        {
            using (var session = DataAccess.NHibernateHelper.CreateStatefulSession())
            {
                if (id == default(Guid))
                {
                    token.SetResult(session.QueryOver<AccountHistoryType>().List());
                }
                else
                {
                    token.SetResult(session.Get<AccountHistoryType>(id));
                }
            }
        }

        /// <summary>
        /// Maps this object to the database.
        /// </summary>
        public class AccountHistoryTypeMapping : ClassMap<AccountHistoryType>
        {
            /// <summary>
            /// Maps this object to the database.
            /// </summary>
            public AccountHistoryTypeMapping()
            {
                Id(x => x.Id).GeneratedBy.Assigned();

                Map(x => x.Value).Not.Nullable().Unique();
                Map(x => x.Description);
            }
        }
    }
}
