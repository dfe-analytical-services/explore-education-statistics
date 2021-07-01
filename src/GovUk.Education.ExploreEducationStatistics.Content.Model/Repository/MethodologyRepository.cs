using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.MethodologyPublishingStrategy;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model.Repository
{
    public class MethodologyRepository : IMethodologyRepository
    {
        private readonly ContentDbContext _contentDbContext;
        private readonly IMethodologyParentRepository _methodologyParentRepository;

        public MethodologyRepository(ContentDbContext contentDbContext,
            IMethodologyParentRepository methodologyParentRepository)
        {
            _contentDbContext = contentDbContext;
            _methodologyParentRepository = methodologyParentRepository;
        }

        public async Task<Methodology> CreateMethodologyForPublication(Guid publicationId)
        {
            var publication = await _contentDbContext
                .Publications
                .SingleAsync(p => p.Id == publicationId);

            var methodology = (await _contentDbContext.Methodologies.AddAsync(new Methodology
            {
                PublishingStrategy = Immediately,
                Slug = publication.Slug,
                Title = publication.Title,
                MethodologyParent = new MethodologyParent
                {
                    Publications = new List<PublicationMethodology>
                    {
                        new PublicationMethodology
                        {
                            Owner = true,
                            PublicationId = publicationId
                        }
                    }
                }
            })).Entity;

            await _contentDbContext.SaveChangesAsync();
            return methodology;
        }

        public async Task<List<Methodology>> GetLatestByPublication(Guid publicationId)
        {
            // First check the publication exists
            var publication = await _contentDbContext.Publications.SingleAsync(p => p.Id == publicationId);

            var methodologies = await _methodologyParentRepository.GetByPublication(publication.Id);
            return methodologies.Select(methodology =>
                {
                    _contentDbContext.Entry(methodology)
                        .Collection(m => m.Versions)
                        .Load();

                    return methodology.LatestVersion();
                })
                .Where(version => version != null)
                .ToList();
        }

        public async Task<List<Methodology>> GetLatestPublishedByPublication(Guid publicationId)
        {
            // First check the publication exists
            var publication = await _contentDbContext.Publications.SingleAsync(p => p.Id == publicationId);

            var methodologies = await _methodologyParentRepository.GetByPublication(publication.Id);
            return methodologies.Select(methodology =>
                {
                    _contentDbContext.Entry(methodology)
                        .Collection(m => m.Versions)
                        .Load();

                    return methodology.LatestPublishedVersion();
                })
                .Where(version => version != null)
                .ToList();
        }

        public async Task<bool> IsPubliclyAccessible(Guid methodologyId)
        {
            var methodology = await _contentDbContext.Methodologies
                .Include(m => m.ScheduledWithRelease)
                .SingleAsync(m => m.Id == methodologyId);

            return methodology.Approved &&
                   (methodology.ScheduledForPublishingImmediately ||
                    methodology.ScheduledForPublishingWithPublishedRelease);
        }
    }
}
