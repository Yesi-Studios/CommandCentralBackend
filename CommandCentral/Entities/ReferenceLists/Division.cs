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
            using (var session = DataAccess.DataProvider.CreateStatefulSession())
            using (var transaction = session.BeginTransaction())
            {
                try
                {
                    //First try to get the department this thing is a part of.
                    if (!Guid.TryParse(item.Value<string>("departmentid"), out var departmentId))
                        throw new CommandCentralException("The department id was not valid.", ErrorTypes.Validation);

                    var departmentFromClient = session.Get<Department>(departmentId) ??
                        throw new CommandCentralException("The department id was not valid.", ErrorTypes.Validation);

                    //Cool the department was good.  Build the division.  We'll check to see if the department is the new or old one later.
                    var divisionFromClient = item.CastJToken<Division>();
                    divisionFromClient.Department = departmentFromClient;

                    //Now validate it.
                    var result = divisionFromClient.Validate();
                    if (!result.IsValid)
                        throw new AggregateException(result.Errors.Select(x => new CommandCentralException(x.ErrorMessage, ErrorTypes.Validation)));

                    //Try to get it.
                    var divisionFromDB = session.Get<Division>(divisionFromClient.Id);

                    //If it's null then add it.
                    if (divisionFromDB == null)
                    {
                        divisionFromClient.Id = Guid.NewGuid();
                        session.Save(divisionFromClient);
                    }
                    else
                    {
                        //If the client wants to update the department of a division, 
                        //we also need to go across all persons and update their departments.
                        if (divisionFromDB.Department.Id != divisionFromClient.Department.Id)
                        {
                            var persons = session.QueryOver<Person>().Where(x => x.Department == divisionFromDB.Department).List();

                            foreach (var person in persons)
                            {
                                person.Department = divisionFromClient.Department;
                                person.Command = divisionFromClient.Department.Command;
                                session.Update(person);
                            }
                        }

                        //If it's not null, then merge it.
                        divisionFromDB.Value = divisionFromClient.Value;
                        divisionFromDB.Description = divisionFromClient.Description;
                        divisionFromDB.Department = divisionFromClient.Department;
                        session.Update(divisionFromDB);
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
            using (var session = DataAccess.DataProvider.CreateStatefulSession())
            using (var transaction = session.BeginTransaction())
            {
                try
                {
                    //First try to get the division.
                    var division = session.Get<Division>(id) ??
                        throw new CommandCentralException("That division Id was not valid.", ErrorTypes.Validation);

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
                            throw new CommandCentralException("We were unable to delete the division, {0}, because it is referenced on {1} profile(s).".With(division, persons.Count), ErrorTypes.Validation);
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
            using (var session = DataAccess.DataProvider.CreateStatefulSession())
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
                        if (!Guid.TryParse(token.Args["departmentid"] as string, out var departmentId))
                        {
                            throw new CommandCentralException("The department id was not valid.", ErrorTypes.Validation);
                        }

                        //Cool, give them back the divisions in this department.
                        return session.QueryOver<Division>()
                            .Where(x => x.Department.Id == departmentId)
                            .Cacheable().CacheMode(NHibernate.CacheMode.Normal)
                            .List<ReferenceListItemBase>().ToList();
                    }
                    else
                    {
                        //Nope, just give them all the departments.
                        return session.QueryOver<Division>()
                            .Cacheable().CacheMode(NHibernate.CacheMode.Normal)
                            .List<ReferenceListItemBase>().ToList();
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
                RuleFor(x => x.Description).Length(0, 255)
                    .WithMessage("The description of a Department must be no more than 255 characters.");
                RuleFor(x => x.Value).NotEmpty()
                    .WithMessage("The value must not be empty");
            }
        }

        
    }
}
