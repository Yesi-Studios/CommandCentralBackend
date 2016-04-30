using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Collections.Concurrent;
using System.Reflection;
using MySql.Data.MySqlClient;
using MySql.Data.Common;
using AtwoodUtils;
using FluentNHibernate.Mapping;
using CommandCentral.DataAccess;

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
        /// The unique ID assigned to this message interaction
        /// </summary>
        public virtual string ID { get; set; }

        /// <summary>
        /// The APIKey that the client used to access the API
        /// </summary>
        public virtual APIKey APIKey { get; set; }

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
        public virtual Session Session { get; set; }

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

                Id(x => x.ID);

                References(x => x.Session).Nullable();
                References(x => x.APIKey).Not.Nullable();

                Map(x => x.CallTime).Not.Nullable();
                Map(x => x.Args).CustomType<NHibernate.Type.SerializableType>().Not.Nullable();
                Map(x => x.Endpoint).Not.Nullable().Length(40);
                Map(x => x.State).Not.Nullable();
                Map(x => x.HandledTime).Nullable();

                Cache.ReadOnly();

            }
        }



    }
}
