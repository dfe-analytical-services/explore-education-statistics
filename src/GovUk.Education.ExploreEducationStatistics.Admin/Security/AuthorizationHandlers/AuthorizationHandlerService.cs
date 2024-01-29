#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using IReleaseRepository =
    GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces.IReleaseRepository;

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

    private readonly IReleaseRepository _releaseRepository;
    private readonly IUserReleaseRoleRepository _userReleaseRoleRepository;
    private readonly IUserPublicationRoleRepository _userPublicationRoleRepository;
    private readonly IPreReleaseService _preReleaseService;

    public AuthorizationHandlerService(
        IReleaseRepository releaseRepository,
        IUserReleaseRoleRepository userReleaseRoleRepository,
        IUserPublicationRoleRepository userPublicationRoleRepository,
        IPreReleaseService preReleaseService)
    {
        _releaseRepository = releaseRepository;
        _userReleaseRoleRepository = userReleaseRoleRepository;
        _userPublicationRoleRepository = userPublicationRoleRepository;
        _preReleaseService = preReleaseService;
    }

    public Task<bool> HasRolesOnPublicationOrRelease(
        Guid userId,
        Guid publicationId,
        Guid releaseId,
        IEnumerable<PublicationRole> publicationRoles,
        IEnumerable<ReleaseRole> releaseRoles)
    {
        return HasRolesOnPublicationOrRelease(
            userId,
            publicationId,
            () => Task.FromResult((Guid?) releaseId),
            publicationRoles,
            releaseRoles);
    }

    public async Task<bool> HasRolesOnPublicationOrRelease(
        Guid userId,
        Guid publicationId,
        Func<Task<Guid?>> releaseIdSupplier,
        IEnumerable<PublicationRole> publicationRoles,
        IEnumerable<ReleaseRole> releaseRoles)
    {
        var usersPublicationRoles = await _userPublicationRoleRepository
            .GetAllRolesByUserAndPublication(userId, publicationId);

        if (usersPublicationRoles.Any(publicationRoles.Contains))
        {
            return true;
        }

        var releaseId = await releaseIdSupplier.Invoke();

        if (releaseId == null)
        {
            return false;
        }

        var usersReleaseRoles = await _userReleaseRoleRepository
            .GetAllRolesByUserAndRelease(userId, releaseId.Value);

        return usersReleaseRoles.Any(releaseRoles.Contains);
    }

    public async Task<bool> HasRolesOnPublicationOrAnyRelease(
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

    public async Task<bool> HasRolesOnRelease(
        Guid userId,
        Guid releaseId,
        params ReleaseRole[] releaseRoles)
    {
        var usersReleaseRoles = await _userReleaseRoleRepository
            .GetAllRolesByUserAndRelease(userId, releaseId);

        return usersReleaseRoles.Any(releaseRoles.Contains);
    }

    public async Task<bool> IsReleaseViewableByUser(Release release, ClaimsPrincipal user)
    {
        // If the user has the "AccessAllReleases" Claim, they can see any Release.
        if (SecurityUtils.HasClaim(user, SecurityClaimTypes.AccessAllReleases))
        {
            return true;
        }

        // If the user has any PublicationRoles on the owning Publication, they can see its child Releases.
        if (await HasRolesOnPublication(
                    user.GetUserId(),
                    release.PublicationId,
                    EnumUtil.GetEnumValuesAsArray<PublicationRole>()))
        {
            return true;
        }

        // If the user has any non-Pre-release Viewer roles on the Release, they can see it at any time.
        if (await HasRolesOnRelease(
                    user.GetUserId(),
                    release.Id,
                    UnrestrictedReleaseViewerRoles))
        {
            return true;
        }

        // If the user has the Pre-release Viewer role on this Release and the Release is within its open
        // Pre-release window, they can see the Release.
        if (await HasRolesOnRelease(
                    user.GetUserId(),
                    release.Id,
                    ReleaseRole.PrereleaseViewer))
        {
            var windowStatus = _preReleaseService.GetPreReleaseWindowStatus(release, DateTime.UtcNow);
            if (windowStatus.Access is PreReleaseAccess.Within or PreReleaseAccess.WithinPublishDayLenience)
            {
                return true;
            }
        }

        // If the Release is public, anyone can see it.
        return await _releaseRepository.IsLatestPublishedReleaseVersion(release.Id);
    }
}
