using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Services.Interfaces;

public interface IPublicationService
{
    Task<Either<ActionResult, PaginatedPublicationListViewModel>> ListPublications(
        int page,
        int pageSize, 
        string? search = null);

    Task<Either<ActionResult, PublicationSummaryViewModel>> GetPublication(Guid publicationId);
}
