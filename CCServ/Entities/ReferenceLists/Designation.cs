using System;
using System.Collections.Generic;
using CCServ.ClientAccess;
using FluentNHibernate.Mapping;
using FluentValidation;
using AtwoodUtils;
using System.Linq;

namespace CCServ.Entities.ReferenceLists
{
    /// <summary>
    /// Describes a single designation.  This is the job title for civilians, the rate for enlisted and the designator for officers.
    /// </summary>
    public class Designation : EditableReferenceListItemBase
    {
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
                    var designation = item.CastJToken<Designation>();

                    //Validate it.
                    var result = designation.Validate();
                    if (!result.IsValid)
                    {
                        token.AddErrorMessages(result.Errors.Select(x => x.ErrorMessage), ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                        return;
                    }

                    var designationFromDB = session.Get<Designation>(designation.Id);

                    if (designationFromDB == null)
                    {
                        designation.Id = Guid.NewGuid();
                        session.Save(designation);
                    }
                    else
                    {
                        session.Merge(designation);
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
        /// Delete the designation.
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
                    var designation = session.Get<Designation>(id);

                    if (designation == null)
                    {
                        token.AddErrorMessage("That designation Id was not valid.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                        return;
                    }

                    var persons = session.QueryOver<Person>().Where(x => x.Designation == designation).List();

                    if (persons.Any())
                    {
                        if (forceDelete)
                        {
                            foreach (var person in persons)
                            {
                                person.Designation = null;

                                session.Update(person);
                            }

                            //Now that everything is cleaned up, drop the designation.
                            session.Delete(designation);
                        }
                        else
                        {
                            //There were references but we can't delete them.
                            token.AddErrorMessage("We were unable to delete the designation, {0}, because it is referenced on {1} profile(s).".FormatS(designation, persons.Count), ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                            return;
                        }
                    }
                    else
                    {
                        session.Delete(designation);
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
        /// Load the designation.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="token"></param>
        public override List<ReferenceListItemBase> Load(Guid id, MessageToken token)
        {
            using (var session = DataAccess.NHibernateHelper.CreateStatefulSession())
            {
                if (id == default(Guid))
                {
                    return session.QueryOver<Designation>().List<ReferenceListItemBase>().ToList();
                }
                else
                {
                    return new[] { (ReferenceListItemBase)session.Get<Designation>(id) }.ToList();
                }
            }
        }

        /// <summary>
        /// Validates this designation.
        /// </summary>
        /// <returns></returns>
        public override FluentValidation.Results.ValidationResult Validate()
        {
            return new DesignationValidator().Validate(this);
        }

        /// <summary>
        /// Maps a Designation to the database.
        /// </summary>
        public class DesignationMapping : ClassMap<Designation>
        {
            /// <summary>
            /// Maps a Designation to the database.
            /// </summary>
            public DesignationMapping()
            {
                Id(x => x.Id).GeneratedBy.Assigned();

                Map(x => x.Value).Not.Nullable().Unique();
                Map(x => x.Description);
            }
        }

        /// <summary>
        /// Validates a designation.
        /// </summary>
        public class DesignationValidator : AbstractValidator<Designation>
        {
            /// <summary>
            /// Validates a designation.
            /// </summary>
            public DesignationValidator()
            {
                RuleFor(x => x.Description).Length(0, 255)
                    .WithMessage("The description of a designation can be no more than 255 characters.");
                RuleFor(x => x.Value).NotEmpty()
                    .WithMessage("The value must not be empty.");
            }
        }

        
    }
}
