#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces.Cache;
using GovUk.Education.ExploreEducationStatistics.Content.Services.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services
{
    public class MethodologyService : IMethodologyService
    {
        private readonly ContentDbContext _contentDbContext;
        private readonly IPersistenceHelper<ContentDbContext> _persistenceHelper;
        private readonly IMapper _mapper;
        private readonly IMethodologyVersionRepository _methodologyVersionRepository;
        private readonly IRedirectsCacheService _redirectsCacheService;

        public MethodologyService(ContentDbContext contentDbContext,
            IPersistenceHelper<ContentDbContext> persistenceHelper,
            IMapper mapper,
            IMethodologyVersionRepository methodologyVersionRepository,
            IRedirectsCacheService redirectsCacheService)
        {
            _contentDbContext = contentDbContext;
            _persistenceHelper = persistenceHelper;
            _mapper = mapper;
            _methodologyVersionRepository = methodologyVersionRepository;
            _redirectsCacheService = redirectsCacheService;
        }

        public async Task<Either<ActionResult, MethodologyVersionViewModel>> GetLatestMethodologyBySlug(string slug)
        {
            return await _persistenceHelper
                .CheckEntityExists<Methodology>(
                    query => query
                        .Include(m => m.LatestPublishedVersion)
                        .Where(m =>
                            m.LatestPublishedVersion != null
                            && slug == (m.LatestPublishedVersion.AlternativeSlug ?? m.OwningPublicationSlug))) // slug == mv.Slug doesn't translate
                .OnSuccess<ActionResult, Methodology, MethodologyVersionViewModel>(async methodology =>
                {
                    var latestPublishedVersion = methodology.LatestPublishedVersion;

                    if (latestPublishedVersion == null)
                    {
                        return new NotFoundResult();
                    }

                    await _contentDbContext
                        .Entry(latestPublishedVersion)
                        .Collection(m => m.Notes)
                        .LoadAsync();

                    await _contentDbContext
                        .Entry(latestPublishedVersion)
                        .Reference(m => m.MethodologyContent)
                        .LoadAsync();

                    var viewModel = _mapper.Map<MethodologyVersionViewModel>(latestPublishedVersion);
                    
                    viewModel.Publications = await GetPublishedPublicationsForMethodology(latestPublishedVersion.MethodologyId);
                    
                    return viewModel;
                });
        }

        public async Task<Either<ActionResult, List<AllMethodologiesThemeViewModel>>> GetSummariesTree()
        {
            var themes = await _contentDbContext.Themes
                .Include(theme => theme.Topics)
                .ThenInclude(topic => topic.Publications)
                .AsNoTracking()
                .Select(theme => new AllMethodologiesThemeViewModel
                {
                    Id = theme.Id,
                    Title = theme.Title,
                    Topics = theme.Topics.Select(topic => new AllMethodologiesTopicViewModel
                    {
                        Id = topic.Id,
                        Title = topic.Title,
                        Publications = topic.Publications.Select(publication =>
                            new AllMethodologiesPublicationViewModel
                            {
                                Id = publication.Id,
                                Title = publication.Title
                            }).ToList()
                    }).ToList()
                })
                .ToListAsync();

            await themes.SelectMany(model => model.Topics)
                .SelectMany(model => model.Publications)
                .ToAsyncEnumerable()
                .ForEachAwaitAsync(async publication =>
                    publication.Methodologies = await BuildMethodologiesForPublication(publication.Id));

            themes.ForEach(theme => theme.RemoveTopicNodesWithoutMethodologiesAndSort());

            return themes.Where(theme => theme.Topics.Any())
                .OrderBy(theme => theme.Title)
                .ToList();
        }

        private async Task<List<PublicationSummaryViewModel>> GetPublishedPublicationsForMethodology(Guid methodologyId)
        {
            return await _contentDbContext.PublicationMethodologies
                .Include(pm => pm.Publication)
                .ThenInclude(p => p.Contact)
                .Where(pm => pm.MethodologyId == methodologyId
                             && pm.Publication.LatestPublishedReleaseId != null)
                .Select(pm => new PublicationSummaryViewModel()
                {
                    Id = pm.PublicationId,
                    Title = pm.Publication.Title,
                    Slug = pm.Publication.Slug,
                    Owner = pm.Owner,
                    Contact = pm.Owner ? _mapper.Map<ContactViewModel>(pm.Publication.Contact) : null
                })
                .OrderBy(pvm => pvm.Title)
                .ToListAsync();
        }

        private async Task<List<MethodologyVersionSummaryViewModel>> BuildMethodologiesForPublication(Guid publicationId)
        {
            var latestPublishedMethodologies =
                await _methodologyVersionRepository.GetLatestPublishedVersionByPublication(publicationId);
            return _mapper.Map<List<MethodologyVersionSummaryViewModel>>(latestPublishedMethodologies);
        }

        // This method is responsible for keeping Methodology Titles and Slugs in sync with their owning Publications
        // where appropriate.  Methodologies always keep track of their owning Publication's titles and slugs for
        // optimisation purposes.
        public async Task PublicationTitleOrSlugChanged(Guid publicationId, string originalSlug, string updatedTitle,
            string updatedSlug)
        {
            var slugChanged = originalSlug != updatedSlug;

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

            if (slugChanged)
            {
                // A redirect is only needed for the LatestPublishedVersion.
                // Unpublished methodologies don't need a redirect - they're not live.
                // An unpublished amendment doesn't need a redirect because:
                // - if it uses OwningPublicationSlug, it is covered by the LatestPublishedVersion redirect created here
                // - if it uses AlternativeSlug, a redirect would have been created at the time the AlternativeSlug
                //   was set (and that redirect will become active when that version is published).
                if (ownedMethodology.LatestPublishedVersion is { AlternativeSlug: null }
                    // guard against duplicates due to users making multiple publication slug changes
                    && !await _contentDbContext.MethodologyRedirects
                        .AnyAsync(mr =>
                            mr.MethodologyVersionId == ownedMethodology.LatestPublishedVersionId
                            && mr.Slug == originalSlug))
                {
                    var redirect = new MethodologyRedirect
                    {
                        MethodologyVersion = ownedMethodology.LatestPublishedVersion,
                        Slug = originalSlug,
                    };
                    _contentDbContext.MethodologyRedirects.Add(redirect);

                    // It's possible we now have two redirects from the same slug. This happens if:
                    // - An unpublished amendment sets an AlternativeSlug
                    // - Then the OwningPublicationSlug changes when the LatestPublishedVersion is
                    //   inheriting it.
                    // We must delete the unpublished amendment's redirect, as otherwise if the unpublished version
                    // changes its slug again, no redirect will be created for the methodology's new slug. (If updating
                    // an unpublished version's slug multiple times, a redirect is only created the first time - see
                    // MethodologyUpdate code). If this doesn't make any sense, see the unit test
                    // PublicationTitleOrSlugChanged_NewMethodologyRedirectSlugMatchesExistingRedirectSlug
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

            if (slugChanged)
            {
                await _redirectsCacheService.UpdateRedirects();
            }
        }
    }
}
