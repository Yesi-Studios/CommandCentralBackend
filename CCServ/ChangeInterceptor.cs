using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CCServ.ClientAccess;
using NHibernate;
using NHibernate.Type;

namespace CCServ
{
    /// <summary>
    /// The interceptor responsible for detecting changes and raising the change events.
    /// </summary>
    class ChangeInterceptor : EmptyInterceptor 
    {
        /// <summary>
        /// The message token representing this transaction.
        /// </summary>
        public MessageToken Token { get; set; }

        /// <summary>
        /// Intercepts the on save event in order to capture change events and handle them.
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="id"></param>
        /// <param name="state"></param>
        /// <param name="propertyNames"></param>
        /// <param name="types"></param>
        /// <returns></returns>
        public override bool OnSave(object entity, object id, object[] state, string[] propertyNames, IType[] types)
        {
            //Check to see if we have a change.  Allow everything else to pass to base.OnSave().
            if (entity is Entities.Change)
            {
                var change = entity as Entities.Change;


            }

            return base.OnSave(entity, id, state, propertyNames, types);
        }
    }
}
