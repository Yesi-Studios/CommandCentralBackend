using System;
using System.Collections.Generic;
using CCServ.ClientAccess;
using FluentNHibernate.Mapping;
using FluentValidation;
using AtwoodUtils;
using System.Linq;
using NHibernate.Criterion;
using Humanizer;

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
                        throw new AggregateException(result.Errors.Select(x => new CommandCentralException(x.ErrorMessage, ErrorTypes.Validation)));

                    //Here, we're going to see if the value already exists.  
                    //This is in response to a bug in which duplicate value entries will cause a bug.
                    if (session.QueryOver<NEC>().Where(x => x.Value.IsInsensitiveLike(nec.Value)).RowCount() != 0)
                        throw new CommandCentralException("The value, '{0}', already exists in the list.".FormatWith(nec.Value), ErrorTypes.Validation);

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
                    var nec = session.Get<NEC>(id) ??
                        throw new CommandCentralException("That nec Id was not valid.", ErrorTypes.Validation);

                    NEC necAlias = null;

                    var persons = session.QueryOver<Person>()
                        .JoinAlias(x => x.SecondaryNECs, () => necAlias)
                        .Where(Restrictions.Disjunction().Add<Person>(x => x.PrimaryNEC == nec).Add(() => necAlias.Id == nec.Id))
                        .List();

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
                            throw new CommandCentralException("We were unable to delete the nec, {0}, because it is referenced on {1} profile(s).".FormatS(nec, persons.Count), ErrorTypes.Validation);
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
                
                References(x => x.NECType).Not.LazyLoad();

                Cache.ReadWrite();
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
