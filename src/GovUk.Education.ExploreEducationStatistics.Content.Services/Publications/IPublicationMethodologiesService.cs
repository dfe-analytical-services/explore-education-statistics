using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Publications.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.Publications;

public interface IPublicationMethodologiesService
{
    Task<Either<ActionResult, PublicationMethodologiesDto>> GetPublicationMethodologies(
        string publicationSlug,
        CancellationToken cancellationToken = default
    );
}
