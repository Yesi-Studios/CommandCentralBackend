using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentNHibernate.Mapping;
using CommandCentral.ClientAccess;

namespace CommandCentral.Authorization
{
    /// <summary>
    /// Describes a single permission track which is a collection of permission groups.  This collection encompasses a single chain of command such as Watch CoC, Main CoC, etc.
    /// </summary>
    public class PermissionTrack
    {
        #region Properties

        /// <summary>
        /// The unique Id of this permission track.
        /// </summary>
        public virtual Guid Id { get; set; }

        /// <summary>
        /// The name of this permission track.
        /// </summary>
        public virtual string Name { get; set; }

        /// <summary>
        /// Those permission groups that make up this track.
        /// </summary>
        public virtual IList<PermissionGroup> PermissionGroups { get; set; }

        #endregion

        #region Client Methods

        /// <summary>
        /// WARNING!  THIS METHOD IS EXPOSED TO THE CLIENT AND IS NOT INTENDED FOR INTERNAL USE.  AUTHENTICATION, AUTHORIZATION AND VALIDATION MUST BE HANDLED PRIOR TO DB INTERACTION.
        /// </summary>
        /// Returns all permission tracks.
        /// <param name="token"></param>
        /// <returns></returns>
        [EndpointMethod(EndpointName = "LoadPermissionTracks", RequiresAuthentication = false, AllowArgumentLogging = true, AllowResponseLogging = true)]
        private static void EndpointMethod_LoadPermissionTracks(MessageToken token)
        {
            token.SetResult(token.CommunicationSession.QueryOver<PermissionTrack>().List());
        }

        #endregion

    }

    /// <summary>
    /// Maps a permission group to the
    /// </summary>
    public class PermissionTrackMapping : ClassMap<PermissionTrack>
    {
        public PermissionTrackMapping()
        {
            Id(x => x.Id).GeneratedBy.Guid();

            Map(x => x.Name).Not.Nullable();

            HasMany(x => x.PermissionGroups);
        }
    }
}
