using CCServ.ClientAccess;
using System.Linq;
using FluentNHibernate.Mapping;
using System;
using System.Collections.Generic;
using AtwoodUtils;
using FluentValidation;
using NHibernate.Criterion;
using CCServ.Authorization;

namespace CCServ.Entities.ReferenceLists
{
    /// <summary>
    /// Describes a single command, such as NIOC GA and all of its departments and divisions.
    /// </summary>
    public class Command : IValidatable
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

        /// <summary>
        /// The old Id of this command in the old database.
        /// </summary>
        public virtual int OldId { get; set; }

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

        #region Helper Methods

        /// <summary>
        /// Validates this command object.
        /// </summary>
        /// <returns></returns>
        public virtual FluentValidation.Results.ValidationResult Validate()
        {
            return new CommandValidator().Validate(this);
        }

        #endregion

        #region Client Access

        /// <summary>
        /// WARNING!  THIS METHOD IS EXPOSED TO THE CLIENT AND IS NOT INTENDED FOR INTERNAL USE.  AUTHENTICATION, AUTHORIZATION AND VALIDATION MUST BE HANDLED PRIOR TO DB INTERACTION.
        /// </summary>
        /// Loads a single command given the command's Id.
        /// <param name="token"></param>
        /// <returns></returns>
        [EndpointMethod(EndpointName = "LoadCommand", AllowArgumentLogging = true, AllowResponseLogging = true, RequiresAuthentication = false)]
        private static void EndpointMethod_LoadCommand(MessageToken token)
        {
            if (!token.Args.ContainsKey("commandid"))
            {
                token.AddErrorMessage("You failed to send a 'commandid' parameter.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                return;
            }

            Guid commandId;
            if (!Guid.TryParse(token.Args["commandid"] as string, out commandId))
            {
                token.AddErrorMessage("Your 'commandid' parameter was not in a valid format.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                return;
            }

            using (var session = DataAccess.NHibernateHelper.CreateStatefulSession())
            {
                token.SetResult(session.Get<Command>(commandId));
            }
        }

        /// <summary>
        /// WARNING!  THIS METHOD IS EXPOSED TO THE CLIENT AND IS NOT INTENDED FOR INTERNAL USE.  AUTHENTICATION, AUTHORIZATION AND VALIDATION MUST BE HANDLED PRIOR TO DB INTERACTION.
        /// </summary>
        /// Loads all commands and their corresponding departments/divisions from the database.
        /// <param name="token"></param>
        /// <returns></returns>
        [EndpointMethod(EndpointName = "LoadCommands", AllowArgumentLogging = true, AllowResponseLogging =  true, RequiresAuthentication = false)]
        private static void EndpointMethod_LoadCommands(MessageToken token)
        {
            using (var session = DataAccess.NHibernateHelper.CreateStatefulSession())
            {
                //Very easily we're just going to throw back all the commands.
                token.SetResult(session.QueryOver<Command>().List<Command>());
            }
        }

        /// <summary>
        /// WARNING!  THIS METHOD IS EXPOSED TO THE CLIENT AND IS NOT INTENDED FOR INTERNAL USE.  AUTHENTICATION, AUTHORIZATION AND VALIDATION MUST BE HANDLED PRIOR TO DB INTERACTION.
        /// </summary>
        /// Adds a command to the database.
        /// <param name="token"></param>
        /// <returns></returns>
        [EndpointMethod(EndpointName = "AddCommand", AllowArgumentLogging = true, AllowResponseLogging = true, RequiresAuthentication = true)]
        private static void EndpointMethod_AddCommand(MessageToken token)
        {
            //First we need to know if the client is logged in and is a client.
            if (token.AuthenticationSession == null)
            {
                token.AddErrorMessage("You must be logged in to add a command.", ErrorTypes.Authentication, System.Net.HttpStatusCode.Forbidden);
                return;
            }

            if (!token.AuthenticationSession.Person.PermissionGroups.CanAccessSubmodules(SubModules.AdminTools.ToString()))
            {
                token.AddErrorMessage("Only developers may add commands.", ErrorTypes.Authorization, System.Net.HttpStatusCode.Unauthorized);
                return;
            }

            using (var session = DataAccess.NHibernateHelper.CreateStatefulSession())
            using (var transaction = session.BeginTransaction())
            {
                try
                {

                    //Okey dokey.  Now let's get the value and the description the client wants to add.
                    if (!token.Args.ContainsKey("value"))
                    {
                        token.AddErrorMessage("You didn't send a 'value' parameter.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                        return;
                    }
                    string value = token.Args["value"] as string;

                    //Now we need the description from the client.  It is optional.
                    string description = "";
                    if (token.Args.ContainsKey("description"))
                        description = token.Args["description"] as string;

                    //Now put it in the object and then validate it.
                    Command command = new Command { Value = value, Description = description, Departments = new List<Department>() };

                    //Validate it.
                    var validationResult = command.Validate();

                    if (validationResult.Errors.Any())
                    {
                        token.AddErrorMessages(validationResult.Errors.Select(x => x.ErrorMessage), ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                        return;
                    }

                    //Are we about to try to create a duplicate command?
                    if (session.CreateCriteria<Command>().Add(Expression.Like("Value", command.Value)).List<Command>().Any())
                    {
                        token.AddErrorMessage("A command with that value already exists.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                        return;
                    }

                    //Save that shit so hard.
                    session.Save(command);

                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        /// <summary>
        /// WARNING!  THIS METHOD IS EXPOSED TO THE CLIENT AND IS NOT INTENDED FOR INTERNAL USE.  AUTHENTICATION, AUTHORIZATION AND VALIDATION MUST BE HANDLED PRIOR TO DB INTERACTION.
        /// </summary>
        /// Edits a given command with the new value and description.
        /// <param name="token"></param>
        /// <returns></returns>
        [EndpointMethod(EndpointName = "EditCommand", AllowArgumentLogging = true, AllowResponseLogging = true, RequiresAuthentication = true)]
        private static void EndpointMethod_EditCommand(MessageToken token)
        {
            //First we need to know if the client is logged in and is a client.
            if (token.AuthenticationSession == null)
            {
                token.AddErrorMessage("You must be logged in to edit a command.", ErrorTypes.Authentication, System.Net.HttpStatusCode.Forbidden);
                return;
            }

            if (!token.AuthenticationSession.Person.PermissionGroups.CanAccessSubmodules(SubModules.AdminTools.ToString()))
            {
                token.AddErrorMessage("Only developers may edit commands.", ErrorTypes.Authorization, System.Net.HttpStatusCode.Unauthorized);
                return;
            }

            //Now we need the params from the client.  First up is the Id.
            if (!token.Args.ContainsKey("commandid"))
            {
                token.AddErrorMessage("You didn't send a 'commandid' parameter.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                return;
            }

            Guid commandId;
            if (!Guid.TryParse(token.Args["commandid"] as string, out commandId))
            {
                token.AddErrorMessage("The command Id you provided was in the wrong format.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                return;
            }

            //Let's load the command and make sure it's real.
            using (var session = DataAccess.NHibernateHelper.CreateStatefulSession())
            using (var transaction = session.BeginTransaction())
            {
                try
                {
                    var command = session.Get<Command>(commandId);

                    if (command == null)
                    {
                        token.AddErrorMessage("The command id that you provided did not resolve to a real command.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                        return;
                    }

                    //Ok, so we have the list.  Now let's put the client's values in and ask if they're valid.
                    if (token.Args.ContainsKey("value"))
                        command.Value = token.Args["value"] as string;
                    if (token.Args.ContainsKey("description"))
                        command.Description = token.Args["description"] as string;

                    //Validation
                    var result = command.Validate();

                    if (!result.IsValid)
                    {
                        token.AddErrorMessages(result.Errors.Select(x => x.ErrorMessage), ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                        return;
                    }

                    //Also make sure no command has this value.
                    if (session.CreateCriteria<Command>().Add(Expression.Like("Value", command.Value)).List<Command>().Any(x => x.Id != command.Id))
                    {
                        token.AddErrorMessage("A command with that value already exists.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                        return;
                    }

                    //Ok that's all good.  Let's update the command.
                    session.Update(command);

                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        /// <summary>
        /// WARNING!  THIS METHOD IS EXPOSED TO THE CLIENT AND IS NOT INTENDED FOR INTERNAL USE.  AUTHENTICATION, AUTHORIZATION AND VALIDATION MUST BE HANDLED PRIOR TO DB INTERACTION.
        /// </summary>
        /// Delete the command, or a command, you know what, fuck it.  Any command.  From the database.
        /// <param name="token"></param>
        /// <returns></returns>
        [EndpointMethod(EndpointName = "DeleteCommand", AllowArgumentLogging = true, AllowResponseLogging = true, RequiresAuthentication = true)]
        private static void EndpointMethod_DeleteCommand(MessageToken token)
        {
            //First we need to know if the client is logged in and is a client.
            if (token.AuthenticationSession == null)
            {
                token.AddErrorMessage("You must be logged in to delete a command.", ErrorTypes.Authentication, System.Net.HttpStatusCode.Forbidden);
                return;
            }

            if (!token.AuthenticationSession.Person.PermissionGroups.CanAccessSubmodules(SubModules.AdminTools.ToString()))
            {
                token.AddErrorMessage("Only developers may delete commands.", ErrorTypes.Authorization, System.Net.HttpStatusCode.Unauthorized);
                return;
            }

            //Now we need the params from the client.  First up is the Id.
            if (!token.Args.ContainsKey("commandid"))
            {
                token.AddErrorMessage("You didn't send a 'commandid' parameter. What's the matter with you?", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                return;
            }

            Guid commandId;
            if (!Guid.TryParse(token.Args["commandid"] as string, out commandId))
            {
                token.AddErrorMessage("The command Id you provided was in the wrong format.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                return;
            }

            //Now we delete it.
            using (var session = DataAccess.NHibernateHelper.CreateStatefulSession())
            using (var transaction = session.BeginTransaction())
            {
                try
                {

                    var command = session.Get<Command>(commandId);

                    if (command == null)
                    {
                        token.AddErrorMessage("The command id provided matched no list items.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                        return;
                    }

                    //Since we found a command let's get rid of it.
                    session.Delete(command);

                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        #endregion

        #region Startup Methods

        [ServiceManagement.StartMethod(Priority = 5)]
        private static void ShowCommands(CLI.Options.LaunchOptions launchOptions)
        {
            using (var session = DataAccess.NHibernateHelper.CreateStatefulSession())
            {
                var commands = session.QueryOver<Command>().List();

                Communicator.PostMessage("Found {0} command(s): {1}".FormatS(commands.Count, String.Join(",", commands.Select(x => x.Value))), Communicator.MessageTypes.Informational);
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
                Map(x => x.OldId).Not.Nullable().Unique();

                HasMany(x => x.Departments).Cascade.All().Not.LazyLoad();

                Cache.ReadWrite();
            }
        }

        /// <summary>
        /// Validates the Command.
        /// </summary>
        public class CommandValidator : AbstractValidator<Command>
        {
            /// <summary>
            /// Validates the Command.
            /// </summary>
            public CommandValidator()
            {
                RuleFor(x => x.Description).Length(0, 40)
                    .WithMessage("The description of a Command must be no more than 40 characters.");
                RuleFor(x => x.Value).Length(1, 15)
                    .WithMessage("The value of a Command must be between one and ten characters.");
            }
        }

    }
}
