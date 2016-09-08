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
    public class Division : EditableReferenceListItemBase
    {

        #region Properties

        /// <summary>
        /// The department to which this division belongs.
        /// </summary>
        public virtual Department Department { get; set; }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Update or Insert
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
                    //First try to get the department this thing is a part of.
                    Guid departmentId;
                    if (!Guid.TryParse(item.Value<string>("departmentid"), out departmentId))
                    {
                        token.AddErrorMessage("The department id was not valid.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                        return;
                    }

                    var department = session.Get<Department>(departmentId);
                    if (department == null)
                    {
                        token.AddErrorMessage("The department id was not valid.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                        return;
                    }

                    //Cool the department was good.  Build the division.
                    var division = item.CastJToken<Division>();
                    division.Department = department;

                    //Now validate it.
                    var result = division.Validate();
                    if (!result.IsValid)
                    {
                        token.AddErrorMessages(result.Errors.Select(x => x.ErrorMessage), ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                        return;
                    }

                    //Try to get it.
                    var divisionFromDB = session.Get<Division>(division.Id);

                    //If it's null then add it.
                    if (divisionFromDB == null)
                    {
                        division.Id = Guid.NewGuid();
                        session.Save(division);
                    }
                    else
                    {
                        //If it's not null, then merge it.
                        session.Merge(division);
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
        /// Delete
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
                    //First try to get the division.
                    var division = session.Get<Division>(id);

                    if (division == null)
                    {
                        token.AddErrorMessage("That division Id was not valid.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                        return;
                    }

                    //Ok, now find all the entities it's a part of.
                    var persons = session.QueryOver<Person>().Where(x => x.Division == division).List();

                    if (persons.Any())
                    {
                        //There are references to deal with.
                        if (forceDelete)
                        {
                            //The client is telling us to force delete the division.  Now we need to clean up everything.
                            foreach (var person in persons)
                            {
                                person.Division = null;

                                session.Save(person);
                            }

                            //Now that everything is cleaned up, drop the division.
                            session.Delete(division);
                        }
                        else
                        {
                            //There were references but we can't delete them.
                            token.AddErrorMessage("We were unable to delete the division, {0}, because it is referenced on {1} profile(s).".FormatS(division, persons.Count), ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                            return;
                        }
                    }
                    else
                    {
                        //There are no references, let's drop the division.
                        session.Delete(division);
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
        /// Load
        /// </summary>
        /// <param name="id"></param>
        /// <param name="token"></param>
        public override List<ReferenceListItemBase> Load(Guid id, MessageToken token)
        {
            using (var session = DataAccess.NHibernateHelper.CreateStatefulSession())
            {
                if (id != default(Guid))
                {
                    return new[] { (ReferenceListItemBase)session.Get<Division>(id) }.ToList();
                }
                else
                {
                    //The id is blank... but were we given a department Id?
                    if (token.Args.ContainsKey("departmentid"))
                    {
                        //Yes we were!
                        Guid departmentId;
                        if (!Guid.TryParse(token.Args["departmentid"] as string, out departmentId))
                        {
                            token.AddErrorMessage("The department id was not valid.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                            return null;
                        }

                        //Cool, give them back the divisions in this department.
                        return session.QueryOver<Division>().Where(x => x.Department.Id == departmentId).List<ReferenceListItemBase>().ToList();
                    }
                    else
                    {
                        //Nope, just give them all the departments.
                        return session.QueryOver<Division>().List<ReferenceListItemBase>().ToList();
                    }
                }
            }
        }

        /// <summary>
        /// Validates this division object.
        /// </summary>
        /// <returns></returns>
        public override FluentValidation.Results.ValidationResult Validate()
        {
            return new DivisionValidator().Validate(this);
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

                Map(x => x.Value).Not.Nullable().Unique();
                Map(x => x.Description);

                References(x => x.Department).LazyLoad(Laziness.False);
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
                RuleFor(x => x.Description).Length(0, 255)
                    .WithMessage("The description of a Department must be no more than 255 characters.");
                RuleFor(x => x.Value).NotEmpty()
                    .WithMessage("The value must not be empty");
            }
        }

        
    }
}
