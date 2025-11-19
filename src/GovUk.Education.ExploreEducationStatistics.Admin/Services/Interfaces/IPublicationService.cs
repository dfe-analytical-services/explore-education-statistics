using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using GovUk.Education.ExploreEducationStatistics.Admin.Requests;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Content.ViewModels;
using Microsoft.AspNetCore.Mvc;
using ExternalMethodologyViewModel = GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.ExternalMethodologyViewModel;
using PublicationViewModel = GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.PublicationViewModel;
using ReleaseVersionSummaryViewModel = GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.ReleaseVersionSummaryViewModel;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;

public interface IPublicationService
{
    Task<Either<ActionResult, List<PublicationViewModel>>> ListPublications(
        Guid? themeId = null,
        CancellationToken cancellationToken = default
    );

    Task<Either<ActionResult, List<PublicationSummaryViewModel>>> ListPublicationSummaries();

    Task<Either<ActionResult, PublicationCreateViewModel>> CreatePublication(PublicationCreateRequest publication);

    Task<Either<ActionResult, PublicationViewModel>> UpdatePublication(
        Guid publicationId,
        PublicationSaveRequest updatedPublication
    );

    Task<Either<ActionResult, PublicationViewModel>> GetPublication(
        Guid publicationId,
        bool includePermissions = false,
        CancellationToken cancellationToken = default
    );

    Task<Either<ActionResult, ExternalMethodologyViewModel>> GetExternalMethodology(Guid publicationId);

    Task<Either<ActionResult, ExternalMethodologyViewModel>> UpdateExternalMethodology(
        Guid publicationId,
        ExternalMethodologySaveRequest updatedExternalMethodology
    );

    Task<Either<ActionResult, Unit>> RemoveExternalMethodology(Guid publicationId);

    Task<Either<ActionResult, ContactViewModel>> GetContact(Guid publicationId);

    Task<Either<ActionResult, ContactViewModel>> UpdateContact(Guid publicationId, ContactSaveRequest updatedContact);

    Task<Either<ActionResult, PaginatedListViewModel<ReleaseVersionSummaryViewModel>>> ListReleaseVersionsPaginated(
        Guid publicationId,
        ReleaseVersionsType versionsType,
        int page,
        int pageSize,
        bool includePermissions = false
    );

    Task<Either<ActionResult, List<ReleaseVersionSummaryViewModel>>> ListReleaseVersions(
        Guid publicationId,
        ReleaseVersionsType versionsType,
        bool includePermissions = false
    );

    Task<Either<ActionResult, List<ReleaseSeriesTableEntryViewModel>>> GetReleaseSeries(Guid publicationId);

    Task<Either<ActionResult, List<ReleaseSeriesTableEntryViewModel>>> AddReleaseSeriesLegacyLink(
        Guid publicationId,
        ReleaseSeriesLegacyLinkAddRequest newLegacyLink
    );

    Task<Either<ActionResult, List<ReleaseSeriesTableEntryViewModel>>> UpdateReleaseSeries(
        Guid publicationId,
        List<ReleaseSeriesItemUpdateRequest> updatedReleaseSeriesItems
    );
}
