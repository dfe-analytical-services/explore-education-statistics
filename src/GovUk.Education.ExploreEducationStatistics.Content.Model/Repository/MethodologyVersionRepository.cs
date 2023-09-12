#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
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

        public async Task<MethodologyVersion> GetLatestVersion(Guid methodologyId)
        {
            var methodology = await _contentDbContext.Methodologies
                .Include(m => m.Versions)
                .SingleAsync(mp => mp.Id == methodologyId);

            return methodology.LatestVersion();
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

        // This method is responsible for keeping Methodology Titles and Slugs in sync with their owning Publications
        // where appropriate.  Methodologies always keep track of their owning Publication's titles and slugs for
        // optimisation purposes.
        public async Task PublicationTitleOrSlugChanged(Guid publicationId, string originalSlug, string updatedTitle,
            string updatedSlug)
        {
            var ownedMethodology = await _contentDbContext
                .PublicationMethodologies
                .Include(pm => pm.Methodology.LatestPublishedVersion)
                .Include(pm => pm.Methodology.Versions)
                .Where(pm => pm.PublicationId == publicationId && pm.Owner)
                .Select(pm => pm.Methodology)
                .SingleOrDefaultAsync();

            if (ownedMethodology == null)
            {
                return;
            }

            ownedMethodology.OwningPublicationTitle = updatedTitle;
            ownedMethodology.OwningPublicationSlug = updatedSlug;

            _contentDbContext.Methodologies.Update(ownedMethodology);

            // If slug has changed, we may need a redirect
            if (originalSlug != updatedSlug)
            {
                // A redirect only needed for the LatestPublishedVersion.
                // Unpublished methodology's don't need a redirect - they're not live.
                // An unpublished amendment doesn't need a redirect because:
                // - if it uses OwningPublicationSlug, it is covered by the LatestPublishedVersion redirect created here
                // - if it uses AlternativeSlug, a redirect would have been created at the time the AlternativeSlug
                //   was set (and that redirect will become active when that version is published).
                if (ownedMethodology.LatestPublishedVersion is { AlternativeSlug: null })
                {
                    var redirect = new MethodologyRedirect
                    {
                        MethodologyVersion = ownedMethodology.LatestPublishedVersion,
                        Slug = originalSlug,
                    };
                    _contentDbContext.MethodologyRedirects.Add(redirect);

                    // NOTE: It's possible we now have two redirects from the same slug. This happens if:
                    // - An unpublished amendment sets an AlternativeSlug
                    // - Then the OwningPublicationSlug changes when the LatestPublishedVersion is inheriting it.
                    // We must delete this, as otherwise if the unpublished version changes its slug again,
                    // no redirect will be created for the methodology's new slug. (If updating an unpublished
                    // version's slug multiple times, a redirect is only created the first time - see
                    // MethodologyUpdate code).
                    var redirectToRemove = await _contentDbContext.MethodologyRedirects
                        .Where(mr =>
                            mr.MethodologyVersionId == ownedMethodology.LatestVersion().Id
                            && mr.Slug == originalSlug)
                        .SingleOrDefaultAsync();

                    if (redirectToRemove != null)
                    {
                        _contentDbContext.MethodologyRedirects.Remove(redirectToRemove);
                    }
                }
            }

            await _contentDbContext.SaveChangesAsync();
        }

        public async Task<bool> IsLatestPublishedVersion(MethodologyVersion methodologyVersion)
        {
            await _contentDbContext.Entry(methodologyVersion)
                .Reference(mv => mv.Methodology)
                .LoadAsync();

            return methodologyVersion.Id == methodologyVersion.Methodology.LatestPublishedVersionId;
        }

        // TODO: EES-4613 Move IsToBePublished to MethodologyApprovalService and change to private after MethodologyMigrationController has been removed
        public async Task<bool> IsToBePublished(MethodologyVersion methodologyVersion)
        {
            // A version that's not approved can't be publicly accessible
            if (!methodologyVersion.Approved)
            {
                return false;
            }

            // A methodology version with a newer published version cannot be live
            var nextVersion = await GetNextVersion(methodologyVersion);
            if (nextVersion?.Approved == true)
            {
                // If the next version is scheduled for immediate publishing, we don't need to check if the
                // associated publication is published: the previous version won't be published in either case.
                if (nextVersion.ScheduledForPublishingImmediately ||
                    await IsVersionScheduledForPublishingWithPublishedRelease(nextVersion))
                {
                    return false;
                }
            }

            // A version scheduled for publishing immediately is restricted from public view until it's used by
            // a publication that's published
            if (methodologyVersion.ScheduledForPublishingImmediately)
            {
                return await PublicationsHaveAtLeastOnePublishedRelease(methodologyVersion);
            }

            // A version scheduled for publishing with a release is only publicly accessible if that release is published
            return await IsVersionScheduledForPublishingWithPublishedRelease(methodologyVersion);
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

        private async Task<bool> IsVersionScheduledForPublishingWithPublishedRelease(
            MethodologyVersion methodologyVersion)
        {
            if (!methodologyVersion.ScheduledForPublishingWithRelease)
            {
                return false;
            }

            await _contentDbContext.Entry(methodologyVersion)
                .Reference(m => m.ScheduledWithRelease)
                .LoadAsync();
            return methodologyVersion.ScheduledForPublishingWithPublishedRelease;
        }

        private async Task<MethodologyVersion?> GetNextVersion(MethodologyVersion methodologyVersion)
        {
            await _contentDbContext.Entry(methodologyVersion)
                .Reference(m => m.Methodology)
                .LoadAsync();

            await _contentDbContext.Entry(methodologyVersion.Methodology)
                .Collection(mp => mp.Versions)
                .LoadAsync();

            // TODO EES-4628 SingleOrDefault here is susceptible to bug EES-2672 which is allowing multiple amendments
            // of the same version to be created. If there is a next version there should only be one.
            return methodologyVersion.Methodology.Versions.SingleOrDefault(mv =>
                mv.PreviousVersionId == methodologyVersion.Id);
        }
    }
}
