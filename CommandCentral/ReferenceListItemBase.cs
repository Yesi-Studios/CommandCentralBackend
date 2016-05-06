using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using CommandCentral.ClientAccess;

namespace CommandCentral
{
    /// <summary>
    /// Provides abstracted access to a reference list such as Ranks or Rates.
    /// </summary>
    public abstract class ReferenceListItemBase
    {
        /// <summary>
        /// The Id of this reference item.
        /// </summary>
        public virtual Guid Id { get; set; }

        /// <summary>
        /// The value of this item.
        /// </summary>
        public virtual string Value { get; set; }

        /// <summary>
        /// A description of this item.
        /// </summary>
        public virtual string Description { get; set; }

        /// <summary>
        /// Returns the Value.
        /// </summary>
        /// <returns></returns>
        public new virtual string ToString()
        {
            return Value;
        }

        #region Client Access Methods

        /// <summary>
        /// WARNING!  THIS IS A CLIENT METHOD.  AUTHENTICATION, AUTHORIZATION AND VALIDATION MUST BE HANDLED PRIOR TO DB INTERACTION.
        /// <para />
        /// Returns all reference lists to the client.  Reference lists are ordered by their type.
        /// <para />
        /// Options: 
        /// <para />
        /// None
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        private static MessageToken LoadReferenceLists_Client(MessageToken token)
        {
            //Very easily we're just going to throw back all the lists.  Easy day.  We're going to group the lists by name so that it looks nice for the client.
            token.Result = token.CommunicationSession.QueryOver<ReferenceListItemBase>().List<ReferenceListItemBase>().GroupBy(x => x.GetType().Name).Select(x =>
                {
                    return new KeyValuePair<string, List<ReferenceListItemBase>>(x.Key, x.ToList());
                }).ToDictionary(x => x.Key, x => x.Value);

            return token;
        }


        /// <summary>
        /// The exposed endpoints
        /// </summary>
        public static Dictionary<string, EndpointDescription> EndpointDescriptions
        {
            get
            {
                return new Dictionary<string, EndpointDescription>
                {
                    { "LoadReferenceLists", new EndpointDescription
                        {
                            AllowArgumentLogging = true,
                            AllowResponseLogging = true,
                            AuthorizationNote = "None",
                            DataMethod = LoadReferenceLists_Client,
                            Description = "Returns all reference lists to the client.  Reference lists are ordered by their type.",
                            ExampleOutput = () => "TODO",
                            IsActive = true,
                            OptionalParameters = null,
                            RequiredParameters = new List<string>
                            {
                                "apikey - The unique GUID token assigned to your application for metrics purposes."
                            },
                            RequiredSpecialPermissions = null,
                            RequiresAuthentication = false
                        }
                    }
                };
            }
        }

        #endregion

    }
}
