using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Requests;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces
{
    public interface IPublicationService
    {
        Task<Either<ActionResult, List<PublicationViewModel>>> ListPublications(Guid? topicId);

        Task<Either<ActionResult, List<PublicationSummaryViewModel>>> ListPublicationSummaries();

        Task<Either<ActionResult, PublicationCreateViewModel>> CreatePublication(
            PublicationCreateRequest publication);

        Task<Either<ActionResult, PublicationViewModel>> UpdatePublication(
            Guid publicationId,
            PublicationSaveRequest updatedPublication);

        Task<Either<ActionResult, PublicationViewModel>> GetPublication(Guid publicationId, bool includePermissions);

        Task<Either<ActionResult, ExternalMethodologyViewModel>> GetExternalMethodology(Guid publicationId);

        Task<Either<ActionResult, ExternalMethodologyViewModel>> UpdateExternalMethodology(
            Guid publicationId, ExternalMethodologySaveRequest updatedExternalMethodology);

        Task<Either<ActionResult, Unit>> RemoveExternalMethodology(
            Guid publicationId);

        Task<Either<ActionResult, ContactViewModel>> GetContact(Guid publicationId);

        Task<Either<ActionResult, ContactViewModel>> UpdateContact(Guid publicationId, Contact updatedContact);

        Task<Either<ActionResult, PaginatedListViewModel<ReleaseSummaryViewModel>>> ListActiveReleasesPaginated(
            Guid publicationId,
            int page,
            int pageSize,
            bool? live = null,
            bool includePermissions = false);

        Task<Either<ActionResult, List<ReleaseSummaryViewModel>>> ListActiveReleases(
            Guid publicationId,
            bool? live = null,
            bool includePermissions = false);

        Task<Either<ActionResult, List<LegacyReleaseViewModel>>> PartialUpdateLegacyReleases(
            Guid publicationId,
            List<LegacyReleasePartialUpdateViewModel> updatedLegacyReleases);
    }
}
