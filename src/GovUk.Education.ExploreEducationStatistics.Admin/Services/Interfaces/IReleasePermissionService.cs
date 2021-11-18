#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces
{
    public interface IReleasePermissionService
    {
        Task<Either<ActionResult, List<ContributorViewModel>>>
            GetReleaseContributorPermissions(Guid publicationId, Guid releaseId);

        Task<Either<ActionResult, List<ContributorViewModel>>>
            GetPublicationContributorList(Guid releaseId);

        Task<Either<ActionResult, Unit>> UpdateReleaseContributors(Guid releaseId,
            List<Guid> userIds);

        Task<Either<ActionResult, Unit>> RemoveAllUserContributorPermissionsForPublication(
            Guid publicationId, Guid userId);
    }
}
