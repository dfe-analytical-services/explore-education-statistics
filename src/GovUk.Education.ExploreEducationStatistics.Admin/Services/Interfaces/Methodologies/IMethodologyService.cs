#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Requests;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.Methodology;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Methodologies;

public interface IMethodologyService
{
    Task<Either<ActionResult, Unit>> AdoptMethodology(Guid publicationId, Guid methodologyId);

    Task<Either<ActionResult, MethodologyVersionViewModel>> CreateMethodology(Guid publicationId);

    Task<Either<ActionResult, Unit>> DeleteMethodology(
        Guid methodologyId,
        bool forceDelete = false
    );

    Task<Either<ActionResult, Unit>> DeleteMethodologyVersion(
        Guid methodologyVersionId,
        bool forceDelete = false
    );

    Task<Either<ActionResult, Unit>> DropMethodology(Guid publicationId, Guid methodologyId);

    Task<Either<ActionResult, List<MethodologyVersionViewModel>>> GetAdoptableMethodologies(
        Guid publicationId
    );

    Task<Either<ActionResult, MethodologyVersionViewModel>> GetMethodology(
        Guid methodologyVersionId
    );

    Task<
        Either<ActionResult, List<MethodologyVersionSummaryViewModel>>
    > ListLatestMethodologyVersions(Guid publicationId, bool isPrerelease = false);

    Task<Either<ActionResult, List<IdTitleViewModel>>> GetUnpublishedReleasesUsingMethodology(
        Guid methodologyVersionId
    );

    Task<Either<ActionResult, MethodologyVersionViewModel>> UpdateMethodology(
        Guid methodologyVersionId,
        MethodologyUpdateRequest request
    );

    Task<Either<ActionResult, Unit>> UpdateMethodologyPublished(
        Guid methodologyVersionId,
        MethodologyPublishedUpdateRequest request
    );

    Task<MethodologyVersionViewModel> BuildMethodologyVersionViewModel(
        MethodologyVersion methodologyVersion
    );

    Task<Either<ActionResult, List<MethodologyStatusViewModel>>> GetMethodologyStatuses(
        Guid methodologyVersionId
    );

    Task<
        Either<ActionResult, List<MethodologyVersionViewModel>>
    > ListUsersMethodologyVersionsForApproval();

    Task PublicationTitleOrSlugChanged(
        Guid publicationId,
        string originalSlug,
        string updatedTitle,
        string updatedSlug
    );

    Task<Either<ActionResult, Unit>> ValidateMethodologySlug(
        string newSlug,
        string? oldSlug = null,
        Guid? methodologyId = null
    );
}
