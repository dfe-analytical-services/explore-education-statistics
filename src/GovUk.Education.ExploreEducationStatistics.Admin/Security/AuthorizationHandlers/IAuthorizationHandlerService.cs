#nullable enable
using System.Security.Claims;
using GovUk.Education.ExploreEducationStatistics.Content.Model;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers;

public interface IAuthorizationHandlerService
{
    Task<bool> UserHasAnyPublicationRoleOnPublication(
        Guid userId,
        Guid publicationId,
        HashSet<PublicationRole> rolesToInclude
    );

    Task<bool> UserHasAnyRoleOnPublication(Guid userId, Guid publicationId);

    Task<bool> UserHasPrereleaseRoleOnReleaseVersion(Guid userId, Guid releaseVersionId);

    Task<bool> IsReleaseVersionViewableByUser(ReleaseVersion releaseVersion, ClaimsPrincipal user);
}
