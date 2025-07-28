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
using IReleaseVersionRepository =
    GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces.IReleaseVersionRepository;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers;

public class ViewSpecificMethodologyRequirement : IAuthorizationRequirement
{
}

public class ViewSpecificMethodologyAuthorizationHandler :
    AuthorizationHandler<ViewSpecificMethodologyRequirement, MethodologyVersion>
{
    private readonly IMethodologyRepository _methodologyRepository;
    private readonly IUserReleaseRoleRepository _userReleaseRoleRepository;
    private readonly IPreReleaseService _preReleaseService;
    private readonly IReleaseVersionRepository _releaseVersionRepository;
    private readonly AuthorizationHandlerService _authorizationHandlerService;

    public ViewSpecificMethodologyAuthorizationHandler(
        IMethodologyRepository methodologyRepository,
        IUserReleaseRoleRepository userReleaseRoleRepository,
        IPreReleaseService preReleaseService,
        IReleaseVersionRepository releaseVersionRepository,
        AuthorizationHandlerService authorizationHandlerService)
    {
        _methodologyRepository = methodologyRepository;
        _userReleaseRoleRepository = userReleaseRoleRepository;
        _preReleaseService = preReleaseService;
        _releaseVersionRepository = releaseVersionRepository;
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
        // view it.  Additionally, if the user is a Contributor or an Approver of any
        // (Live or non-Live) release version of the owning publication of this methodology, they can view it.
        if (await _authorizationHandlerService
                .HasRolesOnPublicationOrAnyReleaseVersion(
                    context.User.GetUserId(),
                    owningPublication.Id,
                    ListOf(PublicationRole.Owner, PublicationRole.Allower),
                    ReleaseEditorAndApproverRoles))
        {
            context.Succeed(requirement);
            return;
        }

        // A user can view an approved methodology version used by a release version in prerelease if they have
        // PrereleaseViewer role on the release version.
        // The release version must be the latest version of the most recent release by time series for the
        // publication, approved but unpublished, and within the prerelease window.
        if (methodologyVersion.Approved)
        {
            var publicationIds = await _methodologyRepository
                .GetAllPublicationIds(methodologyVersion.MethodologyId);

            foreach (var publicationId in publicationIds)
            {
                var latestReleaseVersion = await _releaseVersionRepository.GetLatestReleaseVersion(publicationId);

                // The publication may have no releases
                if (latestReleaseVersion == null)
                {
                    continue;
                }

                // A published release is not in prerelease
                if (latestReleaseVersion.Live)
                {
                    continue;
                }

                // An unapproved release is not in prerelease
                if (latestReleaseVersion.ApprovalStatus != ReleaseApprovalStatus.Approved)
                {
                    continue;
                }

                if (await _userReleaseRoleRepository.HasUserReleaseRole(
                        context.User.GetUserId(),
                        latestReleaseVersion.Id,
                        ReleaseRole.PrereleaseViewer))
                {
                    if (_preReleaseService
                            .GetPreReleaseWindowStatus(latestReleaseVersion, DateTime.UtcNow)
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
