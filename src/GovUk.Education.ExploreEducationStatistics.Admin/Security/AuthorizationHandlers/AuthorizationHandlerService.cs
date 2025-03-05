#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using IReleaseVersionRepository =
    GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces.IReleaseVersionRepository;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers;

public class AuthorizationHandlerService
{
    private static readonly ReleaseRole[] ReleaseEditorRoles =
    {
        ReleaseRole.Contributor,
        ReleaseRole.Lead
    };

    public static readonly ReleaseRole[] UnrestrictedReleaseViewerRoles =
    {
        ReleaseRole.Viewer,
        ReleaseRole.Contributor,
        ReleaseRole.Approver,
        ReleaseRole.Lead
    };

    public static readonly List<ReleaseRole> ReleaseEditorAndApproverRoles =
        ReleaseEditorRoles
            .Append(ReleaseRole.Approver)
            .ToList();

    private readonly IReleaseVersionRepository _releaseVersionRepository;
    private readonly IUserReleaseRoleRepository _userReleaseRoleRepository;
    private readonly IUserPublicationRoleRepository _userPublicationRoleRepository;
    private readonly IPreReleaseService _preReleaseService;

    public AuthorizationHandlerService(
        IReleaseVersionRepository releaseVersionRepository,
        IUserReleaseRoleRepository userReleaseRoleRepository,
        IUserPublicationRoleRepository userPublicationRoleRepository,
        IPreReleaseService preReleaseService)
    {
        _releaseVersionRepository = releaseVersionRepository;
        _userReleaseRoleRepository = userReleaseRoleRepository;
        _userPublicationRoleRepository = userPublicationRoleRepository;
        _preReleaseService = preReleaseService;
    }

    public Task<bool> HasRolesOnPublicationOrReleaseVersion(
        Guid userId,
        Guid publicationId,
        Guid releaseVersionId,
        IEnumerable<PublicationRole> publicationRoles,
        IEnumerable<ReleaseRole> releaseRoles)
    {
        return HasRolesOnPublicationOrReleaseVersion(
            userId,
            publicationId,
            () => Task.FromResult((Guid?) releaseVersionId),
            publicationRoles,
            releaseRoles);
    }

    public async Task<bool> HasRolesOnPublicationOrReleaseVersion(
        Guid userId,
        Guid publicationId,
        Func<Task<Guid?>> releaseVersionIdSupplier,
        IEnumerable<PublicationRole> publicationRoles,
        IEnumerable<ReleaseRole> releaseRoles)
    {
        var usersPublicationRoles = await _userPublicationRoleRepository
            .GetAllRolesByUserAndPublication(userId, publicationId);

        if (usersPublicationRoles.Any(publicationRoles.Contains))
        {
            return true;
        }

        var releaseVersionId = await releaseVersionIdSupplier.Invoke();

        if (releaseVersionId == null)
        {
            return false;
        }

        var usersReleaseRoles = await _userReleaseRoleRepository
            .GetAllRolesByUserAndRelease(userId: userId,
                releaseVersionId: releaseVersionId.Value);

        return usersReleaseRoles.Any(releaseRoles.Contains);
    }

    public async Task<bool> HasRolesOnPublicationOrAnyReleaseVersion(
        Guid userId,
        Guid publicationId,
        IEnumerable<PublicationRole> publicationRoles,
        IEnumerable<ReleaseRole> releaseRoles)
    {
        var usersPublicationRoles = await _userPublicationRoleRepository
            .GetAllRolesByUserAndPublication(userId, publicationId);

        if (usersPublicationRoles.Any(publicationRoles.Contains))
        {
            return true;
        }

        var usersReleaseRoles = await _userReleaseRoleRepository
            .GetAllRolesByUserAndPublication(userId, publicationId);

        return usersReleaseRoles.Any(releaseRoles.Contains);
    }

    public async Task<bool> HasRolesOnPublication(
        Guid userId,
        Guid publicationId,
        params PublicationRole[] publicationRoles)
    {
        var usersPublicationRoles = await _userPublicationRoleRepository
            .GetAllRolesByUserAndPublication(userId, publicationId);

        return usersPublicationRoles.Any(publicationRoles.Contains);
    }

    public async Task<bool> HasRolesOnReleaseVersion(
        Guid userId,
        Guid releaseVersionId,
        params ReleaseRole[] releaseRoles)
    {
        var usersReleaseRoles = await _userReleaseRoleRepository
            .GetAllRolesByUserAndRelease(userId: userId,
                releaseVersionId: releaseVersionId);

        return usersReleaseRoles.Any(releaseRoles.Contains);
    }

    public async Task<bool> IsReleaseVersionViewableByUser(ReleaseVersion releaseVersion, ClaimsPrincipal user)
    {
        // If the user has the "AccessAllReleases" Claim, they can see any release version.
        if (SecurityUtils.HasClaim(user, SecurityClaimTypes.AccessAllReleases))
        {
            return true;
        }

        // If the user has any PublicationRoles on the owning Publication, they can see its child release versions.
        if (await HasRolesOnPublication(
                userId: user.GetUserId(),
                publicationId: releaseVersion.PublicationId,
                EnumUtil.GetEnumsArray<PublicationRole>()))
        {
            return true;
        }

        // If the user has any non-Pre-release Viewer roles on the Release, they can see it at any time.
        if (await HasRolesOnReleaseVersion(
                    userId: user.GetUserId(),
                    releaseVersionId: releaseVersion.Id,
                    UnrestrictedReleaseViewerRoles))
        {
            return true;
        }

        // If the user has the Pre-release Viewer role on this Release and the Release is within its open
        // Pre-release window, they can see the release version.
        if (await HasRolesOnReleaseVersion(
                    userId: user.GetUserId(),
                    releaseVersionId: releaseVersion.Id,
                    ReleaseRole.PrereleaseViewer))
        {
            var windowStatus = _preReleaseService.GetPreReleaseWindowStatus(releaseVersion, DateTime.UtcNow);
            if (windowStatus.Access == PreReleaseAccess.Within)
            {
                return true;
            }
        }

        // If the release version is public, anyone can see it.
        return await _releaseVersionRepository.IsLatestPublishedReleaseVersion(releaseVersion.Id);
    }
}
