#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Content.Model;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces
{
    public interface IPublicationRepository : Content.Model.Repository.Interfaces.IPublicationRepository
    {
        IQueryable<Publication> QueryPublicationsForTopic(Guid? topicId = null);

        Task<List<Publication>> ListPublicationsForUser(Guid userId, Guid? topicId = null);

        Task<List<Release>> ListActiveReleases(Guid publicationId);

        Task<Release?> GetLatestReleaseForPublication(Guid publicationId);
    }
}
