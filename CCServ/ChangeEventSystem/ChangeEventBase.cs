using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CCServ.ChangeEventSystem
{
    public abstract class ChangeEventBase
    {

        #region Properties

        public string Name { protected set; get; }

        public string Description { protected set; get; }

        public bool RequiresChainOfCommand { protected set; get; }

        public Dictionary<string, Dictionary<string, List<string>>> RequiredFields { protected set; get; }

        public abstract void RaiseEvent(object state);

        #endregion

    }
}
