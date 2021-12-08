#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces
{
    public interface IUserReleaseInviteRepository
    {
        Task<Either<ActionResult, Unit>> Create(Guid releaseId, string email, ReleaseRole releaseRole,
            bool emailSent, Guid createdById, bool accepted = false);

        Task<Either<ActionResult, Unit>> CreateManyIfNotExists(List<Guid> releaseIds, string email,
            ReleaseRole releaseRole, bool emailSent, Guid createdById, bool accepted = false);

        Task MarkEmailAsSent(UserReleaseInvite invite);

        Task<bool> UserHasInvite(Guid releaseId, string email, ReleaseRole role);

        Task<bool> UserHasInvites(List<Guid> releaseIds, string email, ReleaseRole role);

        Task RemoveMany(List<Guid> releaseIds, string email, ReleaseRole role);
    }
}
