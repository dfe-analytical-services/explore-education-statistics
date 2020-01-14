using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Models.Api;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces
{
    // TODO EES-919 - return ActionResults rather than ValidationResults
    public interface IPublicationService
    {
        List<Publication> List();

        Task<Publication> GetAsync(Guid id);

        Publication Get(string slug);

        Task<List<MyPublicationViewModel>> GetMyPublicationsAndReleasesByTopicAsync(Guid topicId);

        Task<Either<ActionResult, MyPublicationViewModel>> CreatePublicationAsync(
            CreatePublicationViewModel publication);

        Task<MyPublicationViewModel> GetViewModelAsync(Guid publicationId);
    }
}