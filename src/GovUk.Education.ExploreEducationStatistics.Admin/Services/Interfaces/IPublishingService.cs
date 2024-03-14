#nullable enable
using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;

public interface IPublishingService
{
    Task<Either<ActionResult, Unit>> RetryReleasePublishing(Guid releaseVersionId);

    Task<Either<ActionResult, Unit>> ReleaseChanged(Guid releaseVersionId,
        Guid releaseStatusId,
        bool immediate = false);

    Task<Either<ActionResult, Unit>> PublishMethodologyFiles(Guid methodologyVersionId);

    Task<Either<ActionResult, Unit>> TaxonomyChanged();
}
