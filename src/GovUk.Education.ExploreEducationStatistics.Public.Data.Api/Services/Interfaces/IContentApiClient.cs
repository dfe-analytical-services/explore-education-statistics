using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Services.Interfaces;

public interface IContentApiClient
{
    Task<Either<ActionResult, PublishedPublicationSummaryViewModel>> GetPublication(
        Guid publicationId,
        CancellationToken cancellationToken = default
    );
}
