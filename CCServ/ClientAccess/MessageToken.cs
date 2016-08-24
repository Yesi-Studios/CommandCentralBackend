using System;
using System.Linq;
using System.Collections.Generic;
using System.Globalization;
using NHibernate;
using AtwoodUtils;
using FluentNHibernate.Mapping;

namespace CCServ.ClientAccess
{
    /// <summary>
    /// Describes a message token, including its Access Methods.  Intended to be used to track an interaction with the client.
    /// </summary>
    public class MessageToken
    {

        #region Properties

        /// <summary>
        /// The unique Id assigned to this message interaction
        /// </summary>
        public virtual Guid Id { get; set; }

        /// <summary>
        /// The APIKey that the client used to access the API
        /// </summary>
        public virtual APIKey APIKey { get; set; }

        /// <summary>
        /// The time at which the client called the API.
        /// </summary>
        public virtual DateTime CallTime { get; set; }

        /// <summary>
        /// The Arguments the client sent to the API.  This is the RawJSON transformed into a dictionary.  This is not mapped to the database.
        /// </summary>
        public virtual Dictionary<string, object> Args { get; set; }

        private string _rawRequestBody = "";
        /// <summary>
        /// The body of the request prior to processing.  This should not be used in actual processing because the output is truncated to 10000 characters.
        /// </summary>
        public virtual string RawRequestBody
        {
            get
            {
                return _rawRequestBody.Truncate(10000);
            }
            set
            {
                _rawRequestBody = value;
            }
        }

        /// <summary>
        /// The endpoint that was called by the client.
        /// </summary>
        public virtual string CalledEndpoint { get; set; }

        /// <summary>
        /// The endpoint that was invoked by the client.
        /// </summary>
        public virtual ServiceEndpoint EndpointDescription { get; set; }

        /// <summary>
        /// Any error messages that occur during the request are pushed to this property.
        /// </summary>
        public virtual IList<string> ErrorMessages { get; set; }

        /// <summary>
        /// Indicates if any error messages are contained in the error message collection.
        /// <para/>
        /// If this value is true, the result can not be set.
        /// </summary>
        public virtual bool HasError
        {
            get { return ErrorMessages.Any(); }
                 
        }

        /// <summary>
        /// Describes the error state, if any, that this message token is in.
        /// </summary>
        public virtual ErrorTypes ErrorType { get; set; }

        /// <summary>
        /// The status code that describes the message token's state.
        /// </summary>
        public virtual System.Net.HttpStatusCode StatusCode { get; set; }

        /// <summary>
        /// The resultant object.
        /// </summary>
        public virtual object Result { get; set; }

        /// <summary>
        /// Gets raw response body of this request.  This is simple a property accessor to this.ConstructResponse() for logging purposes.
        /// </summary>
        public virtual string RawResponseBody
        {
            get
            {
                return ConstructResponseString().Truncate(10000);
            }
        }

        /// <summary>
        /// The current state of the message interaction.
        /// </summary>
        public virtual MessageStates State { get; set; }

        /// <summary>
        /// The time at which the message was handled, either successfully or otherwise, and the response was sent to the client.
        /// </summary>
        public virtual DateTime HandledTime { get; set; }

        /// <summary>
        /// The session that was active when the message began.
        /// </summary>
        public virtual AuthenticationSession AuthenticationSession { get; set; }

        /// <summary>
        /// The IP address of the host that called the service.
        /// </summary>
        public virtual string HostAddress { get; set; }

        #endregion

        #region Overrides

        /// <summary>
        /// Casts to a string.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "{0} | {1} | {2}\n\t\tCall Time: {3}\n\t\tProcessing Time: {4}\n\t\tHost: {5}\n\t\tApp Name: {6}\n\t\tSession ID: {7}\n\t\tError Code: {8}\n\t\tStatus Code: {9}\n\t\tMessages: {10}"
                .FormatS(Id, 
                CalledEndpoint, 
                State, 
                CallTime.ToString(CultureInfo.InvariantCulture), 
                DateTime.Now.Subtract(CallTime).ToString(), 
                HostAddress, 
                APIKey == null ? "null" : APIKey.ApplicationName, 
                AuthenticationSession == null ? "null" : AuthenticationSession.Id.ToString(),
                ErrorType,
                StatusCode,
                string.Join("|", ErrorMessages));
        }

        #endregion

        #region ctor

