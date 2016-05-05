using System;
using System.Collections.Generic;
using FluentNHibernate.Mapping;

namespace CommandCentral.Authorization
{
    /// <summary>
    /// Describes permissions to a given model.
    /// </summary>
    public class ModelPermission
    {

        #region Properties

        /// <summary>
        /// The unique Id of this model permission.
        /// </summary>
        public virtual Guid Id { get; set; }

        /// <summary>
        /// A unique name that identifies this model permission.
        /// </summary>
        public virtual string Name { get; set; }

        /// <summary>
        /// The name of the model.
        /// </summary>
        public virtual string ModelName { get; set; }

        /// <summary>
        /// The fields the user can search in in the model.
        /// </summary>
        public virtual IList<string> SearchableFields { get; set; }

        /// <summary>
        /// The fields a user is allowed to see from the model.
        /// </summary>
        public virtual IList<string> ReturnableFields { get; set; }

        /// <summary>
        /// The fields a user is allowed to edit in a model.
        /// </summary>
        public virtual IList<string> EditableFields { get; set; }

        #endregion

        #region Overrides

        /// <summary>
        /// Returns Name.
        /// </summary>
        /// <returns></returns>
        public new virtual string ToString()
        {
            return Name;
        }

        #endregion

        #region ctors

        /// <summary>
        /// Initializes the lists to new lists.
        /// </summary>
        public ModelPermission()
        {
            ReturnableFields = new List<string>();
            EditableFields = new List<string>();
            SearchableFields = new List<string>();
        }

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

                Id(x => x.Id).GeneratedBy.Guid();

                Map(x => x.Name).Not.Nullable().Unique().Length(20);
                Map(x => x.ModelName).Not.Nullable().Length(100);
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
