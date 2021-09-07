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
using static GovUk.Education.ExploreEducationStatistics.Content.Model.MethodologyStatus;

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
                .SingleAsync(p => p.Id == publicationId);

            var methodology = (await _contentDbContext.MethodologyVersions.AddAsync(new MethodologyVersion
            {
                PublishingStrategy = Immediately,
                Methodology = new Methodology
                {
                    Slug = publication.Slug,
                    OwningPublicationTitle = publication.Title,
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
            return methodology;
        }

        public async Task<MethodologyVersion> GetLatestVersion(Guid methodologyId)
        {
            var methodology = await _contentDbContext.Methodologies
                .Include(m => m.Versions)
                .SingleAsync(mp => mp.Id == methodologyId);

            return methodology.LatestVersion();
        }

        public async Task<List<MethodologyVersion>> GetLatestVersionByPublication(Guid publicationId)
        {
            // First check the publication exists
            var publication = await _contentDbContext.Publications
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

        public async Task<MethodologyVersion?> GetLatestPublishedVersion(Guid methodologyId)
        {
            var methodology = await _contentDbContext.Methodologies
                .SingleAsync(mp => mp.Id == methodologyId);

            return await GetLatestPublishedByMethodology(methodology);
        }

        public async Task<List<MethodologyVersion>> GetLatestPublishedVersionByPublication(Guid publicationId)
        {
            var methodologies = await _methodologyRepository.GetByPublication(publicationId);
            return (await methodologies
                    .SelectAsync(async methodology =>
                        await GetLatestPublishedByMethodology(methodology)))
                .WhereNotNull()
                .ToList();
        }

        private async Task<MethodologyVersion?> GetLatestPublishedByMethodology(Methodology methodology)
        {
            await _contentDbContext.Entry(methodology)
                .Collection(m => m.Versions)
                .LoadAsync();

            return await methodology.Versions.FirstOrDefaultAsync(IsPubliclyAccessible);
        }

        public async Task<bool> IsPubliclyAccessible(Guid methodologyVersionId)
        {
            var methodologyVersion = await _contentDbContext.MethodologyVersions
                .FindAsync(methodologyVersionId);

            return await IsPubliclyAccessible(methodologyVersion);
        }

        // This method is responsible for keeping Methodology Titles and Slugs in sync with their owning Publications
        // where appropriate.  Methodologies always keep track of their owning Publication's title for
        // optimisation purposes, but Methodology.Slug is used for the actual Slug for all of its Methodology
        // Versions.  It's therefore important to keep it up-to-date with changes to its owning Publication's Slug too, 
        // but only if none of its Versions are yet publicly accessible.
        public async Task PublicationTitleChanged(Guid publicationId, string originalSlug, string updatedTitle,
            string updatedSlug)
        {
            var slugChanged = originalSlug != updatedSlug;

            // If the Publication Title changed, also change the OwningPublicationTitles of any Methodologies
            // that are owned by this Publication
            var ownedMethodologies = await _contentDbContext
                .PublicationMethodologies
                .Include(m => m.Methodology)
                .Where(m => m.PublicationId == publicationId && m.Owner)
                .Select(m => m.Methodology)
                .ToListAsync();

            await ownedMethodologies.ForEachAsync(async methodology =>
            {
                methodology.OwningPublicationTitle = updatedTitle;

                if (slugChanged && methodology.Slug == originalSlug &&
                    !await IsPubliclyAccessible(methodology))
                {
                    methodology.Slug = updatedSlug;
                }
            });

            _contentDbContext.Methodologies.UpdateRange(ownedMethodologies);
            await _contentDbContext.SaveChangesAsync();
        }

        private async Task<bool> IsPubliclyAccessible(Methodology methodology)
        {
            await _contentDbContext
                .Entry(methodology)
                .Collection(mp => mp.Versions)
                .LoadAsync();

            foreach (var version in methodology.Versions)
            {
                if (await IsPubliclyAccessible(version))
                {
                    return true;
                }
            }

            return false;
        }

        private async Task<bool> IsPubliclyAccessible(MethodologyVersion methodologyVersion)
        {
            if (!methodologyVersion.Approved)
            {
                return false;
            }

            if (!await IsLatestVersionOfMethodologyExcludingDrafts(methodologyVersion))
            {
                return false;
            }

            if (methodologyVersion.ScheduledForPublishingImmediately)
            {
                return await PublicationsHaveAtLeastOnePublishedRelease(methodologyVersion);
            }

            // Scheduled for publishing with a Release so check the Release is published

            await _contentDbContext.Entry(methodologyVersion)
                .Reference(m => m.ScheduledWithRelease)
                .LoadAsync();
            return methodologyVersion.ScheduledForPublishingWithPublishedRelease;
        }

        private async Task<bool> IsLatestVersionOfMethodologyExcludingDrafts(MethodologyVersion methodologyVersion)
        {
            await _contentDbContext.Entry(methodologyVersion)
                .Reference(m => m.Methodology)
                .LoadAsync();

            await _contentDbContext.Entry(methodologyVersion.Methodology)
                .Collection(mp => mp.Versions)
                .LoadAsync();

            return methodologyVersion.Methodology.Versions.All(mv =>
                mv.PreviousVersionId != methodologyVersion.Id ||
                mv.PreviousVersionId == methodologyVersion.Id && mv.Status != Approved);
        }

        private async Task<bool> PublicationsHaveAtLeastOnePublishedRelease(MethodologyVersion methodologyVersion)
        {
            await _contentDbContext.Entry(methodologyVersion)
                .Reference(m => m.Methodology)
                .LoadAsync();

            await _contentDbContext.Entry(methodologyVersion.Methodology)
                .Collection(mp => mp.Publications)
                .LoadAsync();

            return methodologyVersion.Methodology.Publications.Any(publicationMethodology =>
            {
                _contentDbContext.Entry(publicationMethodology)
                    .Reference(pm => pm.Publication)
                    .Load();

                return publicationMethodology.Publication.Live;
            });
        }
    }
}
