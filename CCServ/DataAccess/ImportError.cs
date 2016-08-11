using System;
using System.Collections.Generic;
using System.Linq;
using AtwoodUtils;
using CCServ.Authorization;
using CCServ.ClientAccess;
using FluentNHibernate.Mapping;
using FluentValidation;
using NHibernate.Criterion;

namespace CCServ.DataAccess
{
    /// <summary>
    /// Persists an import error in the database.
    /// </summary>
    public class ImportError
    {
        #region Properties

        /// <summary>
        /// The id of this import error.
        /// </summary>
        public virtual Guid Id { get; set; }

        /// <summary>
        /// The name of the property that was in error.
        /// </summary>
        public virtual string PropertyName { get; set; }

        /// <summary>
        /// The name of the object that was in error.
        /// </summary>
        public virtual string ObjectName { get; set; }

        /// <summary>
        /// The id of the object that was in error.
        /// </summary>
        public virtual string ObjectId { get; set; }

        /// <summary>
        /// The value that caused the error.
        /// </summary>
        public virtual string AttemptedValue { get; set; }

        /// <summary>
        /// The error message that was thrown.
        /// </summary>
        public virtual string ErrorMessage { get; set; }

        #endregion

        /// <summary>
        /// Persists an import error in the database.
        /// </summary>
        public class ImportErrorMapping : ClassMap<ImportError>
        {
            /// <summary>
            /// Persists an import error in the database.
            /// </summary>
            public ImportErrorMapping()
            {
                Id(x => x.Id).GeneratedBy.Guid();

                Map(x => x.PropertyName);
                Map(x => x.ObjectName);
                Map(x => x.ObjectId);
                Map(x => x.ErrorMessage);
                Map(x => x.AttemptedValue);
            }
        }
    }
}
