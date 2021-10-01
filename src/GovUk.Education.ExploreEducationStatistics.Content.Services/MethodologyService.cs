#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Common.Cache;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Services.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services
{
    public class MethodologyService : IMethodologyService
    {
        private readonly ContentDbContext _contentDbContext;
        private readonly IPersistenceHelper<ContentDbContext> _persistenceHelper;
        private readonly IMapper _mapper;
        private readonly IMethodologyVersionRepository _methodologyVersionRepository;

        public MethodologyService(ContentDbContext contentDbContext,
            IPersistenceHelper<ContentDbContext> persistenceHelper,
            IMapper mapper,
            IMethodologyVersionRepository methodologyVersionRepository)
        {
            _contentDbContext = contentDbContext;
            _persistenceHelper = persistenceHelper;
            _mapper = mapper;
            _methodologyVersionRepository = methodologyVersionRepository;
        }

        public async Task<Either<ActionResult, MethodologyViewModel>> GetLatestMethodologyBySlug(string slug)
        {
            return await _persistenceHelper
                .CheckEntityExists<Methodology>(
                    query => query
                        .Where(mp => mp.Slug == slug))
                .OnSuccess<ActionResult, Methodology, MethodologyViewModel>(async methodology =>
                {
                    var latestPublishedVersion =
                        await _methodologyVersionRepository.GetLatestPublishedVersion(methodology.Id);
                    if (latestPublishedVersion == null)
                    {
                        return new NotFoundResult();
                    }

                    await _contentDbContext.Entry(latestPublishedVersion)
                        .Collection(m => m.Notes)
                        .LoadAsync();

                    var viewModel = _mapper.Map<MethodologyViewModel>(latestPublishedVersion);
                    viewModel.Publications =
                        await GetPublishedPublicationsForMethodology(latestPublishedVersion.MethodologyId);

                    return viewModel;
                });
        }

        public async Task<Either<ActionResult, List<MethodologySummaryViewModel>>> GetSummariesByPublication(
            Guid publicationId)
        {
            return await _persistenceHelper
                .CheckEntityExists<Publication>(publicationId)
                .OnSuccess(publication => BuildMethodologiesForPublication(publication.Id));
        }

        [BlobCache(typeof(AllMethodologiesCacheKey))]
        public async Task<Either<ActionResult, List<AllMethodologiesThemeViewModel>>> GetTree()
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
                .ForEachAsync(async publication =>
                    publication.Methodologies = await BuildMethodologiesForPublication(publication.Id));

            themes.ForEach(theme => theme.RemoveTopicNodesWithoutMethodologiesAndSort());

            return themes.Where(theme => theme.Topics.Any())
                .OrderBy(theme => theme.Title)
                .ToList();
        }

        private async Task<List<PublicationSummaryViewModel>> GetPublishedPublicationsForMethodology(Guid methodologyId)
        {
            var publications = await _contentDbContext.PublicationMethodologies
                .Include(pm => pm.Publication)
                .ThenInclude(p => p.Releases)
                .Where(pm => pm.MethodologyId == methodologyId)
                .Select(pm => pm.Publication)
                .ToListAsync();

            var publicationsWithPublishedReleases = publications
                .Where(p => p.Releases.Any(r => r.IsLatestPublishedVersionOfRelease()))
                .OrderBy(p => p.Title)
                .ToList();

            return _mapper.Map<List<PublicationSummaryViewModel>>(publicationsWithPublishedReleases);
        }

        private async Task<List<MethodologySummaryViewModel>> BuildMethodologiesForPublication(Guid publicationId)
        {
            var latestPublishedMethodologies =
                await _methodologyVersionRepository.GetLatestPublishedVersionByPublication(publicationId);
            return _mapper.Map<List<MethodologySummaryViewModel>>(latestPublishedMethodologies);
        }
    }
}
