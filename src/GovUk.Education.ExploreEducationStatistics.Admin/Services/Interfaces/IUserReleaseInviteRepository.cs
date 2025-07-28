#nullable enable
using System;
using System.Collections.Generic;
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

    Task Remove(Guid releaseVersionId,
        string email,
        ReleaseRole role);

    Task<bool> UserHasInvite(Guid releaseVersionId,
        string email,
        ReleaseRole role);

    Task<bool> UserHasInvites(List<Guid> releaseVersionIds,
        string email,
        ReleaseRole role);

    Task RemoveByPublication(Publication publication,
        string email,
        ReleaseRole role);

    Task<List<UserReleaseInvite>> ListByEmail(string email);
}
