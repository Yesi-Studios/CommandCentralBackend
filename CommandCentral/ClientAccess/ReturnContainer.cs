﻿using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace CommandCentral.ClientAccess
{
    /// <summary>
    /// Describes the container that is used to return data to the client. This object provides a reliable structure of data so that the client knows what to expect while allowing us to return exception data as well.
    /// </summary>
    public class ReturnContainer
    {
        /// <summary>
        /// A boolean that indicates if this return container contains an exception.
        /// </summary>
        public bool HasError
        {
            get
            {
                return ErrorMessages.Any();
            }
        }

        /// <summary>
        /// The error message to be sent back to the client.  If this has a value, HasError should be set to true.
        /// </summary>
        public List<string> ErrorMessages { get; set; } = new List<string>();

        /// <summary>
        /// Indicates what type of error is contained in the error message.  Is HasError is false, then this value should be null.
        /// </summary>
        public ErrorTypes ErrorType { get; set; }

        /// <summary>
        /// The actual return value itself.  Most commonly, this should be some sort of object that implements IEnumerable or ISerializable, but anything will work here as long as it can be serialized by JSON.NET.
        /// <para />
        /// I chose not to try to constrain types that go into this property because JSON.NET has proven to be able to serialize some crazy shit.
        /// </summary>
        public object ReturnValue { get; set; }

        /// <summary>
        /// The HTTP status code that indicates how the request was handled.
        /// </summary>
        public System.Net.HttpStatusCode StatusCode { get; set; }


    }
}
