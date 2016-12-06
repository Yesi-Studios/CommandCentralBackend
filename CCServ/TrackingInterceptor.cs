using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CCServ.ClientAccess;
using AtwoodUtils;
using NHibernate;
using NHibernate.Type;

namespace CCServ
{
    public class TrackingInterceptor : EmptyInterceptor
    {

        public MessageToken Token { get; set; }

        public override bool OnSave(object entity, object id, object[] state, string[] propertyNames, IType[] types)
        {
            return base.OnSave(entity, id, state, propertyNames, types);
        }

        public override bool OnFlushDirty(object entity, object id, object[] currentState, object[] previousState, string[] propertyNames, IType[] types)
        {
            //First, we need to know which properties changed.  We can't rely on NHibernate to tell us this because we need to go deeper than it will go.
            for (int x = 0; x < currentState.Length; x++)
            {
                var propertyName = propertyNames[x];

                if (types[x].IsCollectionType)
                {
                    var currentCollection = ((IEnumerable)currentState[x]).Cast<object>().ToList();
                    var previousCollection = ((IEnumerable)previousState[x]).Cast<object>().ToList();

                    var notInCurrent = new List<object>();
                    var notInPrevious = new List<object>();
                    var changes = new List<Tuple<object, object>>();

                    if (!Utilities.GetSetDifferences(currentCollection, previousCollection, out notInCurrent, out notInPrevious, out changes))
                    {
                        int i = 0;
                    }
                }
                else
                {
                }
            }

            return base.OnFlushDirty(entity, id, currentState, previousState, propertyNames, types);
        }

        public TrackingInterceptor(MessageToken token)
        {
            this.Token = token;
        }


    }
}
