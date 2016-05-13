using CommandCentral.ClientAccess;
using System.Linq;
using FluentNHibernate.Mapping;
using System;
using System.Collections.Generic;

namespace CommandCentral.Entities.ReferenceLists
{
    /// <summary>
    /// Describes a single command, such as NIOC GA and all of its departments and divisions.
    /// </summary>
    public class Command
    {
        #region Properties

        /// <summary>
        /// The Command's unique ID
        /// </summary>
        public virtual Guid Id { get; set; }

        /// <summary>
        /// The value of this command.  Eg. NIOC GA
        /// </summary>
        public virtual string Value { get; set; }

        /// <summary>
        /// A short description of this command.
        /// </summary>
        public virtual string Description { get; set; }

        /// <summary>
        /// The departments of the command
        /// </summary>
        public virtual IList<Department> Departments { get; set; }

        #endregion

        #region Overrides

        /// <summary>
        /// Returns the value (name) of this command.
        /// </summary>
        /// <returns></returns>
        public new virtual string ToString()
        {
            return Value;
        }

        #endregion

        #region Client Access

        /// <summary>
        /// WARNING!  THIS IS A CLIENT METHOD.  AUTHENTICATION, AUTHORIZATION AND VALIDATION MUST BE HANDLED PRIOR TO DB INTERACTION.
        /// <para />
        /// Returns all commands, group by the command name.
        /// <para />
        /// Options: 
        /// <para />
        /// None
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        private static void LoadCommands_Client(MessageToken token)
        {
            //Very easily we're just going to throw back all the lists.  Easy day.  We're going to group the lists by name so that it looks nice for the client.
            token.SetResult(token.CommunicationSession.QueryOver<Command>().List<Command>().GroupBy(x => x.Value).Select(x =>
            {
                return new KeyValuePair<string, List<Command>>(x.Key, x.ToList());
            }).ToDictionary(x => x.Key, x => x.Value));
        }

        /// <summary>
        /// The exposed endpoints
        /// </summary>
        public static List<EndpointDescription> EndpointDescriptions
        {
            get
            {
                return new List<EndpointDescription>
                {
                    new EndpointDescription
                    {
                        Name = "LoadCommands",
                        AllowArgumentLogging = true,
                        AllowResponseLogging = true,
                        AuthorizationNote = "None",
                        DataMethod = LoadCommands_Client,
                        Description = "Returns all commands, group by the command name.",
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
                };
            }
        }
        

        #endregion

        /// <summary>
        /// Maps a command to the database.
        /// </summary>
        public class CommandMapping : ClassMap<Command>
        {
            /// <summary>
            /// Maps a command to the database.
            /// </summary>
            public CommandMapping()
            {
                Id(x => x.Id).GeneratedBy.Guid();

                Map(x => x.Value).Not.Nullable().Unique().Length(20);
                Map(x => x.Description).Nullable().Length(50);

                HasMany(x => x.Departments).Cascade.All();

                Cache.ReadWrite();
            }
        }
    }
}
