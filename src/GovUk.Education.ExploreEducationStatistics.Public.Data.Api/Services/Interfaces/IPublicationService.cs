using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Services.Interfaces;

public interface IPublicationService
{
    Task<Either<ActionResult, PublicationPaginatedListViewModel>> ListPublications(
        int page,
        int pageSize,
        string? search = null,
        CancellationToken cancellationToken = default
    );

    Task<Either<ActionResult, PublicationSummaryViewModel>> GetPublication(
        Guid publicationId,
        CancellationToken cancellationToken = default
    );
}
