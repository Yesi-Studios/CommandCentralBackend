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
        public override string ToString()
        {
            return Value;
        }

        /// <summary>
        /// Determines if an object is equal to this object.  wtf do you think equals does?
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (!(obj is Command))
                return false;

            var other = (Command)obj;
            if (other == null)
                return false;

            return this.Id == other.Id && this.Value == other.Value && this.Description == other.Description;
        }

        /// <summary>
        /// Gets the hashcode and ignores any dependencies.
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;

                hash = hash * 23 + Id.GetHashCode();
                hash = hash * 23 + (String.IsNullOrEmpty(Value) ? "".GetHashCode() : Value.GetHashCode());
                hash = hash * 23 + (String.IsNullOrEmpty(Description) ? "".GetHashCode() : Description.GetHashCode());

                return hash;
            }
        }

        #endregion

        #region Client Access

        /// <summary>
        /// WARNING!  THIS METHOD IS EXPOSED TO THE CLIENT AND IS NOT INTENDED FOR INTERNAL USE.  AUTHENTICATION, AUTHORIZATION AND VALIDATION MUST BE HANDLED PRIOR TO DB INTERACTION.
        /// </summary>
        /// Loads all commands and their corresponding departments/divisions from the database.cache.
        /// <param name="token"></param>
        /// <returns></returns>
        [EndpointMethod(EndpointName = "LoadCommands", AllowArgumentLogging = true, AllowResponseLogging =  true, RequiresAuthentication = false)]
        private static void EndpointMethod_LoadCommands(MessageToken token)
        {
            //Very easily we're just going to throw back all the lists.  Easy day.  We're going to group the lists by name so that it looks nice for the client.
            token.SetResult(token.CommunicationSession.QueryOver<Command>().List<Command>());
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
