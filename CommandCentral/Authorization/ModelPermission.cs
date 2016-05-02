using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentNHibernate.Mapping;
using CommandCentral.DataAccess;

namespace CommandCentral.Authorization
{
    /// <summary>
    /// Describes permissions to a given model.
    /// </summary>
    public class ModelPermission
    {

        #region Properties

        /// <summary>
        /// The unique ID of this model permission.
        /// </summary>
        public virtual Guid ID { get; set; }

        /// <summary>
        /// The name of the model.
        /// </summary>
        public virtual string ModelName { get; set; }

        /// <summary>
        /// The fields the user can search in in the model.
        /// </summary>
        public virtual List<string> SearchableFields { get; set; }

        /// <summary>
        /// The fields a user is allowed to see from the model.
        /// </summary>
        public virtual List<string> ReturnableFields { get; set; }

        /// <summary>
        /// The fields a user is allowed to edit in a model.
        /// </summary>
        public virtual List<string> EditableFields { get; set; }

        #endregion

        /// <summary>
        /// Maps the model permission to the database.
        /// </summary>
        public class ModelPermissionMapping : ClassMap<ModelPermission>
        {
            /// <summary>
            /// Maps the model permission to the database.
            /// </summary>
            public ModelPermissionMapping()
            {
                Table("modelpermissions");

                Id(x => x.ID).GeneratedBy.Guid();

                Map(x => x.ModelName).Not.Nullable().Length(20);
                HasMany(x => x.SearchableFields)
                    .KeyColumn("ModelPermissionID")
                    .Table("modelpermissionsearchablefields")
                    .Element("SearchableField");
                HasMany(x => x.ReturnableFields)
                    .KeyColumn("ModelPermissionID")
                    .Table("modelpermissionreturnablefields")
                    .Element("ReturnableField");
                HasMany(x => x.EditableFields)
                    .KeyColumn("ModelPermissionID")
                    .Table("modelpermissioneditablefields")
                    .Element("EditableFields");
            }
        }

    }
}
