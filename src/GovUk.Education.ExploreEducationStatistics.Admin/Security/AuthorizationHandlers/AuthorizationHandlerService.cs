#nullable enable
using System.Security.Claims;
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Microsoft.EntityFrameworkCore;
using IReleaseVersionRepository = GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces.IReleaseVersionRepository;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers;

public class AuthorizationHandlerService(
    IReleaseVersionRepository releaseVersionRepository,
    IUserReleaseRoleRepository userReleaseRoleRepository,
    IUserPublicationRoleRepository userPublicationRoleRepository,
    IPreReleaseService preReleaseService
)
{
    private static readonly HashSet<ReleaseRole> ReleaseEditorRoles = [ReleaseRole.Contributor];

    public static readonly HashSet<ReleaseRole> UnrestrictedReleaseViewerRoles =
    [
        ReleaseRole.Contributor,
        ReleaseRole.Approver,
    ];

    public static readonly HashSet<ReleaseRole> ReleaseEditorAndApproverRoles =
    [
        .. ReleaseEditorRoles,
        ReleaseRole.Approver,
    ];

    public Task<bool> HasRolesOnPublicationOrReleaseVersion(
        Guid userId,
        Guid publicationId,
        Guid releaseVersionId,
        HashSet<PublicationRole> publicationRoles,
        HashSet<ReleaseRole> releaseRoles
    )
    {
        return HasRolesOnPublicationOrReleaseVersion(
            userId,
            publicationId,
            () => Task.FromResult((Guid?)releaseVersionId),
            publicationRoles,
            releaseRoles
        );
    }

    public async Task<bool> HasRolesOnPublicationOrReleaseVersion(
        Guid userId,
        Guid publicationId,
        Func<Task<Guid?>> releaseVersionIdSupplier,
        HashSet<PublicationRole> publicationRoles,
        HashSet<ReleaseRole> releaseRoles
    )
    {
        if (
            await UserHasAnyPublicationRoleOnPublication(
                userId: userId,
                publicationId: publicationId,
                rolesToInclude: [.. publicationRoles]
            )
        )
        {
            return true;
        }

        var releaseVersionId = await releaseVersionIdSupplier.Invoke();

        if (releaseVersionId == null)
        {
            return false;
        }

        return await UserHasAnyReleaseRoleOnReleaseVersion(
            userId: userId,
            releaseVersionId: releaseVersionId.Value,
            rolesToInclude: [.. releaseRoles]
        );
    }

    public async Task<bool> UserHasAnyRoleOnPublicationOrAnyReleaseVersion(
        Guid userId,
        Guid publicationId,
        HashSet<PublicationRole> publicationRoles,
        HashSet<ReleaseRole> releaseRoles
    )
    {
        if (
            await UserHasAnyPublicationRoleOnPublication(
                userId: userId,
                publicationId: publicationId,
                rolesToInclude: [.. publicationRoles]
            )
        )
        {
            return true;
        }

        return await UserHasAnyReleaseRoleOnPublication(
            userId: userId,
            publicationId: publicationId,
            rolesToInclude: [.. releaseRoles]
        );
    }

    public async Task<bool> UserHasAnyPublicationRoleOnPublication(
        Guid userId,
        Guid publicationId,
        params PublicationRole[] rolesToInclude
    ) =>
        await userPublicationRoleRepository.UserHasAnyRoleOnPublication(
            userId: userId,
            publicationId: publicationId,
            rolesToInclude: [.. rolesToInclude]
        );

    public async Task<bool> UserHasAnyReleaseRoleOnReleaseVersion(
        Guid userId,
        Guid releaseVersionId,
        params ReleaseRole[] rolesToInclude
    ) =>
        await userReleaseRoleRepository.UserHasAnyRoleOnReleaseVersion(
            userId: userId,
            releaseVersionId: releaseVersionId,
            rolesToInclude: [.. rolesToInclude]
        );

    public async Task<bool> UserHasAnyReleaseRoleOnPublication(
        Guid userId,
        Guid publicationId,
        params ReleaseRole[] rolesToInclude
    ) =>
        await userReleaseRoleRepository.UserHasAnyRoleOnPublication(
            userId: userId,
            publicationId: publicationId,
            rolesToInclude: [.. rolesToInclude]
        );

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
        var validPublicationRoles = EnumUtil
            .GetEnums<PublicationRole>()
            .Except([PublicationRole.Approver, PublicationRole.Drafter]);

        // If the user has any PublicationRoles on the owning Publication, they can see its child release versions.
        if (
            await UserHasAnyPublicationRoleOnPublication(
                userId: user.GetUserId(),
                publicationId: releaseVersion.PublicationId,
                [.. validPublicationRoles]
            )
        )
        {
            return true;
        }

        // If the user has any non-Pre-release Viewer roles on the Release, they can see it at any time.
        if (
            await UserHasAnyReleaseRoleOnReleaseVersion(
                userId: user.GetUserId(),
                releaseVersionId: releaseVersion.Id,
                [.. UnrestrictedReleaseViewerRoles]
            )
        )
        {
            return true;
        }

        // If the user has the Pre-release Viewer role on this Release and the Release is within its open
        // Pre-release window, they can see the release version.
        if (
            await UserHasAnyReleaseRoleOnReleaseVersion(
                userId: user.GetUserId(),
                releaseVersionId: releaseVersion.Id,
                ReleaseRole.PrereleaseViewer
            )
        )
        {
            var windowStatus = preReleaseService.GetPreReleaseWindowStatus(releaseVersion, DateTimeOffset.UtcNow);
            if (windowStatus.Access == PreReleaseAccess.Within)
            {
                return true;
            }
        }

        // If the release version is public, anyone can see it.
        return await releaseVersionRepository.IsLatestPublishedReleaseVersion(releaseVersion.Id);
    }
}
