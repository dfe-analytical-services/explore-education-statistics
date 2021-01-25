using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces
{
    public interface IPublicationService
    {
        Task<Either<ActionResult, List<MyPublicationViewModel>>> GetMyPublicationsAndReleasesByTopic(Guid topicId);

        Task<Either<ActionResult, PublicationViewModel>> CreatePublication(
            SavePublicationViewModel publication);

        Task<Either<ActionResult, PublicationViewModel>> UpdatePublication(
            Guid publicationId,
            SavePublicationViewModel updatedPublication);

        Task<Either<ActionResult, PublicationViewModel>> GetViewModel(Guid publicationId);
        
        Task<Either<ActionResult, List<LegacyReleaseViewModel>>> PartialUpdateLegacyReleases(
            Guid publicationId, 
            List<PartialUpdateLegacyReleaseViewModel> updatedLegacyReleases);
    }
}