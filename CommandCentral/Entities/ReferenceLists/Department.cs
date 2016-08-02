using System;
using System.Collections.Generic;
using System.Linq;
using AtwoodUtils;
using CommandCentral.ClientAccess;
using FluentNHibernate.Mapping;
using FluentValidation;
using NHibernate.Criterion;

namespace CommandCentral.Entities.ReferenceLists
{
    /// <summary>
    /// Describes a single Department and all of its divisions.
    /// </summary>
    public class Department : IValidatable
    {
        #region Properties

        /// <summary>
        /// The Department's unique ID
        /// </summary>
        public virtual Guid Id { get; set; }

        /// <summary>
        /// The value of this department.  Eg. C40
        /// </summary>
        public virtual string Value { get; set; }

        /// <summary>
        /// A short description of this department.
        /// </summary>
        public virtual string Description { get; set; }

        /// <summary>
        /// The command to which this department belongs.
        /// </summary>
        public virtual Command Command { get; set; }

        /// <summary>
        /// A list of those divisions that belong to this department.
        /// </summary>
        public virtual IList<Division> Divisions { get; set; }

        #endregion

        #region Overrides

        /// <summary>
        /// Returns the value (name) of this department.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return Value;
        }

        /// <summary>
        /// Compares a fucking department to another department.  What else?
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {

            if (!(obj is Department))
                return false;

            var other = (Department)obj;
            if (other == null)
                return false;

            return this.Id == other.Id && this.Value == other.Value && this.Description == other.Description;
        }

        /// <summary>
        /// Gets the hash code. Ignores dependencies. This kills the hash function.
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
        /// Validates this department object.
        /// </summary>
        /// <returns></returns>
        public virtual FluentValidation.Results.ValidationResult Validate()
        {
            return new DepartmentValidator().Validate(this);
        }

        #endregion

        #region Client Access

