#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Services.RelatedInformation.Dtos;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.ManageContent;

public interface IRelatedInformationService
{
    Task<Either<ActionResult, List<RelatedInformationDto>>> CreateRelatedInformation(
        Guid releaseVersionId,
        string title,
        string url,
        CancellationToken cancellationToken = default
    );

    Task<Either<ActionResult, List<RelatedInformationDto>>> GetRelatedInformation(
        Guid releaseVersionId,
        CancellationToken cancellationToken = default
    );

    Task<Either<ActionResult, List<RelatedInformationDto>>> UpdateRelatedInformation(
        Guid releaseVersionId,
        string title,
        string url,
        CancellationToken cancellationToken = default
    );

    Task<Either<ActionResult, List<RelatedInformationDto>>> DeleteRelatedInformation(
        Guid releaseVersionId,
        Guid relatedInformationId,
        CancellationToken cancellationToken = default
    );
}
