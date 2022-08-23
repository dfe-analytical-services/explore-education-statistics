using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Content.Services.ViewModels;
using Microsoft.AspNetCore.Mvc;
using LegacyReleaseViewModel = GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.LegacyReleaseViewModel;
using PublicationViewModel = GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.PublicationViewModel;
using ReleaseSummaryViewModel = GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.ReleaseSummaryViewModel;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces
{
    public interface IPublicationService
    {
        Task<Either<ActionResult, List<MyPublicationViewModel>>> GetMyPublicationsAndReleasesByTopic(Guid topicId);

        Task<Either<ActionResult, MyPublicationViewModel>> GetMyPublication(Guid publicationId);

        Task<Either<ActionResult, List<PublicationSummaryViewModel>>> ListPublicationSummaries();

        Task<Either<ActionResult, PublicationViewModel>> CreatePublication(
            PublicationSaveViewModel publication);

        Task<Either<ActionResult, PublicationViewModel>> UpdatePublication(
            Guid publicationId,
            PublicationSaveViewModel updatedPublication);

        Task<Either<ActionResult, PublicationViewModel>> GetPublication(Guid publicationId);

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
