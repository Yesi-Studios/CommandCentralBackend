using System;
using System.Collections.Generic;
using System.Linq;
using AtwoodUtils;
using CCServ.Authorization;
using CCServ.ClientAccess;
using FluentNHibernate.Mapping;
using FluentValidation;
using NHibernate.Criterion;

namespace CCServ.Entities.ReferenceLists
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

        #endregion

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
                    var department = session.Get<Department>(id);

                    if (department == null)
                    {
                        token.AddErrorMessage("That department Id was not valid.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                        return;
                    }

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
                            token.AddErrorMessage("We were unable to delete the department, {0}, because it is referenced on {1} profile(s).".FormatS(department, persons.Count), ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                            return;
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
        public override void Load(Guid id, MessageToken token)
        {
            using (var session = DataAccess.NHibernateHelper.CreateStatefulSession())
            {
                if (id != default(Guid))
                {
                    token.SetResult(session.Get<Department>(id));
                }
                else
                {
                    //The id is blank... but were we given a command Id?
                    if (token.Args.ContainsKey("commandid"))
                    {
                        //Yes we were!
                        Guid commandId;
                        if (!Guid.TryParse(token.Args["commandid"] as string, out commandId))
                        {
                            token.AddErrorMessage("The command id was not valid.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                            return;
                        }

                        //Cool, give them back the departments in this command.
                        token.SetResult(session.QueryOver<Department>().Where(x => x.Command.Id == commandId).List());
                    }
                    else
                    {
                        //Nope, just give them all the departments.
                        token.SetResult(session.QueryOver<Department>().List());
                    }
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
                    Guid commandId;
                    if (!Guid.TryParse(item.Value<string>("commandid"), out commandId))
                    {
                        token.AddErrorMessage("The command id was not valid.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                        return;
                    }

                    var command = session.Get<Command>(commandId);
                    if (command == null)
                    {
                        token.AddErrorMessage("The command id was not valid.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                        return;
                    }

                    //Cool the command is legit.  Let's build the department.
                    var department = item.CastJToken<Department>();
                    department.Command = command;

                    //Now validate it.
                    var result = department.Validate();
                    if (!result.IsValid)
                    {
                        token.AddErrorMessages(result.Errors.Select(x => x.ErrorMessage), ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                        return;
                    }

                    //Try to get it.
                    var departmentFromDB = session.Get<Department>(department.Id);

                    //If it's null then add it.
                    if (departmentFromDB == null)
                    {
                        department.Id = Guid.NewGuid();
                        session.Save(department);
                    }
                    else
                    {
                        //If it's not null, then merge it.
                        session.Merge(department);
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

                HasMany(x => x.Divisions).Cascade.All().Not.LazyLoad();

                References(x => x.Command).LazyLoad(Laziness.False);
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
