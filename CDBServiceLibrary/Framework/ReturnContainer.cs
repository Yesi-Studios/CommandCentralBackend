using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using System.Threading.Tasks;
using AtwoodUtils;

namespace UnifiedServiceFramework.Framework
{
    /// <summary>
    /// Describes the container that is used to return data to the client. This object provides a reliable structure of data so that the client knows what to expect while allowing us to return exception data as well.
    /// </summary>
    [DataContract]
    public class ReturnContainer
    {
        /// <summary>
        /// A boolean that indicates if this return container contains an exception.  If this is true, error message should be set, else ReturnValue should be set.
        /// </summary>
        [DataMember]
        public bool HasError { get; set; }

        private string _errorMessage = "";
        /// <summary>
        /// The error message to be sent back to the client.  If this has a value, HasError should be set to true.
        /// </summary>
        [DataMember]
        public string ErrorMessage
        {
            get
            {
                return _errorMessage;
            }
            set
            {
                _errorMessage = value;
            }
        }

        private ErrorTypes _errorType = ErrorTypes.NULL;

        /// <summary>
        /// Indicates what type of error is contained in the error message.  Is HasError is false, then this value should be null.
        /// </summary>
        public ErrorTypes ErrorType
        {
            get
            {
                return _errorType;
            }
            set
            {
                _errorType = value;
            }
        }

        /// <summary>
        /// The actual return value itself.  Most commonly, this should be some sort of object that implements IEnumerable or ISerializable, but anything will work here as long as it can be serialized by JSON.NET.
        /// <para />
        /// I chose not to try to constrain types that go into this property because JSON.NET has proven to be able to serialize some crazy shit.
        /// </summary>
        [DataMember]
        public object ReturnValue { get; set; }

    }
}
