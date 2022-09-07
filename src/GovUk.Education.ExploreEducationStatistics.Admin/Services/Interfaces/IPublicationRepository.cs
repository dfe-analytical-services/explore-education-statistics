#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Content.Model;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces
{
    public interface IPublicationRepository
    {
        IQueryable<Publication> QueryPublicationsForTopic(Guid? topicId = null);

        Task<List<Publication>> ListPublicationsForUser(Guid userId, Guid? topicId = null);

        Task<Publication> GetPublicationForUser(Guid publicationId, Guid userId);

        Task<Publication> GetPublicationWithAllReleases(Guid publicationId);

        Task<List<Release>> ListActiveReleases(Guid publicationId);

        Task<Release?> GetLatestReleaseForPublication(Guid publicationId);

        bool IsSuperseded(Publication publication);
    }
}
