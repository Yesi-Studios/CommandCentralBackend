using System;
using System.Collections.Generic;
using CCServ.ClientAccess;
using FluentNHibernate.Mapping;
using FluentValidation;

namespace CCServ.Entities.ReferenceLists
{
    /// <summary>
    /// Describes a single designation.  This is the job title for civilians, the rate for enlisted and the designator for officers.
    /// </summary>
    public class Designation : ReferenceListItemBase
    {
        public override void Delete(string entityName, Guid id, bool forceDelete, MessageToken token)
        {
            throw new NotImplementedException();
        }

        public override List<ReferenceListItemBase> Load(string entityName, MessageToken token)
        {
            throw new NotImplementedException();
        }

        public override void UpdateOrInsert(string entityName, ReferenceListItemBase item, MessageToken token)
        {
            throw new NotImplementedException();
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

                Cache.ReadWrite();
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
