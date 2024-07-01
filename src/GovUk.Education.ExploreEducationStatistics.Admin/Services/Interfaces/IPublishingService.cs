#nullable enable
using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Publisher.Model;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;

public interface IPublishingService
{
    Task<Either<ActionResult, Unit>> RetryReleasePublishing(Guid releaseVersionId);

    Task<Either<ActionResult, Unit>> ReleaseChanged(ReleasePublishingKey releasePublishingKey, bool immediate = false);

    Task<Either<ActionResult, Unit>> PublishMethodologyFiles(Guid methodologyVersionId);

    Task<Either<ActionResult, Unit>> TaxonomyChanged();
}
