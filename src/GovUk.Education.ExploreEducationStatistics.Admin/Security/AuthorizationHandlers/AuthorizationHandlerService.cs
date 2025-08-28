#nullable enable
using System.Security.Claims;
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using IReleaseVersionRepository =
    GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces.IReleaseVersionRepository;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers;

public class AuthorizationHandlerService(
    IReleaseVersionRepository releaseVersionRepository,
    IUserReleaseRoleRepository userReleaseRoleRepository,
    IUserPublicationRoleRepository userPublicationRoleRepository,
    IPreReleaseService preReleaseService)
{
    private static readonly ReleaseRole[] ReleaseEditorRoles =
    {
        ReleaseRole.Contributor
    };

    public static readonly ReleaseRole[] UnrestrictedReleaseViewerRoles =
    {
        ReleaseRole.Contributor,
        ReleaseRole.Approver
    };

    public static readonly List<ReleaseRole> ReleaseEditorAndApproverRoles =
        ReleaseEditorRoles
            .Append(ReleaseRole.Approver)
            .ToList();

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
            () => Task.FromResult((Guid?)releaseVersionId),
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
        var usersPublicationRoles = await userPublicationRoleRepository
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

        var usersReleaseRoles = await userReleaseRoleRepository
            .GetAllRolesByUserAndReleaseVersion(userId: userId,
                releaseVersionId: releaseVersionId.Value);

        return usersReleaseRoles.Any(releaseRoles.Contains);
    }

    public async Task<bool> HasRolesOnPublicationOrAnyReleaseVersion(
        Guid userId,
        Guid publicationId,
        IEnumerable<PublicationRole> publicationRoles,
        IEnumerable<ReleaseRole> releaseRoles)
    {
        var usersPublicationRoles = await userPublicationRoleRepository
            .GetAllRolesByUserAndPublication(userId, publicationId);

        if (usersPublicationRoles.Any(publicationRoles.Contains))
        {
            return true;
        }

        var usersReleaseRoles = await userReleaseRoleRepository
            .GetAllRolesByUserAndPublication(userId, publicationId);

        return usersReleaseRoles.Any(releaseRoles.Contains);
    }

    public async Task<bool> HasRolesOnPublication(
        Guid userId,
        Guid publicationId,
        params PublicationRole[] publicationRoles)
    {
        var usersPublicationRoles = await userPublicationRoleRepository
            .GetAllRolesByUserAndPublication(userId, publicationId);

        return usersPublicationRoles.Any(publicationRoles.Contains);
    }

    public async Task<bool> HasRolesOnReleaseVersion(
        Guid userId,
        Guid releaseVersionId,
        params ReleaseRole[] releaseRoles)
    {
        var usersReleaseRoles = await userReleaseRoleRepository
            .GetAllRolesByUserAndReleaseVersion(userId: userId,
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

        // This will be changed when we start introducing the use of the NEW publication roles in the 
        // authorisation handlers, in STEP 8 (EES-6194) of the Permissions Rework. For now, we want to
        // filter out any usage of the NEW roles.
        var validPublicationRoles = EnumUtil.GetEnums<PublicationRole>()
            .Except([PublicationRole.Approver, PublicationRole.Drafter]);

        // If the user has any PublicationRoles on the owning Publication, they can see its child release versions.
        if (await HasRolesOnPublication(
                userId: user.GetUserId(),
                publicationId: releaseVersion.PublicationId,
                [.. validPublicationRoles]))
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
            var windowStatus = preReleaseService.GetPreReleaseWindowStatus(releaseVersion, DateTime.UtcNow);
            if (windowStatus.Access == PreReleaseAccess.Within)
            {
                return true;
            }
        }

        // If the release version is public, anyone can see it.
        return await releaseVersionRepository.IsLatestPublishedReleaseVersion(releaseVersion.Id);
    }
}
