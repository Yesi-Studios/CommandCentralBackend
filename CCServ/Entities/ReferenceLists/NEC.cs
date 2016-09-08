﻿using System;
using System.Collections.Generic;
using CCServ.ClientAccess;
using FluentNHibernate.Mapping;
using FluentValidation;
using AtwoodUtils;
using System.Linq;

namespace CCServ.Entities.ReferenceLists
{
    /// <summary>
    /// Describes a single NEC.
    /// </summary>
    public class NEC : EditableReferenceListItemBase
    {

        /// <summary>
        /// The type of the NEC.
        /// </summary>
        public virtual PersonType NECType { get; set; }

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
                    var nec = item.CastJToken<NEC>();

                    //Validate it.
                    var result = nec.Validate();
                    if (!result.IsValid)
                    {
                        token.AddErrorMessages(result.Errors.Select(x => x.ErrorMessage), ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                        return;
                    }

                    var necFromDB = session.Get<NEC>(nec.Id);

                    if (necFromDB == null)
                    {
                        nec.Id = Guid.NewGuid();
                        session.Save(nec);
                    }
                    else
                    {
                        session.Merge(nec);
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
            using (var session = DataAccess.NHibernateHelper.CreateStatefulSession())
            using (var transaction = session.BeginTransaction())
            {
                try
                {
                    var nec = session.Get<NEC>(id);

                    if (nec == null)
                    {
                        token.AddErrorMessage("That nec Id was not valid.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                        return;
                    }

                    var persons = session.QueryOver<Person>().Where(x => x.PrimaryNEC == nec || x.SecondaryNECs.Any(y => y == nec)).List();

                    if (persons.Any())
                    {
                        if (forceDelete)
                        {
                            foreach (var person in persons)
                            {
                                if (person.PrimaryNEC == nec)
                                    person.PrimaryNEC = null;

                                var secondaryNECs = person.SecondaryNECs.ToList();
                                secondaryNECs.RemoveAll(x => x == nec);
                                person.SecondaryNECs = secondaryNECs;

                                session.Update(person);
                            }

                            //Now that everything is cleaned up, drop the nec.
                            session.Delete(nec);
                        }
                        else
                        {
                            //There were references but we can't delete them.
                            token.AddErrorMessage("We were unable to delete the nec, {0}, because it is referenced on {1} profile(s).".FormatS(nec, persons.Count), ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                            return;
                        }
                    }
                    else
                    {
                        session.Delete(nec);
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
        public override void Load(System.Guid id, ClientAccess.MessageToken token)
        {
            using (var session = DataAccess.NHibernateHelper.CreateStatefulSession())
            {
                if (id != default(Guid))
                {
                    token.SetResult(session.Get<NEC>(id));
                }
                else
                {
                    token.SetResult(session.QueryOver<NEC>().List());
                }
            }
        }

        /// <summary>
        /// Validates an NEC.  Weeee.
        /// </summary>
        /// <returns></returns>
        public override FluentValidation.Results.ValidationResult Validate()
        {
            return new NECValidator().Validate(this);
        }

        /// <summary>
        /// Maps an NEC to the database.
        /// </summary>
        public class NECMapping : ClassMap<NEC>
        {
            /// <summary>
            /// Maps an NEC to the database.
            /// </summary>
            public NECMapping()
            {
                Id(x => x.Id).GeneratedBy.Assigned();

                Map(x => x.Value).Not.Nullable().Unique();
                Map(x => x.Description);
                
                References(x => x.NECType).LazyLoad(Laziness.False);
            }
        }

        /// <summary>
        /// Validates an NEC
        /// </summary>
        public class NECValidator : AbstractValidator<NEC>
        {
            /// <summary>
            /// Validates an NEC
            /// </summary>
            public NECValidator()
            {
                RuleFor(x => x.Description).Length(0, 255)
                    .WithMessage("The description of an NEC can be no more than 255 characters.");
                RuleFor(x => x.Value).NotEmpty()
                    .WithMessage("The value must not be empty.");
            }
        }

        
    }
}
