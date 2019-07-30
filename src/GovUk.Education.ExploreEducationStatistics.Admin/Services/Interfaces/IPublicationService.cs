using GovUk.Education.ExploreEducationStatistics.Content.Model;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Models.Api;
using UserId = System.Guid;
using TopicId = System.Guid;
using PublicationId = System.Guid;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces
{
    public interface IPublicationService
    {
        List<Publication> List();

        Publication Get(Guid id);

        Publication Get(string slug);

        List<PublicationViewModel> GetByTopicAndUser(TopicId topicId, UserId userId);
        Task<PublicationViewModel> CreatePublication(CreatePublicationViewModel publication);
        
        Task<PublicationViewModel> GetViewModelAsync(PublicationId publicationId);
        
    }
}
