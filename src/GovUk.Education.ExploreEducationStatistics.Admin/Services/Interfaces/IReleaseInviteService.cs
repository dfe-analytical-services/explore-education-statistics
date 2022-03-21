#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces
{
    public interface IReleaseInviteService
    {
        Task<Either<ActionResult, Unit>> InviteContributor(string email, Guid publicationId, List<Guid> releaseIds);

        Task<Either<ActionResult, Unit>> RemoveByPublication(string email, Guid publicationId, ReleaseRole releaseRole);
    }
}
