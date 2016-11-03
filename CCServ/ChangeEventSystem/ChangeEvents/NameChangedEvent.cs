using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CCServ.ChangeEventSystem.ChangeEvents
{
    public class NameChangedEvent : ChangeEventBase
    {

        public override void RaiseEvent(ChangeEventArgsBase eventArgs)
        {
            throw new NotImplementedException();
        }

        public NameChangedEvent()
        {
            Name = "Name Changed Event";
            Description = "Occurs when a person's first name, last name or middle name changes.";
        }
    }
}
