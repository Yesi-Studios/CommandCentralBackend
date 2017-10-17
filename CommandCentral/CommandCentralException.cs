﻿using CommandCentral.ClientAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommandCentral
{
    /// <summary>
    /// Indicates that an error was generated by Command Central and the client needs to be informed that something has gone wrong.
    /// </summary>
    public class CommandCentralException : Exception
    {
        /// <summary>
        /// The status code contained in this exception.
        /// </summary>
        public ErrorTypes ErrorType { get; private set; }

        /// <summary>
        /// Creates a new command central exception.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="type"></param>
        public CommandCentralException(string message, ErrorTypes type) 
            : base(message)
        {
            ErrorType = type;
        }
    }
}
