#nullable enable
using System.Security.Claims;
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Security;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using IReleaseVersionRepository = GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces.IReleaseVersionRepository;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers;

public class AuthorizationHandlerService(
    IReleaseVersionRepository releaseVersionRepository,
    IUserReleaseRoleRepository userReleaseRoleRepository,
    IUserPublicationRoleRepository userPublicationRoleRepository,
    IPreReleaseService preReleaseService
) : IAuthorizationHandlerService
{
    public async Task<bool> UserHasAnyPublicationRoleOnPublication(
        Guid userId,
        Guid publicationId,
        HashSet<PublicationRole> rolesToInclude
    )
    {
        if (!rolesToInclude.Any())
        {
            throw new ArgumentException("At least one publication role must be included in the check.");
        }

        return await userPublicationRoleRepository.UserHasAnyRoleOnPublication(
            userId: userId,
            publicationId: publicationId,
            rolesToInclude: [.. rolesToInclude]
        );
    }

    public async Task<bool> UserHasAnyRoleOnPublication(Guid userId, Guid publicationId) =>
        await userPublicationRoleRepository.UserHasAnyRoleOnPublication(userId: userId, publicationId: publicationId)
        || await userReleaseRoleRepository.UserHasAnyRoleOnPublication(userId: userId, publicationId: publicationId);

    public async Task<bool> UserHasPrereleaseRoleOnReleaseVersion(Guid userId, Guid releaseVersionId) =>
        await userReleaseRoleRepository.UserHasRoleOnReleaseVersion(
            userId: userId,
            releaseVersionId: releaseVersionId,
            ReleaseRole.PrereleaseViewer
        );

    public async Task<bool> IsReleaseVersionViewableByUser(ReleaseVersion releaseVersion, ClaimsPrincipal user)
    {
        // If the user has the "AccessAllReleases" Claim, they can see any release version.
        if (SecurityUtils.HasClaim(user, SecurityClaimTypes.AccessAllReleases))
        {
            return true;
        }

        // If the user has either Drafter or Approver publication roles on the owning Publication, they can see its child release versions.
        if (
            await UserHasAnyPublicationRoleOnPublication(
                userId: user.GetUserId(),
                publicationId: releaseVersion.Release.PublicationId,
                rolesToInclude: [PublicationRole.Drafter, PublicationRole.Approver]
            )
        )
        {
            return true;
        }

        var a = await UserHasPrereleaseRoleOnReleaseVersion(
            userId: user.GetUserId(),
            releaseVersionId: releaseVersion.Id
        );

        var b =
            preReleaseService.GetPreReleaseWindowStatus(releaseVersion, DateTimeOffset.UtcNow).Access
            == PreReleaseAccess.Within;

        // If the user has the Pre-release Viewer role on this Release and the Release is within its open
        // Pre-release window, they can see the release version.
        if (
            await UserHasPrereleaseRoleOnReleaseVersion(userId: user.GetUserId(), releaseVersionId: releaseVersion.Id)
            && preReleaseService.GetPreReleaseWindowStatus(releaseVersion, DateTimeOffset.UtcNow).Access
                == PreReleaseAccess.Within
        )
        {
            return true;
        }

        // If the release version is public, anyone can see it.
        return await releaseVersionRepository.IsLatestPublishedReleaseVersion(releaseVersion.Id);
    }
}
