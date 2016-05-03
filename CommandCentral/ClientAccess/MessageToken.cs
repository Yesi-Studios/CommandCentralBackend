using System;
using System.Collections.Generic;
using FluentNHibernate.Mapping;
using NHibernate;

namespace CommandCentral.ClientAccess
{
    /// <summary>
    /// An enum describing the possible states a message can be in.
    /// </summary>
    public enum MessageStates
    {
        /// <summary>
        /// Indicates that the message is currently being handled.
        /// </summary>
        Active,
        /// <summary>
        /// Indicates that the message has been handled.
        /// </summary>
        Handled,
        /// <summary>
        /// Indicates the message failed for some reason.
        /// </summary>
        Failed
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
        /// The Arguments the client sent to the API.
        /// </summary>
        public virtual Dictionary<string, object> Args { get; set; }

        /// <summary>
        /// The endpoint that was invoked by the client.
        /// </summary>
        public virtual string Endpoint { get; set; }

        /// <summary>
        /// Stores the result from the service.  This is not mapped to the database yet.
        /// <para />
        /// TODO: map this to the database.
        /// </summary>
        public virtual object Result { get; set; }

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
                throw new ServiceException(errorMessage, ErrorTypes.Validation, HttpStatusCodes.BadRequest);

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

                References(x => x.AuthenticationSession).Nullable();
                References(x => x.ApiKey).Not.Nullable();

                Map(x => x.CallTime).Not.Nullable();
                //Map(x => x.Args).CustomType<NHibernate.Type.SerializableType>().Not.Nullable();
                Map(x => x.Endpoint).Not.Nullable().Length(40);
                Map(x => x.State).Not.Nullable();
                Map(x => x.HandledTime).Nullable();

                Cache.ReadWrite();

            }
        }



    }
}
