#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Content.Model;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces
{
    public interface IPublicationRepository
    {
        Task<List<Publication>> GetAllPublicationsForTopic(Guid topicId);

        Task<List<Publication>> GetPublicationsForTopicRelatedToUser(Guid topicId, Guid userId);

        Task<Publication> GetPublicationForUser(Guid publicationId, Guid userId);

        Task<Publication> GetPublicationWithAllReleases(Guid publicationId);

        Task<List<Release>> ListActiveReleases(Guid publicationId);

        Task<Release?> GetLatestReleaseForPublication(Guid publicationId);

        bool IsSuperseded(Publication publication);
    }
}
