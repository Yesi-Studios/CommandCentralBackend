using System;
using System.Linq;
using System.Collections.Generic;
using System.Globalization;
using NHibernate;
using AtwoodUtils;
using FluentNHibernate.Mapping;

namespace CommandCentral.ClientAccess
{
    /// <summary>
    /// An enum describing the possible states a message can be in.
    /// </summary>
    public enum MessageStates
    {
        /// <summary>
        /// Indicates that a request has been received and has not begun processing.  
        /// <para />
        /// Requests in this state have not undergone validation of any type and assumptions regarding their contained data should not be made.
        /// </summary>
        Received,
        /// <summary>
        /// Indicates a message has been processed successfully prior to authentication or method invocation.
        /// </summary>
        Processed,
        /// <summary>
        /// Inidicates a message has been successfully authenticated prior to method invocation but after message processing.
        /// </summary>
        Authenticated,
        /// <summary>
        /// Indicates that the message has completed invoking its endpoint's data handler.  The final actions are final logging and response release.
        /// </summary>
        Invoked,
        /// <summary>
        /// Indicates that the message has been processed, possible authenticated, and has passed its data handler.
        /// <para />
        /// Additionally, the request has completed final logging and is only waiting to release the response.
        /// <para />
        /// At this point the message is complete and the response need only be sent back to the client.
        /// </summary>
        Handled,
        /// <summary>
        /// Indicates the message failed during the processing step.
        /// </summary>
        FailedAtProcessing,
        /// <summary>
        /// Indicates the message failed during the authentication step.
        /// </summary>
        FailedAtAuthentication,
        /// <summary>
        /// Indicates the message failed during the invocation step.
        /// </summary>
        FailedAtInvocation,
        /// <summary>
        /// Indicates the message failed during the final handling step.
        /// </summary>
        FailedAtFinalHandling,
        /// <summary>
        /// Indicates that we shat the bed so fucking hard that the service collapsed under the weight of the shit in the bed.  This epic amount of shit then tore through the floor boards, collapsed the supports under the floor we were staying on and then slammed straight into the .NET framework (or some other framework).  Freaking out, .NET or another framework screamed a bloody scream. "WHY WOULD YOU DO THAT?!", it exclaimed in dismay. It then balled up all of our shit into a nice neat pile (and there was a lot so you can be sure it's a big ball) and then began the process of HEAVING this big 'ole ball of shit alllll the way back up to the endpoint's entry.  Here it was caught by the catch block of the outer try/catch block.  This experience, from the catch block's point of view, was not unlike that of a mack truck slamming into a deer at 60+ mph.  The deer, for its part, did not move.  It took that bitch right in the face. God bless you Ron.  We couldn't find any piece of you left, but we will love you forever.  You're in our hearts.  After pulverizing the deer, the mack truck unloaded the big pile of shit we made on the second story of that Hotel California and then - bless its heart - packaged up the shit yet again into a different ball.  It sent this pile of shit, by way of email, to the developers who, we hope, still give a shit enough to fix whatever caused the shitting in the first place.  The client, however, saw none of this.  They got a message apologizing that something went wrong.  They. will. never. understand what work. What PAINFUL work went into keeping the whole world from collpasing because we shat in the bed of their request. In the end, fuck you, fuck your bald spot, fuck your glasses, and McLean, fuck your beer belly. Btw, if you're reading this and you're thinking "well this should be removed" you're wrong.  And if you think you're right then Doctor's orders - go fuck yourself. In fact, if you read this (even if you have read it before), you are now responsible for adding at least one sentence to further the story (before this sentence).
        /// </summary>
        FatalError
    }

        

    /// <summary>
    /// Describes a message token, including its Access Methods.  Intended to be used to track an interaction with the client.
    /// </summary>
    public class MessageToken
    {

        #region Properties

        /// <summary>
        /// The unique Id assigned to this message interaction
        /// </summary>
        public virtual Guid Id { get; protected set; }

        /// <summary>
        /// The APIKey that the client used to access the API
        /// </summary>
        public virtual APIKey APIKey { get; set; }

        /// <summary>
        /// The time at which the client called the API.
        /// </summary>
        public virtual DateTime CallTime { get; protected set; }

        /// <summary>
        /// The Arguments the client sent to the API.  This is the RawJSON transformed into a dictionary.  This is not mapped to the database.
        /// </summary>
        public virtual Dictionary<string, object> Args { get; set; }

        /// <summary>
        /// The body of the request prior to processing.
        /// </summary>
        public virtual string RawRequestBody { get; protected set; }

        /// <summary>
        /// The endpoint that was called by the client.
        /// </summary>
        public virtual string CalledEndpoint { get; set; }

        /// <summary>
        /// The endpoint that was invoked by the client.
        /// </summary>
        public virtual EndpointDescription Endpoint { get; set; }

        /// <summary>
        /// Any error messages that occur during the request are pushed to this property.
        /// </summary>
        public virtual IList<string> ErrorMessages { get; protected set; }

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
        public virtual ErrorTypes ErrorType { get; protected set; }

        /// <summary>
        /// The status code that describes the message token's state.
        /// </summary>
        public virtual System.Net.HttpStatusCode StatusCode { get; protected set; }

        /// <summary>
        /// The resultant object.
        /// </summary>
        public virtual object Result { get; protected set; }

        /// <summary>
        /// Gets raw response body of this request.  This is simple a property accessor to this.ConstructResponse() for logging purposes.
        /// </summary>
        public virtual string RawResponseBody
        {
            get
            {
                return ConstructResponse();
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
        /// The session that should be used throughout the lifetime of the request to interact with the database.
        /// </summary>
        public virtual ISession CommunicationSession { get; set; }

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
            return "{0} | {1} | {2}\n\t\tCall Time: {3}\n\t\tProcessing Time: {4}\n\t\tHost: {5}\n\t\tApp Name: {6}\n\t\tSession ID: {7}\n\t\tError Code: {8}\n\t\t Status Code: {9}\n\t\tMessages: {10}"
                .FormatS(Id, 
                Endpoint == null ? CalledEndpoint : Endpoint.Name, 
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
                    Args = RawRequestBody.Deserialize<Dictionary<string, object>>();
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
                throw new Exception("You can not set a error message on a message token that has already been assigned a return value.  You get one or the other.");

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
        public virtual string ConstructResponse()
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

        #endregion

        public class MessageTokenMapping : ClassMap<MessageToken>
        {
            public MessageTokenMapping()
            {
                Table("message_tokens");

                Id(x => x.Id).GeneratedBy.Assigned();

                References(x => x.APIKey);
                References(x => x.AuthenticationSession);

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

                Component(x => x.Endpoint, endpoint =>
                    {
                        endpoint.Map(x => x.Name).Nullable().Column("ResolvedEndpointName");
                    });

                HasMany(x => x.ErrorMessages)
                    .Table("message_token_error_messages")
                    .KeyColumn("MessageTokenId")
                    .Element("ErrorMessage");

            }
        }

    }
}