        /// <summary>
        /// WARNING!  THIS METHOD IS EXPOSED TO THE CLIENT AND IS NOT INTENDED FOR INTERNAL USE.  AUTHENTICATION, AUTHORIZATION AND VALIDATION MUST BE HANDLED PRIOR TO DB INTERACTION.
        /// </summary>
        /// Loads a single department given the department's Id.
        /// <param name="token"></param>
        /// <returns></returns>
        [EndpointMethod(EndpointName = "LoadDepartment", AllowArgumentLogging = true, AllowResponseLogging = true, RequiresAuthentication = false)]
        private static void EndpointMethod_LoadDepartment(MessageToken token)
        {
            if (!token.Args.ContainsKey("departmentid"))
            {
                token.AddErrorMessage("You failed to send a 'departmentid' parameter.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                return;
            }

            Guid departmentId;
            if (!Guid.TryParse(token.Args["departmentid"] as string, out departmentId))
            {
                token.AddErrorMessage("Your 'departmentid' parameter was not in a valid format.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                return;
            }

            using (var session = DataAccess.NHibernateHelper.CreateStatefulSession())
            {
                token.SetResult(session.Get<Department>(departmentId));
            }
        }

        /// <summary>
        /// WARNING!  THIS METHOD IS EXPOSED TO THE CLIENT AND IS NOT INTENDED FOR INTERNAL USE.  AUTHENTICATION, AUTHORIZATION AND VALIDATION MUST BE HANDLED PRIOR TO DB INTERACTION.
        /// </summary>
        /// Loads all departments and their corresponding divisions from the database.
        /// <param name="token"></param>
        /// <returns></returns>
        [EndpointMethod(EndpointName = "LoadDepartments", AllowArgumentLogging = true, AllowResponseLogging = true, RequiresAuthentication = false)]
        private static void EndpointMethod_LoadDepartments(MessageToken token)
        {
            using (var session = DataAccess.NHibernateHelper.CreateStatefulSession())
            {
                //Very easily we're just going to throw back all the departments.
                token.SetResult(session.QueryOver<Department>().List<Department>());
            }
        }

        /// <summary>
        /// WARNING!  THIS METHOD IS EXPOSED TO THE CLIENT AND IS NOT INTENDED FOR INTERNAL USE.  AUTHENTICATION, AUTHORIZATION AND VALIDATION MUST BE HANDLED PRIOR TO DB INTERACTION.
        /// </summary>
        /// Loads all departments for a single command and their corresponding divisions from the database.
        /// <param name="token"></param>
        /// <returns></returns>
        [EndpointMethod(EndpointName = "LoadDepartmentsByCommand", AllowArgumentLogging = true, AllowResponseLogging = true, RequiresAuthentication = false)]
        private static void EndpointMethod_LoadDepartmentsByCommand(MessageToken token)
        {

            if (!token.Args.ContainsKey("commandid"))
            {
                token.AddErrorMessage("You failed to send a 'commandid' parameter.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                return;
            }

            Guid commandId;
            if (!Guid.TryParse(token.Args["commandid"] as string, out commandId))
            {
                token.AddErrorMessage("The 'commandid' parameter that you sent was in the wrong format.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                return;
            }

            //We don't need to validate that this command is valid.  Just load all departments by this command.  If we get none because the Id is bad, sucks to suck.
            using (var session = DataAccess.NHibernateHelper.CreateStatefulSession())
            {
                //Very easily we're just going to throw back all the departments.
                token.SetResult(session.QueryOver<Department>().Where(x => x.Command.Id == commandId).List<Department>());
            }
        }

        /// <summary>
        /// WARNING!  THIS METHOD IS EXPOSED TO THE CLIENT AND IS NOT INTENDED FOR INTERNAL USE.  AUTHENTICATION, AUTHORIZATION AND VALIDATION MUST BE HANDLED PRIOR TO DB INTERACTION.
        /// </summary>
        /// Adds a department to the database in a given command.
        /// <param name="token"></param>
        /// <returns></returns>
        [EndpointMethod(EndpointName = "AddDepartment", AllowArgumentLogging = true, AllowResponseLogging = true, RequiresAuthentication = true)]
        private static void EndpointMethod_AddDepartment(MessageToken token)
        {
            //First we need to know if the client is logged in and is a client.
            if (token.AuthenticationSession == null)
            {
                token.AddErrorMessage("You must be logged in to add a department.", ErrorTypes.Authentication, System.Net.HttpStatusCode.Forbidden);
                return;
            }

            if (!token.AuthenticationSession.Person.HasSpecialPermissions(Authorization.SpecialPermissions.Developer))
            {
                token.AddErrorMessage("Only developers may add departments.", ErrorTypes.Authorization, System.Net.HttpStatusCode.Unauthorized);
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
                    Department department = new Department { Value = value, Description = description, Divisions = new List<Division>() };

                    //Validate it.
                    var validationResult = department.Validate();

                    if (validationResult.Errors.Any())
                    {
                        token.AddErrorMessages(validationResult.Errors.Select(x => x.ErrorMessage), ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                        return;
                    }

                    //Ok it passed basic validation.  Now let's also get the command.
                    if (!token.Args.ContainsKey("commandid"))
                    {
                        token.AddErrorMessage("You failed to send a 'commandid' parameter.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                        return;
                    }

                    Guid commandId;
                    if (!Guid.TryParse(token.Args["commandid"] as string, out commandId))
                    {
                        token.AddErrorMessage("The 'commandid' parameter that you sent was in the wrong format.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                        return;
                    }

                    //Ok, let's try to load this command.
                    var command = session.Get<Command>(commandId);
                    if (command == null)
                    {
                        token.AddErrorMessage("The command Id you sent did not belong to a real command.  Sad face.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                        return;
                    }

                    //Well, conveniently, we now have the command here with all its departments.  Let's see if the value is a duplicate.
                    if (command.Departments.Any(x => x.Value.SafeEquals(department.Value)))
                    {
                        token.AddErrorMessage("The command, '{0}', already has a department named, '{1}'.".FormatS(command.Value, department.Value), ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                        return;
                    }

                    //Ok so this is all ok, let's tell the department how owns it and then add it.
                    department.Command = command;

                    //Omg, I think we're ready to actually save the department.
                    session.Save(department);

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
        /// Edits a given department's value and description. for a given department Id.
        /// <param name="token"></param>
        /// <returns></returns>
        [EndpointMethod(EndpointName = "EditDepartment", AllowArgumentLogging = true, AllowResponseLogging = true, RequiresAuthentication = true)]
        private static void EndpointMethod_EditDepartment(MessageToken token)
        {
            //First we need to know if the client is logged in and is a client.
            if (token.AuthenticationSession == null)
            {
                token.AddErrorMessage("You must be logged in to edit a department.", ErrorTypes.Authentication, System.Net.HttpStatusCode.Forbidden);
                return;
            }

            if (!token.AuthenticationSession.Person.HasSpecialPermissions(Authorization.SpecialPermissions.Developer))
            {
                token.AddErrorMessage("Only developers may edit departments.", ErrorTypes.Authorization, System.Net.HttpStatusCode.Unauthorized);
                return;
            }

            //Now we need the params from the client.  First up is the Id.
            if (!token.Args.ContainsKey("departmentid"))
            {
                token.AddErrorMessage("You didn't send a 'departmentid' parameter.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                return;
            }

            Guid departmentId;
            if (!Guid.TryParse(token.Args["departmentid"] as string, out departmentId))
            {
                token.AddErrorMessage("The department Id you provided was in the wrong format.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                return;
            }

            //Let's load the department and make sure it's real.
            using (var session = DataAccess.NHibernateHelper.CreateStatefulSession())
            using (var transaction = session.BeginTransaction())
            {
                try
                {
                    var department = session.Get<Department>(departmentId);

                    if (department == null)
                    {
                        token.AddErrorMessage("The department id that you provided did not resolve to a real department.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                        return;
                    }

                    //Ok, so we have the list.  Now let's put the client's values in and ask if they're valid.
                    if (token.Args.ContainsKey("value"))
                        department.Value = token.Args["value"] as string;
                    if (token.Args.ContainsKey("description"))
                        department.Description = token.Args["description"] as string;

                    //Validation
                    var result = department.Validate();

                    if (!result.IsValid)
                    {
                        token.AddErrorMessages(result.Errors.Select(x => x.ErrorMessage), ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                        return;
                    }

                    //Also make sure no department has this value.
                    if (session.CreateCriteria<Department>().Add(Expression.Like("Value", department.Value)).List<Department>().Any(x => x.Id != department.Id))
                    {
                        token.AddErrorMessage("A department with that value already exists.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                        return;
                    }

                    //Ok that's all good.  Let's update the department.
                    session.Update(department);

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
                Id(x => x.Id).GeneratedBy.Guid();

                Map(x => x.Value).Not.Nullable().Unique().Length(20);
                Map(x => x.Description).Nullable().Length(50);

                HasMany(x => x.Divisions).Cascade.All().Not.LazyLoad();

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
                RuleFor(x => x.Description).Length(0, 40)
                    .WithMessage("The description of a Department must be no more than 40 characters.");
                RuleFor(x => x.Value).Length(1, 15)
                    .WithMessage("The value of a Department must be between one and ten characters.");
            }
        }

    }
}
