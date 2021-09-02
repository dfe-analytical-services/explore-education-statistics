#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Content.Model;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces
{
    public interface IPublicationRepository
    {
        Task<List<MyPublicationViewModel>> GetAllPublicationsForTopic(Guid topicId);

        Task<List<MyPublicationViewModel>> GetPublicationsForTopicRelatedToUser(Guid topicId, Guid userId);

        Task<MyPublicationViewModel> GetPublicationForUser(Guid publicationId, Guid userId);

        Task<MyPublicationViewModel> GetPublicationWithAllReleases(Guid publicationId);

        Task<Release?> GetLatestReleaseForPublication(Guid publicationId);
    }
}
