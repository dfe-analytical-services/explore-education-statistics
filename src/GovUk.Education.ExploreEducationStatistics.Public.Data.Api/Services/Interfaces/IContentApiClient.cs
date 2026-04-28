using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Publications.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Services.Interfaces;

public interface IContentApiClient
{
    Task<Either<ActionResult, PublicationSummaryDto>> GetPublicationSummary(
        Guid publicationId,
        CancellationToken cancellationToken = default
    );
}
