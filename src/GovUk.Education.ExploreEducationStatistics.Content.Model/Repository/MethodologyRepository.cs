using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.MethodologyPublishingStrategy;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.MethodologyStatus;

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

        public async Task<Methodology> CreateMethodologyForPublication(
            Guid publicationId, 
            DateTime createdDate, 
            Guid createdByUserId)
        {
            var publication = await _contentDbContext
                .Publications
                .SingleAsync(p => p.Id == publicationId);

            var methodology = (await _contentDbContext.Methodologies.AddAsync(new Methodology
            {
                PublishingStrategy = Immediately,
                MethodologyParent = new MethodologyParent
                {
                    Slug = publication.Slug,
                    OwningPublicationTitle = publication.Title,
                    Publications = new List<PublicationMethodology>
                    {
                        new PublicationMethodology
                        {
                            Owner = true,
                            PublicationId = publicationId
                        }
                    }
                },
                Created = createdDate,
                CreatedById = createdByUserId
            })).Entity;

            await _contentDbContext.SaveChangesAsync();
            return methodology;
        }

        public async Task<List<Methodology>> GetLatestByPublication(Guid publicationId)
        {
            // First check the publication exists
            var publication = await _contentDbContext.Publications.SingleAsync(p => p.Id == publicationId);

            var methodologyParents = await _methodologyParentRepository.GetByPublication(publication.Id);
            return (await methodologyParents.SelectAsync(async methodologyParent =>
                {
                    await _contentDbContext.Entry(methodologyParent)
                        .Collection(m => m.Versions)
                        .LoadAsync();

                    return methodologyParent.LatestVersion();
                }))
                .Where(version => version != null)
                .ToList();
        }

        public async Task<Methodology> GetLatestPublishedByMethodologyParent(Guid methodologyParentId)
        {
            var methodologyParent = await _contentDbContext.MethodologyParents
                .SingleAsync(mp => mp.Id == methodologyParentId);

            return await GetLatestPublishedByMethodologyParent(methodologyParent);
        }

        public async Task<List<Methodology>> GetLatestPublishedByPublication(Guid publicationId)
        {
            var methodologyParents = await _methodologyParentRepository.GetByPublication(publicationId);
            return (await methodologyParents.SelectAsync(async methodologyParent =>
                    await GetLatestPublishedByMethodologyParent(methodologyParent)))
                .Where(version => version != null)
                .ToList();
        }

        private async Task<Methodology> GetLatestPublishedByMethodologyParent(MethodologyParent methodologyParent)
        {
            await _contentDbContext.Entry(methodologyParent)
                .Collection(m => m.Versions)
                .LoadAsync();

            return await methodologyParent.Versions.FirstOrDefaultAsync(IsPubliclyAccessible);
        }

        public async Task<bool> IsPubliclyAccessible(Guid methodologyId)
        {
            var methodology = await _contentDbContext.Methodologies
                .FindAsync(methodologyId);

            return await IsPubliclyAccessible(methodology);
        }

        // This method is responsible for keeping Methodology Titles and Slugs in sync with their owning Publications
        // where appropriate.  MethodologyParents always keep track of their owning Publication's title for
        // optimisation purposes, but MethodologyParent.Slug is used for the actual Slug for all of its Methodology
        // Versions.  It's therefore important to keep it up-to-date with changes to its owning Publication's Slug too, 
        // but only if none of its Versions are yet publicly accessible.
        public async Task PublicationTitleChanged(Guid publicationId, string originalSlug, string updatedTitle, string updatedSlug)
        {
            var slugChanged = originalSlug != updatedSlug;
            
            // If the Publication Title changed, also change the OwningPublicationTitles of any Methodologies
            // that are owned by this Publication
            var ownedMethodologyParents = _contentDbContext
                .PublicationMethodologies
                .Include(m => m.MethodologyParent)
                .Where(m => m.PublicationId == publicationId && m.Owner)
                .Select(m => m.MethodologyParent);

            await ownedMethodologyParents.ForEachAsync(async methodologyParent =>
            {
                methodologyParent.OwningPublicationTitle = updatedTitle;

                if (slugChanged && methodologyParent.Slug == originalSlug && !await IsPubliclyAccessible(methodologyParent))
                {
                    methodologyParent.Slug = updatedSlug;
                }
            });
                        
            _contentDbContext.MethodologyParents.UpdateRange(ownedMethodologyParents);
            await _contentDbContext.SaveChangesAsync();
        }
        
        private async Task<bool> IsPubliclyAccessible(MethodologyParent methodologyParent)
        {
            await _contentDbContext
                .Entry(methodologyParent)
                .Collection(mp => mp.Versions)
                .LoadAsync();

            foreach (var version in methodologyParent.Versions)
            {
                if (await IsPubliclyAccessible(version))
                {
                    return true;
                }
            }

            return false;
        }

        private async Task<bool> IsPubliclyAccessible(Methodology methodology)
        {
            if (!methodology.Approved)
            {
                return false;
            }

            if (!await IsLatestVersionOfMethodologyExcludingDrafts(methodology))
            {
                return false;
            }

            if (methodology.ScheduledForPublishingImmediately)
            {
                return await PublicationsHaveAtLeastOnePublishedRelease(methodology);
            }

            // Scheduled for publishing with a Release so check the Release is published

            await _contentDbContext.Entry(methodology)
                .Reference(m => m.ScheduledWithRelease)
                .LoadAsync();
            return methodology.ScheduledForPublishingWithPublishedRelease;
        }

        private async Task<bool> IsLatestVersionOfMethodologyExcludingDrafts(Methodology methodology)
        {
            await _contentDbContext.Entry(methodology)
                .Reference(m => m.MethodologyParent)
                .LoadAsync();

            await _contentDbContext.Entry(methodology.MethodologyParent)
                .Collection(mp => mp.Versions)
                .LoadAsync();

            return methodology.MethodologyParent.Versions.All(mv =>
                mv.PreviousVersionId != methodology.Id ||
                mv.PreviousVersionId == methodology.Id && mv.Status != Approved);
        }

        private async Task<bool> PublicationsHaveAtLeastOnePublishedRelease(Methodology methodology)
        {
            await _contentDbContext.Entry(methodology)
                .Reference(m => m.MethodologyParent)
                .LoadAsync();

            await _contentDbContext.Entry(methodology.MethodologyParent)
                .Collection(mp => mp.Publications)
                .LoadAsync();

            return methodology.MethodologyParent.Publications.Any(publicationMethodology =>
            {
                _contentDbContext.Entry(publicationMethodology)
                    .Reference(pm => pm.Publication)
                    .Load();

                return publicationMethodology.Publication.Live;
            });
        }
    }
}
