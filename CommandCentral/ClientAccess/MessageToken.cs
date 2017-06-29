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

        /// <summary>
        /// The endpoint that was called by the client.
        /// </summary>
        public virtual string CalledEndpoint { get; set; }

        /// <summary>
        /// The endpoint that was invoked by the client.
        /// </summary>
        public virtual ServiceEndpoint EndpointDescription { get; set; }

        /// <summary>
        /// The resultant object, set by SetResult.
        /// </summary>
        public virtual Newtonsoft.Json.Linq.JToken Result { get; set; }

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
            return "{0} | {1} | \n\t\tCall Time: {2}\n\t\tProcessing Time: {3}\n\t\tHost: {4}\n\t\tApp Name: {5}\n\t\tSession ID: {6}"
                .With(Id,
                CalledEndpoint,
                CallTime.ToString(CultureInfo.InvariantCulture),
                DateTime.UtcNow.Subtract(CallTime).ToString(),
                HostAddress,
                APIKey == null ? "null" : APIKey.ApplicationName,
                AuthenticationSession == null ? "null" : AuthenticationSession.Id.ToString());
        }

        #endregion

        #region ctor

        /// <summary>
        /// Creates a new message token and creates a new Id for it and sets the call time to DateTime.UtcNow while setting the error messages to a blank message.
        /// </summary>
        public MessageToken()
        {
            Id = Guid.NewGuid();
            CallTime = DateTime.UtcNow;
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Sets the args of the message token.
        /// </summary>
        /// <param name="body" />
        public virtual void SetArgs(string body)
        {
            try
            {
                var dict = body.Deserialize<Dictionary<string, object>>();
                Args = new Dictionary<string, object>(dict, StringComparer.OrdinalIgnoreCase);
            }
            catch
            {
                throw new CommandCentralException("There was an error while attempting to parse your request body.  " +
                    "This request body should be JSON in the form of a dictionary.", ErrorTypes.Validation);
            }
        }

        /// <summary>
        /// Sets the result for this message token.  An exception will be thrown if you attempt to set the result on a message that has errors.
        /// <para/>
        /// Additionally, if lazy loading is required, the message token must be set within its associated session.
        /// </summary>
        /// <param name="result"></param>
        public virtual void SetResult(object result)
        {
            Result = Newtonsoft.Json.Linq.JToken.FromObject(result, Newtonsoft.Json.JsonSerializer.Create(SerializationSettings.StandardSettings));
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

                References(x => x.APIKey).Nullable();
                References(x => x.AuthenticationSession).Nullable();

                Map(x => x.CallTime);
                Map(x => x.CalledEndpoint);
                Map(x => x.HandledTime);
                Map(x => x.HostAddress);
            }
        }
    }

    /// <summary>
    /// Contains extension method meant to make dealing with message tokens a little easier.
    /// </summary>
    public static class MessageTokenExtensions
    {
        /// <summary>
        /// Throws a command central bad request exception if not all of the keys are contained in the dictionary.
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="dict"></param>
        /// <param name="keys"></param>
        public static void AssertContainsKeys<TKey, TValue>(this IDictionary<TKey, TValue> dict, params TKey[] keys)
        {
            foreach (var key in keys)
            {
                if (!dict.ContainsKey(key))
                    throw new CommandCentralException("You must send all of the following parameters: {0}".With(String.Join(", ", keys)), ErrorTypes.Validation);
            }
        }

        /// <summary>
        /// Throws an error if the token doesn't have a valid login session.
        /// </summary>
        /// <param name="token"></param>
        public static void AssertLoggedIn(this MessageToken token)
        {
            if (token.AuthenticationSession == null)
                throw new CommandCentralException("You must be logged in.", ErrorTypes.Authentication);
        }
    }
}
