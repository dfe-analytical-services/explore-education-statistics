#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.MethodologyPublishingStrategy;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model.Repository
{
    public class MethodologyVersionRepository : IMethodologyVersionRepository
    {
        private readonly ContentDbContext _contentDbContext;
        private readonly IMethodologyRepository _methodologyRepository;

        public MethodologyVersionRepository(ContentDbContext contentDbContext,
            IMethodologyRepository methodologyRepository)
        {
            _contentDbContext = contentDbContext;
            _methodologyRepository = methodologyRepository;
        }

        public async Task<MethodologyVersion> CreateMethodologyForPublication(Guid publicationId, Guid createdByUserId)
        {
            var publication = await _contentDbContext
                .Publications
                .AsQueryable()
                .SingleAsync(p => p.Id == publicationId);

            var methodologyVersion = (await _contentDbContext.MethodologyVersions.AddAsync(new MethodologyVersion
            {
                PublishingStrategy = Immediately,
                Methodology = new Methodology
                {
                    Slug = publication.Slug, // Remove EES-4627
                    OwningPublicationTitle = publication.Title,
                    OwningPublicationSlug = publication.Slug,
                    Publications = new List<PublicationMethodology>
                    {
                        new()
                        {
                            Owner = true,
                            PublicationId = publicationId
                        }
                    }
                },
                Created = DateTime.UtcNow,
                CreatedById = createdByUserId
            })).Entity;

            await _contentDbContext.SaveChangesAsync();
            return methodologyVersion;
        }

        public async Task<List<MethodologyVersion>> GetLatestVersionByPublication(Guid publicationId)
        {
            var publication = await _contentDbContext.Publications
                .AsQueryable()
                .SingleAsync(p => p.Id == publicationId);

            var methodologies = await _methodologyRepository.GetByPublication(publication.Id);
            return (await methodologies.SelectAsync(async methodology =>
                {
                    await _contentDbContext.Entry(methodology)
                        .Collection(m => m.Versions)
                        .LoadAsync();

                    return methodology.LatestVersion();
                }))
                .Where(version => version != null)
                .ToList();
        }

        public async Task<MethodologyVersion?> GetLatestPublishedVersionBySlug(string slug)
        {
            return await _contentDbContext
                .MethodologyVersions
                .Where(mv =>
                    mv.Methodology.LatestPublishedVersionId == mv.Id
                    // EF cannot translate mv.Slug into a Queryable, so we have to do this...
                    && slug == (mv.AlternativeSlug ?? mv.Methodology.OwningPublicationSlug))
                .SingleOrDefaultAsync();
        }

        public async Task<MethodologyVersion?> GetLatestPublishedVersion(Guid methodologyId)
        {
            return await _contentDbContext.Methodologies
                .Include(m => m.LatestPublishedVersion)
                .AsQueryable()
                .Where(mp => mp.Id == methodologyId)
                .Select(m => m.LatestPublishedVersion)
                .SingleAsync();
        }

        public async Task<List<MethodologyVersion>> GetLatestPublishedVersionByPublication(Guid publicationId)
        {
            var methodologies = await _methodologyRepository.GetByPublication(publicationId);

            var methodologyVersions = await methodologies
                .SelectAsync(async methodology =>
                {
                    await _contentDbContext.Entry(methodology)
                        .Reference(m => m.LatestPublishedVersion)
                        .LoadAsync();

                    return methodology.LatestPublishedVersion;
                });

            return methodologyVersions
                .WhereNotNull()
                .ToList();
        }

        public async Task<bool> IsLatestPublishedVersion(MethodologyVersion methodologyVersion)
        {
            await _contentDbContext.Entry(methodologyVersion)
                .Reference(mv => mv.Methodology)
                .LoadAsync();

            return methodologyVersion.Id == methodologyVersion.Methodology.LatestPublishedVersionId;
        }
    }
}
