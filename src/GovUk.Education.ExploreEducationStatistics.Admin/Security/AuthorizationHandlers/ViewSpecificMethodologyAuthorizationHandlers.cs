#nullable enable
using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Security;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using Microsoft.AspNetCore.Authorization;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers.
    AuthorizationHandlerResourceRoleService;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.PublicationRole;
using IPublicationRepository =
    GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.IPublicationRepository;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers
{
    public class ViewSpecificMethodologyRequirement : IAuthorizationRequirement
    {
    }

    public class ViewSpecificMethodologyAuthorizationHandler :
        AuthorizationHandler<ViewSpecificMethodologyRequirement, MethodologyVersion>
    {
        private readonly IMethodologyRepository _methodologyRepository;
        private readonly IUserReleaseRoleRepository _userReleaseRoleRepository;
        private readonly IPreReleaseService _preReleaseService;
        private readonly IPublicationRepository _publicationRepository;
        private readonly AuthorizationHandlerResourceRoleService _authorizationHandlerResourceRoleService;

        public ViewSpecificMethodologyAuthorizationHandler(
            IMethodologyRepository methodologyRepository,
            IUserReleaseRoleRepository userReleaseRoleRepository,
            IPreReleaseService preReleaseService,
            IPublicationRepository publicationRepository,
            AuthorizationHandlerResourceRoleService authorizationHandlerResourceRoleService)
        {
            _methodologyRepository = methodologyRepository;
            _userReleaseRoleRepository = userReleaseRoleRepository;
            _preReleaseService = preReleaseService;
            _publicationRepository = publicationRepository;
            _authorizationHandlerResourceRoleService = authorizationHandlerResourceRoleService;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context,
            ViewSpecificMethodologyRequirement requirement,
            MethodologyVersion methodologyVersion)
        {
            // If the user has a global Claim that allows them to access any Methodology, allow it.
            if (SecurityUtils.HasClaim(context.User, SecurityClaimTypes.AccessAllMethodologies))
            {
                context.Succeed(requirement);
                return;
            }

            var owningPublication =
                await _methodologyRepository.GetOwningPublication(methodologyVersion.MethodologyId);

            // If the user is a Publication Owner or Approver of the Publication that owns this Methodology, they can
            // view it.  Additionally, if the user is an Editor (Contributor, Lead) or an Approver of the latest
            // (Live or non-Live) Release of the owning Publication of this Methodology, they can view it.
            if (await _authorizationHandlerResourceRoleService
                    .HasRolesOnPublicationOrLatestRelease(
                        context.User.GetUserId(),
                        owningPublication.Id,
                        ListOf(Owner, Approver),
                        ReleaseEditorAndApproverRoles))
            {
                context.Succeed(requirement);
                return;
            }

            // If the user is a PrereleaseViewer of the latest non-Live, Approved Release of any Publication
            // using this Methodology, and the methodology is approved, and the latest release under that publication
            // is within the prerelease time window, they can view it
            if (methodologyVersion.Approved)
            {
                var publicationIds = await _methodologyRepository
                    .GetAllPublicationIds(methodologyVersion.MethodologyId);

                foreach (var publicationId in publicationIds)
                {
                    if (await _userReleaseRoleRepository.IsUserPrereleaseViewerOnLatestPreReleaseRelease(
                            context.User.GetUserId(),
                            publicationId))
                    {
                        var latestReleaseForConnectedPublication = await
                            _publicationRepository.GetLatestReleaseForPublication(publicationId);

                        if (latestReleaseForConnectedPublication != null &&
                            _preReleaseService
                                .GetPreReleaseWindowStatus(latestReleaseForConnectedPublication, DateTime.UtcNow)
                                .Access == PreReleaseAccess.Within)
                        {
                            context.Succeed(requirement);
                            return;
                        }
                    }
                }
            }
        }
    }
}
