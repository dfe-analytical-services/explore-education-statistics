#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces
{
    public interface IReleasePermissionService
    {
        Task<Either<ActionResult, List<UserReleaseRoleSummaryViewModel>>>
            ListReleaseRoles(Guid releaseId, ReleaseRole[]? rolesToInclude);

        Task<Either<ActionResult, List<UserReleaseInviteViewModel>>>
            ListReleaseInvites(Guid releaseId, ReleaseRole[]? rolesToInclude = null);

        Task<Either<ActionResult, List<UserReleaseRoleSummaryViewModel>>>
            ListPublicationContributors(Guid releaseId);

        Task<Either<ActionResult, Unit>> UpdateReleaseContributors(Guid releaseId,
            List<Guid> userIds);

        Task<Either<ActionResult, Unit>> RemoveAllUserContributorPermissionsForPublication(
            Guid publicationId, Guid userId);
    }
}
