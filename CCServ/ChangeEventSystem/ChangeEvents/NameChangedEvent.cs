﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CCServ.ChangeEventSystem.ChangeEvents
{
    /// <summary>
    /// The name changed event, meant to be raised if a person's name changes.
    /// </summary>
    public class NameChangedEvent : ChangeEventBase
    {

        /// <summary>
        /// Raises the name changed event.
        /// </summary>
        /// <param name="eventArgs"></param>
        public override void RaiseEvent(object state)
        {
            var args = state as Email.Models.NameChangedEventEmailModel;

            if (args == null)
                throw new ArgumentException("The state object was either of the wrong type or null.");

            
        }

        /// <summary>
        /// Set up the name changed event.
        /// </summary>
        public NameChangedEvent()
        {
            Name = "Name Changed";
            Description = "Occurs when a person's first name, last name or middle name changes.";
        }
    }
}
