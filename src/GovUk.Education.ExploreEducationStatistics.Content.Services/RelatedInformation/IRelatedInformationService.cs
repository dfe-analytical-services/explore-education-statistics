using CSharpFunctionalExtensions;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Errors;
using GovUk.Education.ExploreEducationStatistics.Content.Services.RelatedInformation.Dtos;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.RelatedInformation;

public interface IRelatedInformationService
{
    Task<Result<RelatedInformationDto[], ResourceNotFoundError>> GetRelatedInformationForRelease(
        string publicationSlug,
        string releaseSlug,
        CancellationToken cancellationToken = default
    );
}
