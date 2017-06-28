using System;
using System.Collections.Generic;
using System.Linq;
using AtwoodUtils;
using CommandCentral.Authorization;
using CommandCentral.ClientAccess;
using FluentNHibernate.Mapping;
using FluentValidation;
using NHibernate.Criterion;

namespace CommandCentral.Entities.ReferenceLists
{
    /// <summary>
    /// Describes a single Department and all of its divisions.
    /// </summary>
    public class Department : EditableReferenceListItemBase
    {
        #region Properties
        
        /// The command to which this department belongs.
        /// </summary>
        public virtual Command Command { get; set; }

        /// <summary>
        /// A list of those divisions that belong to this department.
        /// </summary>
        public virtual IList<Division> Divisions { get; set; }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Validates this department object.
        /// </summary>
        /// <returns></returns>
        public override FluentValidation.Results.ValidationResult Validate()
        {
            return new DepartmentValidator().Validate(this);
        }

        /// <summary>
        /// Delete the department.
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
                    //First try to get the department.
                    var department = session.Get<Department>(id) ??
                        throw new CommandCentralException("That department Id was not valid.", ErrorTypes.Validation);
                    
                    //Ok, now find all the entities it's a part of.
                    var persons = session.QueryOver<Person>().Where(x => x.Department == department).List();

                    if (persons.Any())
                    {
                        //There are references to deal with.
                        if (forceDelete)
                        {
                            //The client is telling us to force delete the department.  Now we need to clean up everything.
                            foreach (var person in persons)
                            {
                                person.Department = null;
                                person.Division = null;

                                session.Save(person);
                            }

                            //Now that everything is cleaned up, drop the department.
                            session.Delete(department);
                        }
                        else
                        {
                            //There were references but we can't delete them.
                            throw new CommandCentralException("We were unable to delete the department, {0}, because it is referenced on {1} profile(s).".FormatS(department, persons.Count), ErrorTypes.Validation);
                        }
                    }
                    else
                    {
                        //There are no references, let's drop the department.
                        session.Delete(department);
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
        /// Load the departments.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="token"></param>
        public override List<ReferenceListItemBase> Load(Guid id, MessageToken token)
        {
            using (var session = DataAccess.NHibernateHelper.CreateStatefulSession())
            using (var transaction = session.BeginTransaction())
            {
                try
                {
                    if (id != default(Guid))
                    {
                        return new[] { (ReferenceListItemBase)session.Get<Department>(id) }.ToList();
                    }
                    else
                    {
                        List<ReferenceListItemBase> results;

                        //The id is blank... but were we given a command Id?
                        if (token.Args.ContainsKey("commandid"))
                        {
                            //Yes we were!
                            if (!Guid.TryParse(token.Args["commandid"] as string, out Guid commandId))
                                throw new CommandCentralException("The command id was not valid.", ErrorTypes.Validation);

                            //Cool, give them back the departments in this command.
                            results = session.QueryOver<Department>()
                                .Where(x => x.Command.Id == commandId)
                                .Cacheable().CacheMode(NHibernate.CacheMode.Normal)
                                .List<ReferenceListItemBase>()
                                .ToList();
                        }
                        else
                        {
                            //Nope, just give them all the departments.
                            results = session.QueryOver<Department>()
                                .Cacheable().CacheMode(NHibernate.CacheMode.Normal)
                                .List<ReferenceListItemBase>().ToList();
                        }

                        transaction.Commit();

                        return results;
                    }
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
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
                    //First try to get the command this thing is supposed to be a part of.
                    if (!Guid.TryParse(item.Value<string>("commandid"), out Guid commandId))
                        throw new CommandCentralException("The command id was not valid.", ErrorTypes.Validation);

                    var commandFromClient = session.Get<Command>(commandId) ??
                        throw new CommandCentralException("The command id was not valid.", ErrorTypes.Validation);

                    //Cool the command is legit.  Let's build the department.
                    var departmentFromClient = item.CastJToken<Department>();
                    departmentFromClient.Command = commandFromClient;

                    //Now validate it.
                    var result = departmentFromClient.Validate();
                    if (!result.IsValid)
                        throw new AggregateException(result.Errors.Select(x => new CommandCentralException(x.ErrorMessage, ErrorTypes.Validation)));

                    //Try to get it.
                    var departmentFromDB = session.Get<Department>(departmentFromClient.Id);

                    //If it's null then add it.
                    if (departmentFromDB == null)
                    {
                        departmentFromClient.Id = Guid.NewGuid();
                        session.Save(departmentFromClient);
                    }
                    else
                    {
                        //If the client is updating the command, then we need to walk across all the persons and update that.
                        if (departmentFromDB.Command.Id != departmentFromClient.Command.Id)
                        {
                            var persons = session.QueryOver<Person>()
                                .Where(x => x.Command == departmentFromDB.Command)
                                .List();

                            foreach (var person in persons)
                            {
                                person.Command = departmentFromClient.Command;
                                session.Update(person);
                            }
                        }

                        //If it's not null, then merge it.
                        departmentFromDB.Value = departmentFromClient.Value;
                        departmentFromDB.Description = departmentFromClient.Description;
                        departmentFromDB.Command = departmentFromClient.Command;
                        session.Update(departmentFromDB);
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

        #endregion

        /// <summary>
        /// Maps a department to the database.
        /// </summary>
        public class DepartmentMapping : ClassMap<Department>
        {
            /// <summary>
            /// Maps a department to the database.
            /// </summary>
            public DepartmentMapping()
            {
                Id(x => x.Id).GeneratedBy.Assigned();

                Map(x => x.Value).Not.Nullable().Unique();
                Map(x => x.Description);

                HasMany(x => x.Divisions).Not.LazyLoad().Cascade.DeleteOrphan();

                References(x => x.Command).LazyLoad(Laziness.False);

                Cache.ReadWrite();
            }
        }

        /// <summary>
        /// Validates the Department.
        /// </summary>
        public class DepartmentValidator : AbstractValidator<Department>
        {
            /// <summary>
            /// Validates the Department.
            /// </summary>
            public DepartmentValidator()
            {
                RuleFor(x => x.Description).Length(0, 255)
                    .WithMessage("The description of a department must be no more than 255 characters.");
                RuleFor(x => x.Value).NotEmpty()
                    .WithMessage("The value must not be empty.");
            }
        }
    }
}