        /// <summary>
        /// Creates a new message token and creates a new Id for it and sets the call time to DateTime.Now while setting the error messages to a blank message.
        /// </summary>
        public MessageToken()
        {
            Id = Guid.NewGuid();
            CallTime = DateTime.Now;
            ErrorMessages = new List<string>();
            State = MessageStates.Received;
            //Initialize the status code to OK.  If the error message is ever set, then that'll change.
            StatusCode = System.Net.HttpStatusCode.OK;
            ErrorType = ErrorTypes.Null;
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Sets the request body and optionally attempts to convert the request body into the args dictionary.  If this conversion fails, the error message will be added to the token.
        /// </summary>
        /// <param name="body" />
        /// <param name="convert" />
        public virtual void SetRequestBody(string body, bool convert)
        {
            RawRequestBody = body;

            if (convert)
            {
                try
                {
                    var dict = body.Deserialize<Dictionary<string, object>>();
                    Args = new Dictionary<string, object>(dict, StringComparer.OrdinalIgnoreCase);
                }
                catch
                {
                    AddErrorMessage("There was an error while attempting to parse your request body.  This request body should be JSON in the form of a dictionary.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                }
            }

        }

        /// <summary>
        /// Adds an error message to the error messages collection and sets the error type and the status code.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="error"></param>
        /// <param name="status"></param>
        public virtual void AddErrorMessage(string message, ErrorTypes error, System.Net.HttpStatusCode status)
        {
            if (Result != null)
                throw new Exception("You can not set a error message on a message token that has already been assigned a return value.  You get one or the other.");

            ErrorMessages.Add(message);
            this.ErrorType = error;
            this.StatusCode = status;
        }

        /// <summary>
        /// Adds multiple error messages to the error messages collection and sets the error type and the status code.
        /// </summary>
        /// <param name="messages"></param>
        /// <param name="error"></param>
        /// <param name="status"></param>
        public virtual void AddErrorMessages(IEnumerable<string> messages, ErrorTypes error, System.Net.HttpStatusCode status)
        {
            if (Result != null)
                throw new Exception("You cannot set a error message on a message token that has already been assigned a return value.  You get one or the other.");

            messages.ToList().ForEach(x => ErrorMessages.Add(x));
            this.ErrorType = error;
            this.StatusCode = status;
        }

        /// <summary>
        /// Sets the result for this message token.  An exception will be thrown if you attempt to set the result on a message that has errors.
        /// </summary>
        /// <param name="result"></param>
        public virtual void SetResult(object result)
        {
            if (HasError)
                throw new Exception("You can not set the result on a message that has errors.");

            Result = result;
        }

        /// <summary>
        /// Returns the JSON string of a return container than contains the response that should be sent back to the client for this object.
        /// </summary>
        /// <returns></returns>
        public virtual string ConstructResponseString()
        {
            return new ReturnContainer
            {
                ErrorMessages = ErrorMessages.ToList(),
                ErrorType = ErrorType,
                HasError = HasError,
                ReturnValue = Result,
                StatusCode = StatusCode
            }.Serialize();
        }

        /// <summary>
        /// Returns the return container that should be returned to the client.
        /// </summary>
        /// <returns></returns>
        public virtual ReturnContainer ContructReturnContainer()
        {
            return new ReturnContainer
            {
                ErrorMessages = ErrorMessages.ToList(),
                ErrorType = ErrorType,
                HasError = HasError,
                ReturnValue = Result,
                StatusCode = StatusCode
            };
        }

        #endregion

        /// <summary>
        /// Maps a message token to the database.
        /// </summary>
        public class MessageTokenMapping : ClassMap<MessageToken>
        {
            /// <summary>
            /// Maps a message token to the database.
            /// </summary>
            public MessageTokenMapping()
            {
                Id(x => x.Id).GeneratedBy.Assigned();

                References(x => x.APIKey).LazyLoad(Laziness.False);
                References(x => x.AuthenticationSession).LazyLoad(Laziness.False);

                Map(x => x.CallTime);
                Map(x => x.RawRequestBody).Length(10000);
                Map(x => x.CalledEndpoint);
                Map(x => x.HasError).Access.ReadOnly();
                Map(x => x.ErrorType);
                Map(x => x.StatusCode);
                Map(x => x.State);
                Map(x => x.HandledTime);
                Map(x => x.HostAddress);
                Map(x => x.RawResponseBody).Length(10000).Access.ReadOnly();

                HasMany(x => x.ErrorMessages)
                    .KeyColumn("MessageTokenId")
                    .Element("ErrorMessage", map => map.Length(10000))
                    .Not.LazyLoad();

            }
        }

    }
}
