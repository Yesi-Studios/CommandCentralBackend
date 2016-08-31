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
    /// Describes a single Division.
    /// </summary>
    public class Division : IValidatable
    {

        #region Properties

        /// <summary>
        /// The Division's unique ID
        /// </summary>
        public virtual Guid Id { get; set; }

        /// <summary>
        /// The value of this Division.  Eg. N75
        /// </summary>
        public virtual string Value { get; set; }

        /// <summary>
        /// A short description of this Division.
        /// </summary>
        public virtual string Description { get; set; }

        /// <summary>
        /// The department to which this division belongs.
        /// </summary>
        public virtual Department Department { get; set; }

        #endregion

        #region Overrides

        /// <summary>
        /// Returns the value (name) of this division.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return Value;
        }

        /// <summary>
        /// Compares this property to another division
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (!(obj is Division))
                return false;

            var other = (Division)obj;

            return this.Description == other.Description && this.Id == other.Id && this.Value == other.Value;
        }

        /// <summary>
        /// Hashes all but the dependency properties
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
        /// Validates this division object.
        /// </summary>
        /// <returns></returns>
        public virtual FluentValidation.Results.ValidationResult Validate()
        {
            return new DivisionValidator().Validate(this);
        }

        #endregion

        #region Client Access

        /// <summary>
        /// WARNING!  THIS METHOD IS EXPOSED TO THE CLIENT AND IS NOT INTENDED FOR INTERNAL USE.  AUTHENTICATION, AUTHORIZATION AND VALIDATION MUST BE HANDLED PRIOR TO DB INTERACTION.
        /// </summary>
        /// Loads a single division given the division's Id.
        /// <param name="token"></param>
        /// <returns></returns>
        [EndpointMethod(EndpointName = "LoadDivision", AllowArgumentLogging = true, AllowResponseLogging = true, RequiresAuthentication = false)]
        private static void EndpointMethod_LoadDivision(MessageToken token)
        {
            if (!token.Args.ContainsKey("divisionid"))
            {
                token.AddErrorMessage("You failed to send a 'divisionid' parameter.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                return;
            }

            Guid divisionId;
            if (!Guid.TryParse(token.Args["divisionid"] as string, out divisionId))
            {
                token.AddErrorMessage("Your 'divisionid' parameter was not in a valid format.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                return;
            }

            using (var session = DataAccess.NHibernateHelper.CreateStatefulSession())
            {
                token.SetResult(session.Get<Division>(divisionId));
            }
        }


        /// <summary>
        /// WARNING!  THIS METHOD IS EXPOSED TO THE CLIENT AND IS NOT INTENDED FOR INTERNAL USE.  AUTHENTICATION, AUTHORIZATION AND VALIDATION MUST BE HANDLED PRIOR TO DB INTERACTION.
        /// </summary>
        /// Loads all divisions.
        /// <param name="token"></param>
        /// <returns></returns>
        [EndpointMethod(EndpointName = "LoadDivisions", AllowArgumentLogging = true, AllowResponseLogging = true, RequiresAuthentication = false)]
        private static void EndpointMethod_LoadDivisions(MessageToken token)
        {
            using (var session = DataAccess.NHibernateHelper.CreateStatefulSession())
            {
                //Very easily we're just going to throw back all the divisions.
                token.SetResult(session.QueryOver<Division>().List<Division>());
            }
        }

        /// <summary>
        /// WARNING!  THIS METHOD IS EXPOSED TO THE CLIENT AND IS NOT INTENDED FOR INTERNAL USE.  AUTHENTICATION, AUTHORIZATION AND VALIDATION MUST BE HANDLED PRIOR TO DB INTERACTION.
        /// </summary>
        /// Loads all divisions for a given department's Id.
        /// <param name="token"></param>
        /// <returns></returns>
        [EndpointMethod(EndpointName = "LoadDivisionsByDepartment", AllowArgumentLogging = true, AllowResponseLogging = true, RequiresAuthentication = false)]
        private static void EndpointMethod_LoadDivisionsByDepartment(MessageToken token)
        {

            if (!token.Args.ContainsKey("departmentid"))
            {
                token.AddErrorMessage("You failed to send a 'departmentid' parameter.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                return;
            }

            Guid departmentId;
            if (!Guid.TryParse(token.Args["departmentid"] as string, out departmentId))
            {
                token.AddErrorMessage("The 'departmentid' parameter that you sent was in the wrong format.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                return;
            }

            //We don't need to validate that this department is valid.  Just load all divisions by this department.  If we get none because the Id is bad, sucks to suck.
            using (var session = DataAccess.NHibernateHelper.CreateStatefulSession())
            {
                //Very easily we're just going to throw back all the divisions.
                token.SetResult(session.QueryOver<Division>().Where(x => x.Department.Id == departmentId).List<Division>());
            }
        }

        /// <summary>
        /// WARNING!  THIS METHOD IS EXPOSED TO THE CLIENT AND IS NOT INTENDED FOR INTERNAL USE.  AUTHENTICATION, AUTHORIZATION AND VALIDATION MUST BE HANDLED PRIOR TO DB INTERACTION.
        /// </summary>
        /// Adds a division with the given value and description for the given department id.
        /// <param name="token"></param>
        /// <returns></returns>
        [EndpointMethod(EndpointName = "AddDivision", AllowArgumentLogging = true, AllowResponseLogging = true, RequiresAuthentication = true)]
        private static void EndpointMethod_AddDivision(MessageToken token)
        {
            //First we need to know if the client is logged in and is a client.
            if (token.AuthenticationSession == null)
            {
                token.AddErrorMessage("You must be logged in to add a division.", ErrorTypes.Authentication, System.Net.HttpStatusCode.Forbidden);
                return;
            }

            if (!token.AuthenticationSession.Person.PermissionGroups.CanAccessSubmodules(SubModules.AdminTools.ToString()))
            {
                token.AddErrorMessage("Only developers may add divisions.", ErrorTypes.Authorization, System.Net.HttpStatusCode.Unauthorized);
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
                    Division division = new Division { Id = Guid.NewGuid(), Value = value, Description = description };

                    //Validate it.
                    var validationResult = division.Validate();

                    if (validationResult.Errors.Any())
                    {
                        token.AddErrorMessages(validationResult.Errors.Select(x => x.ErrorMessage), ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                        return;
                    }

                    //Ok it passed basic validation.  Now let's also get the department that will own this division.
                    if (!token.Args.ContainsKey("departmentid"))
                    {
                        token.AddErrorMessage("You failed to send a 'departmentid' parameter.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                        return;
                    }

                    Guid departmentId;
                    if (!Guid.TryParse(token.Args["departmentid"] as string, out departmentId))
                    {
                        token.AddErrorMessage("The 'departmentid' parameter that you sent was in the wrong format.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                        return;
                    }

                    //Ok, let's try to load this department.
                    var department = session.Get<Department>(departmentId);
                    if (department == null)
                    {
                        token.AddErrorMessage("The department Id you sent did not belong to a real department.  Sad face.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                        return;
                    }

                    //Well, conveniently, we now have the department here with all its divisions.  Let's see if the value is a duplicate.
                    if (department.Divisions.Any(x => x.Value.SafeEquals(division.Value)))
                    {
                        token.AddErrorMessage("The department, '{0}', already has a division named, '{1}'.".FormatS(department.Value, division.Value), ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                        return;
                    }

                    //Ok so this is all ok, let's tell the division who owns it and then add it.
                    division.Department = department;

                    //Omg, I think we're ready to actually save the division.
                    session.Save(division);

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
        /// Edits a given division's value and description.
        /// <param name="token"></param>
        /// <returns></returns>
        [EndpointMethod(EndpointName = "EditDivision", AllowArgumentLogging = true, AllowResponseLogging = true, RequiresAuthentication = true)]
        private static void EndpointMethod_EditDivision(MessageToken token)
        {
            //First we need to know if the client is logged in and is a client.
            if (token.AuthenticationSession == null)
            {
                token.AddErrorMessage("You must be logged in to edit a division.", ErrorTypes.Authentication, System.Net.HttpStatusCode.Forbidden);
                return;
            }

            if (!token.AuthenticationSession.Person.PermissionGroups.CanAccessSubmodules(SubModules.AdminTools.ToString()))
            {
                token.AddErrorMessage("Only developers may edit divisions.", ErrorTypes.Authorization, System.Net.HttpStatusCode.Unauthorized);
                return;
            }

            //Now we need the params from the client.  First up is the Id.
            if (!token.Args.ContainsKey("divisionid"))
            {
                token.AddErrorMessage("You didn't send a 'divisionid' parameter.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                return;
            }

            Guid divisionId;
            if (!Guid.TryParse(token.Args["divisionid"] as string, out divisionId))
            {
                token.AddErrorMessage("The 'divisionid' you provided was in the wrong format.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                return;
            }

            //Let's load the division and make sure it's real.
            using (var session = DataAccess.NHibernateHelper.CreateStatefulSession())
            using (var transaction = session.BeginTransaction())
            {
                try
                {
                    var division = session.Get<Division>(divisionId);

                    if (division == null)
                    {
                        token.AddErrorMessage("The division id that you provided did not resolve to a real division.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                        return;
                    }

                    //Ok, so we have the list.  Now let's put the client's values in and ask if they're valid.
                    if (token.Args.ContainsKey("value"))
                        division.Value = token.Args["value"] as string;
                    if (token.Args.ContainsKey("description"))
                        division.Description = token.Args["description"] as string;

                    //Validation
                    var result = division.Validate();

                    if (!result.IsValid)
                    {
                        token.AddErrorMessages(result.Errors.Select(x => x.ErrorMessage), ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                        return;
                    }

                    //Also make sure no division has this value.
                    if (session.CreateCriteria<Division>().Add(Expression.Like("Value", division.Value)).List<Division>().Any(x => x.Id != division.Id))
                    {
                        token.AddErrorMessage("A division with that value already exists.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                        return;
                    }

                    //Ok that's all good.  Let's update the department.
                    session.Update(division);

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
        /// Maps a division to the database.
        /// </summary>
        public class DivisionMapping : ClassMap<Division>
        {
            /// <summary>
            /// Maps a division to the database.
            /// </summary>
            public DivisionMapping()
            {
                Id(x => x.Id).GeneratedBy.Assigned();

                Map(x => x.Value).Not.Nullable().Unique().Length(20);
                Map(x => x.Description).Nullable().Length(50);

                References(x => x.Department).LazyLoad(Laziness.False);

                Cache.ReadWrite();
            }
        }

        /// <summary>
        /// Validates le division.
        /// </summary>
        public class DivisionValidator : AbstractValidator<Division>
        {
            /// <summary>
            /// Validates the division.
            /// </summary>
            public DivisionValidator()
            {
                RuleFor(x => x.Description).Length(0, 40)
                    .WithMessage("The description of a Department must be no more than 40 characters.");
                RuleFor(x => x.Value).Length(1, 15)
                    .WithMessage("The value of a Department must be between one and ten characters.");
            }
        }
    }
}
