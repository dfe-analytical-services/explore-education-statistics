using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Models.Api;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces
{
    public interface IPublicationService
    {
        Task<Either<ActionResult, List<MyPublicationViewModel>>> GetMyPublicationsAndReleasesByTopic(Guid topicId);

        Task<Either<ActionResult, PublicationViewModel>> CreatePublication(
            CreatePublicationViewModel publication);

        Task<Either<ActionResult, PublicationViewModel>> GetViewModel(Guid publicationId);
        
        Task<Either<ActionResult, bool>> UpdatePublicationMethodology(Guid publicationId, UpdatePublicationMethodologyViewModel methodology);
    }
}