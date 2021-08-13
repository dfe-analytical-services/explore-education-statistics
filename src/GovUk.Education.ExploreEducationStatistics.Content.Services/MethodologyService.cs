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
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Services.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Publisher.Model.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services
{
    public class MethodologyService : IMethodologyService
    {
        private readonly ContentDbContext _contentDbContext;
        private readonly IPersistenceHelper<ContentDbContext> _persistenceHelper;
        private readonly IMapper _mapper;
        private readonly IMethodologyRepository _methodologyRepository;

        public MethodologyService(ContentDbContext contentDbContext,
            IPersistenceHelper<ContentDbContext> persistenceHelper,
            IMapper mapper,
            IMethodologyRepository methodologyRepository)
        {
            _contentDbContext = contentDbContext;
            _persistenceHelper = persistenceHelper;
            _mapper = mapper;
            _methodologyRepository = methodologyRepository;
        }

        public async Task<Either<ActionResult, MethodologyViewModel>> GetLatestMethodologyBySlug(string slug)
        {
            return await _persistenceHelper
                .CheckEntityExists<MethodologyParent>(
                    query => query
                        .Where(mp => mp.Slug == slug))
                .OnSuccess<ActionResult, MethodologyParent, MethodologyViewModel>(async methodologyParent =>
                {
                    var latestPublishedVersion =
                        await _methodologyRepository.GetLatestPublishedByMethodologyParent(methodologyParent.Id);
                    if (latestPublishedVersion == null)
                    {
                        return new NotFoundResult();
                    }

                    return _mapper.Map<MethodologyViewModel>(latestPublishedVersion);
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

        private async Task<List<MethodologySummaryViewModel>> BuildMethodologiesForPublication(Guid publicationId)
        {
            var latestPublishedMethodologies =
                await _methodologyRepository.GetLatestPublishedByPublication(publicationId);
            return _mapper.Map<List<MethodologySummaryViewModel>>(latestPublishedMethodologies);
        }
    }
}
