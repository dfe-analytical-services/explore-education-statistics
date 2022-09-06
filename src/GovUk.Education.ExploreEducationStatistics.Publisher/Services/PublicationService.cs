#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Extensions;
using GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Services
{
    public class PublicationService : IPublicationService
    {
        private readonly ContentDbContext _contentDbContext;

        public PublicationService(ContentDbContext contentDbContext)
        {
            _contentDbContext = contentDbContext;
        }

        public List<Publication> GetPublicationsWithPublishedReleases()
        {
            return _contentDbContext.Publications
                .Include(publication => publication.Releases)
                .ToList()
                .Where(p => p.Releases.Any(release => release.IsLatestPublishedVersionOfRelease()))
                .ToList();
        }

        public async Task<bool> IsPublicationPublished(Guid publicationId)
        {
            var publication = await _contentDbContext.Publications
                .Include(p => p.Releases)
                .SingleAsync(p => p.Id == publicationId);

            return publication.Releases.Any(release => release.IsLatestPublishedVersionOfRelease());
        }
    }
}
