using System;
using System.Collections.Generic;
using CommandCentral.ClientAccess;
using FluentNHibernate.Mapping;
using FluentValidation;
using AtwoodUtils;
using System.Linq;
using NHibernate.Criterion;

namespace CommandCentral.Entities.ReferenceLists
{
    /// <summary>
    /// Describes a single ethnicity
    /// </summary>
    public class Ethnicity : EditableReferenceListItemBase
    {
        /// <summary>
        /// Update or insert.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="token"></param>
        public override void UpdateOrInsert(Newtonsoft.Json.Linq.JToken item, ClientAccess.MessageToken token)
        {
            using (var session = DataAccess.NHibernateHelper.CreateStatefulSession())
            using (var transaction = session.BeginTransaction())
            {
                try
                {
                    var ethnicity = item.CastJToken<Ethnicity>();

                    //Validate it.
                    var result = ethnicity.Validate();
                    if (!result.IsValid)
                        throw new AggregateException(result.Errors.Select(x => new CommandCentralException(x.ErrorMessage, ErrorTypes.Validation)));

                    //Here, we're going to see if the value already exists.  
                    //This is in response to a bug in which duplicate value entries will cause a bug.
                    if (session.QueryOver<Ethnicity>().Where(x => x.Value.IsInsensitiveLike(ethnicity.Value)).RowCount() != 0)
                        throw new CommandCentralException("The value, '{0}', already exists in the list.".FormatS(ethnicity.Value), ErrorTypes.Validation);

                    var ethnicityFromDB = session.Get<Ethnicity>(ethnicity.Id);

                    if (ethnicityFromDB == null)
                    {
                        ethnicity.Id = Guid.NewGuid();
                        session.Save(ethnicity);
                    }
                    else
                    {
                        session.Merge(ethnicity);
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
                    var ethnicity = session.Get<Ethnicity>(id) ??
                        throw new CommandCentralException("That ethnicity Id was not valid.", ErrorTypes.Validation);

                    var persons = session.QueryOver<Person>().Where(x => x.Ethnicity == ethnicity).List();

                    if (persons.Any())
                    {
                        if (forceDelete)
                        {
                            foreach (var person in persons)
                            {
                                person.Ethnicity = null;

                                session.Update(person);
                            }

                            //Now that everything is cleaned up, drop the object.
                            session.Delete(ethnicity);
                        }
                        else
                        {
                            //There were references but we can't delete them.
                            throw new CommandCentralException("We were unable to delete the ethnicity, {0}, because it is referenced on {1} profile(s).".FormatS(ethnicity, persons.Count), ErrorTypes.Validation);
                        }
                    }
                    else
                    {
                        session.Delete(ethnicity);
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
        public override List<ReferenceListItemBase> Load(System.Guid id, ClientAccess.MessageToken token)
        {
            using (var session = DataAccess.NHibernateHelper.CreateStatefulSession())
            {
                if (id == default(Guid))
                {
                    return session.QueryOver<Ethnicity>()
                        .Cacheable().CacheMode(NHibernate.CacheMode.Normal)
                        .List<ReferenceListItemBase>().ToList();
                }
                else
                {
                    return new[] { (ReferenceListItemBase)session.Get<Ethnicity>(id) }.ToList();
                }
            }
        }

        /// <summary>
        /// Validates this ethnicity.
        /// </summary>
        /// <returns></returns>
        public override FluentValidation.Results.ValidationResult Validate()
        {
            return new EthnicityValidator().Validate(this);
        }

        /// <summary>
        /// Maps an ethnicity to the database.
        /// </summary>
        public class EthnicityMapping : ClassMap<Ethnicity>
        {
            /// <summary>
            /// Maps an ethnicity to the database.
            /// </summary>
            public EthnicityMapping()
            {
                Id(x => x.Id).GeneratedBy.Assigned();

                Map(x => x.Value).Not.Nullable().Unique();
                Map(x => x.Description);

                Cache.ReadWrite();
            }
        }

        /// <summary>
        /// Validates an ethnicity.
        /// </summary>
        public class EthnicityValidator : AbstractValidator<Ethnicity>
        {
            /// <summary>
            /// Validates an ethnicity.
            /// </summary>
            public EthnicityValidator()
            {
                RuleFor(x => x.Description).Length(0, 255)
                    .WithMessage("The description of an ethnicity may be no more than 255 characters.");
                RuleFor(x => x.Value).NotEmpty()
                    .WithMessage("The value must not be empty.");
            }
        }

        
    }

}
