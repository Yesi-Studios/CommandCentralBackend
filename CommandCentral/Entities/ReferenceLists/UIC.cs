﻿using System;
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
    /// Describes a single UIC.
    /// </summary>
    public class UIC : EditableReferenceListItemBase
    {
        /// <summary>
        /// Update or insert.
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
                    var uic = item.CastJToken<UIC>();

                    //Validate it.
                    var result = uic.Validate();
                    if (!result.IsValid)
                        throw new AggregateException(result.Errors.Select(x => new CommandCentralException(x.ErrorMessage, ErrorTypes.Validation)));

                    //Here, we're going to see if the value already exists.  
                    //This is in response to a bug in which duplicate value entries will cause a bug.
                    if (session.QueryOver<UIC>().Where(x => x.Value.IsInsensitiveLike(uic.Value)).RowCount() != 0)
                        throw new CommandCentralException("The value, '{0}', already exists in the list.".With(uic.Value), ErrorTypes.Validation);

                    var uicFromDB = session.Get<UIC>(uic.Id);

                    if (uicFromDB == null)
                    {
                        uic.Id = Guid.NewGuid();
                        session.Save(uic);
                    }
                    else
                    {
                        session.Merge(uic);
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
                    var uic = session.Get<UIC>(id) ??
                        throw new CommandCentralException("That uic Id was not valid.", ErrorTypes.Validation);

                    var persons = session.QueryOver<Person>().Where(x => x.UIC == uic).List();

                    if (persons.Any())
                    {
                        if (forceDelete)
                        {
                            foreach (var person in persons)
                            {
                                person.UIC = null;

                                session.Update(person);
                            }

                            //Now that everything is cleaned up, drop the object.
                            session.Delete(uic);
                        }
                        else
                        {
                            //There were references but we can't delete them.
                            throw new CommandCentralException("We were unable to delete the uic, {0}, because it is referenced on {1} profile(s).".With(uic, persons.Count), ErrorTypes.Validation);
                        }
                    }
                    else
                    {
                        session.Delete(uic);
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
                    return session.QueryOver<UIC>()
                        .Cacheable().CacheMode(NHibernate.CacheMode.Normal)
                        .List<ReferenceListItemBase>().ToList();
                }
                else
                {
                    return new[] { (ReferenceListItemBase)session.Get<UIC>(id) }.ToList();
                }
            }
        }

        /// <summary>
        /// Returns a validation result which contains the result of validation. lol.
        /// </summary>
        /// <returns></returns>
        public override FluentValidation.Results.ValidationResult Validate()
        {
            return new UICValidator().Validate(this);
        }

        /// <summary>
        /// Maps a UIC to the database.
        /// </summary>
        public class UICMapping : ClassMap<UIC>
        {
            /// <summary>
            /// Maps a UIC to the database.
            /// </summary>
            public UICMapping()
            {
                Id(x => x.Id).GeneratedBy.Assigned();

                Map(x => x.Value).Not.Nullable().Unique();
                Map(x => x.Description);

                Cache.ReadWrite();
            }
        }

        /// <summary>
        /// Validates the UIC.
        /// </summary>
        public class UICValidator : AbstractValidator<UIC>
        {
            /// <summary>
            /// Validates the UIC.
            /// </summary>
            public UICValidator()
            {
                RuleFor(x => x.Description).Length(0, 255)
                    .WithMessage("The description of a UIC must be no more than 255 characters.");
                RuleFor(x => x.Value).NotEmpty()
                    .WithMessage("The value must not be null.");
            }
        }


    }
}
