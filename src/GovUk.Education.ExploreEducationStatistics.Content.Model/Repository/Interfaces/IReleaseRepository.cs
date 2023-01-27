#nullable enable
using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;

public interface IReleaseRepository
{
    Task<DateTime> GetPublishedDate(Guid releaseId,
        DateTime actualPublishedDate);

    Task<Either<ActionResult, Release>> GetLatestPublishedRelease(Guid publicationId);

    Task<bool> IsLatestPublishedVersionOfRelease(Guid releaseId);
}
