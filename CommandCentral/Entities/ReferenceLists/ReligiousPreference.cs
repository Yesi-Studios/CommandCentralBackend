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
    /// Describes a single Religious Preference
    /// </summary>
    public class ReligiousPreference : EditableReferenceListItemBase
    {
        /// <summary>
        /// Update or insert.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="token"></param>
        public override void UpdateOrInsert(Newtonsoft.Json.Linq.JToken item, ClientAccess.MessageToken token)
        {
            using (var session = DataAccess.DataProvider.CreateStatefulSession())
            using (var transaction = session.BeginTransaction())
            {
                try
                {
                    var relPref = item.CastJToken<ReligiousPreference>();

                    //Validate it.
                    var result = relPref.Validate();
                    if (!result.IsValid)
                        throw new AggregateException(result.Errors.Select(x => new CommandCentralException(x.ErrorMessage, ErrorTypes.Validation)));

                    //Here, we're going to see if the value already exists.  
                    //This is in response to a bug in which duplicate value entries will cause a bug.
                    if (session.QueryOver<ReligiousPreference>().Where(x => x.Value.IsInsensitiveLike(relPref.Value)).RowCount() != 0)
                        throw new CommandCentralException("The value, '{0}', already exists in the list.".With(relPref.Value), ErrorTypes.Validation);

                    var relPrefFromDB = session.Get<ReligiousPreference>(relPref.Id);

                    if (relPrefFromDB == null)
                    {
                        relPref.Id = Guid.NewGuid();
                        session.Save(relPref);
                    }
                    else
                    {
                        session.Merge(relPref);
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
        public override void Delete(System.Guid id, bool forceDelete, ClientAccess.MessageToken token)
        {
            using (var session = DataAccess.DataProvider.CreateStatefulSession())
            using (var transaction = session.BeginTransaction())
            {
                try
                {
                    var relPref = session.Get<ReligiousPreference>(id) ??
                        throw new CommandCentralException("That religious preference Id was not valid.", ErrorTypes.Validation);

                    var persons = session.QueryOver<Person>().Where(x => x.ReligiousPreference == relPref).List();

                    if (persons.Any())
                    {
                        if (forceDelete)
                        {
                            foreach (var person in persons)
                            {
                                person.ReligiousPreference = null;

                                session.Update(person);
                            }

                            //Now that everything is cleaned up, drop the object.
                            session.Delete(relPref);
                        }
                        else
                        {
                            //There were references but we can't delete them.
                            throw new CommandCentralException("We were unable to delete the religious preference, {0}, because it is referenced on {1} profile(s).".With(relPref, persons.Count), ErrorTypes.Validation);
                        }
                    }
                    else
                    {
                        session.Delete(relPref);
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
                if (id == default(Guid))
                {
                    return session.QueryOver<ReligiousPreference>()
                        .Cacheable().CacheMode(NHibernate.CacheMode.Normal)
                        .List<ReferenceListItemBase>().ToList();
                }
                else
                {
                    return new[] { (ReferenceListItemBase)session.Get<ReligiousPreference>(id) }.ToList();
                }
            }
        }

        /// <summary>
        /// Validates this religious preference.  
        /// </summary>
        /// <returns></returns>
        public override FluentValidation.Results.ValidationResult Validate()
        {
            return new ReligiousPreferenceValidator().Validate(this);
        }
        /// <summary>
        /// Maps a Religious Preference to the database.
        /// </summary>
        public class ReligiousPreferenceMapping : ClassMap<ReligiousPreference>
        {
            /// <summary>
            /// Maps a Religious Preference to the database.
            /// </summary>
            public ReligiousPreferenceMapping()
            {
                Id(x => x.Id).GeneratedBy.Assigned();

                Map(x => x.Value).Not.Nullable().Unique();
                Map(x => x.Description);

                Cache.ReadWrite();
            }
        }

        /// <summary>
        /// Validates the RP
        /// </summary>
        public class ReligiousPreferenceValidator : AbstractValidator<ReligiousPreference>
        {
            /// <summary>
            /// Validates the RP
            /// </summary>
            public ReligiousPreferenceValidator()
            {
                RuleFor(x => x.Description).Length(0, 255)
                    .WithMessage("A religious preference's decription may be no more than 255 characters.");
                RuleFor(x => x.Value).NotEmpty()
                    .WithMessage("The value must not be empty.");
            }
        }

    }
}
