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
            var args = eventArgs as ChangeEventArgs.NameChangedEventArgs;

            if (args == null)
                throw new ArgumentException("The event args were either of the wrong type or null.");

            
        }

        public NameChangedEvent()
        {
            Name = "Name Changed Event";
            Description = "Occurs when a person's first name, last name or middle name changes.";
        }
    }
}
