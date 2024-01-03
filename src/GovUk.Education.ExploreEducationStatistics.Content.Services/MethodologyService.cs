﻿#nullable enable
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
                    Contact = _mapper.Map<ContactViewModel>(pm.Publication.Contact)
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
    }
}
