using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Models.Api;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces
{
    public interface IPublicationService
    {
        List<Publication> List();

        Task<Publication> GetAsync(Guid id);

        Publication Get(string slug);

        Task<List<PublicationViewModel>> GetByTopicAndUserAsync(Guid topicId, Guid userId);

        Task<Either<ValidationResult, PublicationViewModel>> CreatePublicationAsync(
            CreatePublicationViewModel publication);

        Task<PublicationViewModel> GetViewModelAsync(Guid publicationId);
    }
}