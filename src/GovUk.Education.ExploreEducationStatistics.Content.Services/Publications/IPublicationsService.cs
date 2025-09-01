using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Publications.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.Publications;

public interface IPublicationsService
{
    Task<Either<ActionResult, PublicationDto>> GetPublication(
        string publicationSlug,
        CancellationToken cancellationToken = default);
}
