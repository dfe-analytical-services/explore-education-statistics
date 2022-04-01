#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Content.Model;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces
{
    public interface IPublicationService
    {
        Task<Publication> Get(Guid id);

        List<Publication> GetPublicationsWithPublishedReleases();

        Task<bool> IsPublicationPublished(Guid publicationId);
    }
}
