using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AtwoodUtils;
using CCServ.ClientAccess;
using FluentNHibernate.Mapping;
using FluentValidation;
using FluentValidation.Results;
using Humanizer;
using NHibernate.Criterion;

namespace CCServ.Entities.ReferenceLists.Watchbill
{
    /// <summary>
    /// Defines a watch input reason.  This is used to provide a selection of reasons as to why a person can not stand watch.
    /// </summary>
    public class WatchInputReason : EditableReferenceListItemBase
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
                    var reason = item.CastJToken<WatchInputReason>();

                    //Validate it.
                    var result = reason.Validate();
                    if (!result.IsValid)
                    {
                        throw new CommandCentralExceptions(result.Errors.Select(x => x.ErrorMessage), ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                        return;
                    }

                    //Here, we're going to see if the value already exists.  
                    //This is in response to a bug in which duplicate value entries will cause a bug.
                    if (session.QueryOver<WatchInputReason>().Where(x => x.Value.IsInsensitiveLike(reason.Value)).RowCount() != 0)
                    {
                        throw new CommandCentralException("The value, '{0}', already exists in the list.".FormatWith(reason.Value), ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                        return;
                    }

                    var reasonFromDB = session.Get<WatchInputReason>(reason.Id);

                    if (reasonFromDB == null)
                    {
                        reason.Id = Guid.NewGuid();
                        session.Save(reason);
                    }
                    else
                    {
                        session.Merge(reason);
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
            throw new CommandCentralException("Watch input reasons are not able to be deleted.  If you need it deleted, please contact the developers and remember to bring cookies as tribute.", ErrorTypes.Validation, System.Net.HttpStatusCode.MethodNotAllowed);
            return;
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
                if (id == default(Guid))
                {
                    return session.QueryOver<WatchInputReason>()
                        .Cacheable().CacheMode(NHibernate.CacheMode.Normal)
                        .List<ReferenceListItemBase>().ToList();
                }
                else
                {
                    return new[] { (ReferenceListItemBase)session.Get<WatchInputReason>(id) }.ToList();
                }
            }
        }

        /// <summary>
        /// We do not implement a validator
        /// </summary>
        /// <returns></returns>
        public override ValidationResult Validate()
        {
            return new WatchInputReasonValidator().Validate(this);
        }

        /// <summary>
        /// Maps this object to the database.
        /// </summary>
        public class WatchInputReasonMapping : ClassMap<WatchInputReason>
        {
            /// <summary>
            /// Maps this object to the database.
            /// </summary>
            public WatchInputReasonMapping()
            {
                Id(x => x.Id).GeneratedBy.Assigned();

                Map(x => x.Value).Not.Nullable().Unique();
                Map(x => x.Description);

                Cache.ReadWrite();
            }
        }

        /// <summary>
        /// Validates the object.
        /// </summary>
        public class WatchInputReasonValidator : AbstractValidator<WatchInputReason>
        {
            /// <summary>
            /// Validates the object.
            /// </summary>
            public WatchInputReasonValidator()
            {
                RuleFor(x => x.Description).Length(0, 255)
                    .WithMessage("The description of a watch input reason must be no more than 255 characters.");
                RuleFor(x => x.Value).NotEmpty()
                    .WithMessage("The value must not be null.");
            }
        }

    }
}
