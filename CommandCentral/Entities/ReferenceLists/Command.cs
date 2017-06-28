using CommandCentral.ClientAccess;
using System.Linq;
using FluentNHibernate.Mapping;
using System;
using System.Collections.Generic;
using AtwoodUtils;
using FluentValidation;
using NHibernate.Criterion;
using CommandCentral.Authorization;
using CommandCentral.Logging;
using FluentValidation.Results;

namespace CommandCentral.Entities.ReferenceLists
{
    /// <summary>
    /// Describes a single command, such as NIOC GA and all of its departments and divisions.
    /// </summary>
    public class Command : EditableReferenceListItemBase
    {
        #region Properties

        /// <summary>
        /// The departments of the command
        /// </summary>
        public virtual IList<Department> Departments { get; set; }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Validates this command object.
        /// </summary>
        /// <returns></returns>
        public override ValidationResult Validate()
        {
            return new CommandValidator().Validate(this);
        }

        /// <summary>
        /// Update or insert.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="token"></param>
        public override void UpdateOrInsert(Newtonsoft.Json.Linq.JToken item, MessageToken token)
        {
            using (var session = DataAccess.NHibernateHelper.CreateStatefulSession())
            using (var transaction = session.BeginTransaction())
            {
                try
                {
                    //We can use the DTO and command interchangeably here.
                    var commandFromClient = item.CastJToken<Command>();

                    //Now validate it.
                    var result = commandFromClient.Validate();
                    if (!result.IsValid)
                        throw new AggregateException(result.Errors.Select(x => new CommandCentralException(x.ErrorMessage, ErrorTypes.Validation)));

                    //Try to get it.
                    var commandFromDB = session.Get<Command>(commandFromClient.Id);

                    //If it's null then add it.
                    if (commandFromDB == null)
                    {
                        commandFromClient.Id = Guid.NewGuid();
                        session.Save(commandFromClient);
                    }
                    else
                    {
                        //If it's not null, then merge it.
                        commandFromDB.Value = commandFromClient.Value;
                        commandFromDB.Description = commandFromClient.Description;
                        session.Update(commandFromDB);
                    }

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
        /// Deletes the command.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="forceDelete"></param>
        /// <param name="token"></param>
        public override void Delete(Guid id, bool forceDelete, MessageToken token)
        {
            using (var session = DataAccess.NHibernateHelper.CreateStatefulSession())
            using (var transaction = session.BeginTransaction())
            {
                try
                {
                    //First try to get the command in question.
                    var command = session.Get<Command>(id) ??
                        throw new CommandCentralException("That command Id was not valid.", ErrorTypes.Validation);

                    //Ok, now find all the entities it's a part of.
                    var persons = session.QueryOver<Person>().Where(x => x.Command == command).List();

                    if (persons.Any())
                    {
                        //There are references to deal with.
                        if (forceDelete)
                        {
                            //The client is telling us to force delete the command.  Now we need to clean up everything.
                            foreach (var person in persons)
                            {
                                person.Command = null;
                                person.Department = null;
                                person.Division = null;

                                session.Save(person);
                            }

                            //Now that everything is cleaned up, drop the command.
                            session.Delete(command);
                        }
                        else
                        {
                            //There were references but we can't delete them.
                            throw new CommandCentralException("We were unable to delete the command, {0}, because it is referenced on {1} profile(s).".FormatS(command, persons.Count), ErrorTypes.Validation);
                        }
                    }
                    else
                    {
                        //There are no references, let's drop the command.
                        session.Delete(command);
                    }

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
        /// Load...
        /// </summary>
        /// <param name="id"></param>
        /// <param name="token"></param>
        public override List<ReferenceListItemBase> Load(Guid id, MessageToken token)
        {
            using (var session = DataAccess.NHibernateHelper.CreateStatefulSession())
            {
                if (id == default(Guid))
                {
                    return session.QueryOver<Command>()
                        .Cacheable().CacheMode(NHibernate.CacheMode.Normal)
                        .List<ReferenceListItemBase>().ToList();
                }
                else
                {
                    return new[] { (ReferenceListItemBase)session.Get<Command>(id) }.ToList();
                }
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
                Id(x => x.Id).GeneratedBy.Assigned();

                Map(x => x.Value).Not.Nullable().Unique();
                Map(x => x.Description);

                HasMany(x => x.Departments).Not.LazyLoad().Cascade.DeleteOrphan();

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
                RuleFor(x => x.Description).Length(0, 255)
                    .WithMessage("The description of a Command must be no more than 255 characters.");
                RuleFor(x => x.Value).NotEmpty()
                    .WithMessage("The value must not be empty.");
            }
        }

    }
}
