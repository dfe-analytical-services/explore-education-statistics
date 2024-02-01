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
    AuthorizationHandlerService;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;
using IReleaseRepository =
    GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces.IReleaseRepository;

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
        private readonly IReleaseRepository _releaseRepository;
        private readonly AuthorizationHandlerService _authorizationHandlerService;

        public ViewSpecificMethodologyAuthorizationHandler(
            IMethodologyRepository methodologyRepository,
            IUserReleaseRoleRepository userReleaseRoleRepository,
            IPreReleaseService preReleaseService,
            IReleaseRepository releaseRepository,
            AuthorizationHandlerService authorizationHandlerService)
        {
            _methodologyRepository = methodologyRepository;
            _userReleaseRoleRepository = userReleaseRoleRepository;
            _preReleaseService = preReleaseService;
            _releaseRepository = releaseRepository;
            _authorizationHandlerService = authorizationHandlerService;
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
            // view it.  Additionally, if the user is an Editor (Contributor, Lead) or an Approver of any
            // (Live or non-Live) Release of the owning Publication of this Methodology, they can view it.
            if (await _authorizationHandlerService
                    .HasRolesOnPublicationOrAnyRelease(
                        context.User.GetUserId(),
                        owningPublication.Id,
                        ListOf(PublicationRole.Owner, PublicationRole.Approver),
                        ReleaseEditorAndApproverRoles))
            {
                context.Succeed(requirement);
                return;
            }

            // A user can view an approved methodology version used by a release in prerelease if they have
            // PrereleaseViewer role on the release.
            // The release must be the latest version of the most recent release by time series for the publication,
            // approved but unpublished, and within the prerelease window.
            if (methodologyVersion.Approved)
            {
                var publicationIds = await _methodologyRepository
                    .GetAllPublicationIds(methodologyVersion.MethodologyId);

                foreach (var publicationId in publicationIds)
                {
                    var latestRelease = await _releaseRepository.GetLatestReleaseVersion(publicationId);

                    // The publication may have no releases
                    if (latestRelease == null)
                    {
                        continue;
                    }

                    // A published release is not in prerelease
                    if (latestRelease.Live)
                    {
                        continue;
                    }

                    // An unapproved release is not in prerelease
                    if (latestRelease.ApprovalStatus != ReleaseApprovalStatus.Approved)
                    {
                        continue;
                    }

                    if (await _userReleaseRoleRepository.HasUserReleaseRole(
                            context.User.GetUserId(),
                            latestRelease.Id,
                            ReleaseRole.PrereleaseViewer))
                    {
                        if (_preReleaseService
                                .GetPreReleaseWindowStatus(latestRelease, DateTime.UtcNow)
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
