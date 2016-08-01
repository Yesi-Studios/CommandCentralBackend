using System;
using CommandCentral.ClientAccess;
using FluentNHibernate.Mapping;
using FluentValidation;

namespace CommandCentral.Entities.ReferenceLists
{
    /// <summary>
    /// Describes a single Division.
    /// </summary>
    public class Division
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
                token.SetResult(session.QueryOver<Division>().Where(x => x.Department.Id == departmentId).List<Department>());
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
                Id(x => x.Id).GeneratedBy.Guid();

                Map(x => x.Value).Not.Nullable().Unique().Length(20);
                Map(x => x.Description).Nullable().Length(50);

                References(x => x.Department);

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
