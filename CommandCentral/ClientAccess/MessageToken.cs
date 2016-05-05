using System;
using System.Collections.Generic;
using FluentNHibernate.Mapping;
using NHibernate;
using AtwoodUtils;

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
        public virtual Guid Id { get; set; }

        /// <summary>
        /// The APIKey that the client used to access the API
        /// </summary>
        public virtual ApiKey ApiKey { get; set; }

        /// <summary>
        /// The time at which the client called the API.
        /// </summary>
        public virtual DateTime CallTime { get; set; }

        /// <summary>
        /// The Arguments the client sent to the API.  This is the RawJSON transformed into a dictionary.  This is not mapped to the database.
        /// </summary>
        public virtual Dictionary<string, object> Args { get; set; }

        private virtual string _rawJSON = null;
        /// <summary>
        /// The raw JSON as it was received from the client, prior to any processing.  This value should not be used for any processing as the value is truncated to no more than 9000 characters.
        /// </summary>
        public virtual string RawJSON
        {
            get
            {
                return _rawJSON;
            }
            set
            {
                _rawJSON = value.Truncate(9000);
            }
        }

        /// <summary>
        /// The endpoint that was invoked by the client.
        /// </summary>
        public virtual string Endpoint { get; set; }

        /// <summary>
        /// Stores the result from the service.  This is not mapped to the database.  Its representation, ResultJSON is stored.
        /// <para />
        /// </summary>
        public virtual object Result { get; set; }

        private virtual string _resultJSON = null;
        /// <summary>
        /// Represents the Result object as json.  It is equivelent to Result.Serialize(). This value should not be used for any processing as the value is truncated to no more than 9000 characters.
        /// </summary>
        public virtual string ResultJSON
        {
            get
            {
                return _resultJSON;
            }
            set
            {
                _resultJSON = value.Truncate(9000);
            }
        }

        /// <summary>
        /// The current state of the message interaction.
        /// </summary>
        public virtual MessageStates State { get; set; }

        /// <summary>
        /// The time at which the message was handled.
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

        #region Helper Methods

        /// <summary>
        /// Attempts to obtain an argument from the underlying args dictionary and throws an exception with the given error if it can't find it.
        /// </summary>
        /// <param name="argName"></param>
        /// <param name="errorMessage"></param>
        /// <returns></returns>
        public virtual object GetArgOrFail(string argName, string errorMessage)
        {
            if (!Args.ContainsKey(argName))
                throw new ServiceException(errorMessage, ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);

            return Args[argName];
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
                Table("message_tokens");

                Id(x => x.Id);

                References(x => x.AuthenticationSession).Nullable().Cascade.All();
                References(x => x.ApiKey).Nullable();

                Map(x => x.CallTime).Not.Nullable();
                Map(x => x.Endpoint).Not.Nullable().Length(40);
                Map(x => x.State).Not.Nullable();
                Map(x => x.HandledTime).Nullable();
                Map(x => x.HostAddress).Nullable().Length(30);
                Map(x => x.RawJSON).Not.Nullable().Length(10000);
                Map(x => x.ResultJSON).Not.Nullable().Length(10000);

                Cache.ReadWrite();

            }
        }



    }
}
