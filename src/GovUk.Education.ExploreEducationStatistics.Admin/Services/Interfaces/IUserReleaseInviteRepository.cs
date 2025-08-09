#nullable enable
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Content.Model;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;

public interface IUserReleaseInviteRepository
{
    Task Create(Guid releaseVersionId,
        string email,
        ReleaseRole releaseRole,
        bool emailSent,
        Guid createdById,
        DateTime? createdDate = null);

    Task CreateManyIfNotExists(List<Guid> releaseVersionIds,
        string email,
        ReleaseRole releaseRole,
        bool emailSent,
        Guid createdById,
        DateTime? createdDate = null);

    Task<bool> UserHasInvite(Guid releaseVersionId,
        string email,
        ReleaseRole role);

    Task<bool> UserHasInvites(List<Guid> releaseVersionIds,
        string email,
        ReleaseRole role);

    Task<List<UserReleaseInvite>> GetInvitesByEmail(string email);

    Task Remove(
        Guid releaseVersionId,
        string email,
        ReleaseRole role,
        CancellationToken cancellationToken = default);

    Task RemoveMany(
        IReadOnlyList<UserReleaseInvite> userReleaseInvites,
        CancellationToken cancellationToken = default);

    Task RemoveByPublication(
        Guid publicationId,
        string? email = null,
        CancellationToken cancellationToken = default,
        params ReleaseRole[] rolesToInclude);

    Task RemoveByReleaseVersion(
        Guid releaseVersionId,
        string? email = null,
        CancellationToken cancellationToken = default,
        params ReleaseRole[] rolesToInclude);

    Task RemoveByUserEmail(
        string email,
        CancellationToken cancellationToken = default);
}
