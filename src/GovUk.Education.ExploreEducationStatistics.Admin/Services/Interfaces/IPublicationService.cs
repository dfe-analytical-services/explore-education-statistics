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
        Task<List<MyPublicationViewModel>> GetMyPublicationsAndReleasesByTopicAsync(Guid topicId);

        Task<Either<ActionResult, PublicationViewModel>> CreatePublicationAsync(
            CreatePublicationViewModel publication);

        Task<PublicationViewModel> GetViewModelAsync(Guid publicationId);
    }
}