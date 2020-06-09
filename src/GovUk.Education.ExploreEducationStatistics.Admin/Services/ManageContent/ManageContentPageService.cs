﻿using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.ManageContent;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.ManageContent;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.ManageContent
{
    public class ManageContentPageService : IManageContentPageService
    {
        private readonly IMapper _mapper;
        private readonly IFileStorageService _fileStorageService;
        private readonly IContentService _contentService;
        private readonly IReleaseService _releaseService;
        private readonly IPersistenceHelper<ContentDbContext> _persistenceHelper;

        public ManageContentPageService(
            IMapper mapper,
            IFileStorageService fileStorageService, 
            IContentService contentService,
            IReleaseService releaseService,
            IPersistenceHelper<ContentDbContext> persistenceHelper)
        {
            _mapper = mapper;
            _fileStorageService = fileStorageService;
            _contentService = contentService;
            _releaseService = releaseService;
            _persistenceHelper = persistenceHelper;
        }

        public async Task<Either<ActionResult, ManageContentPageViewModel>> GetManageContentPageViewModelAsync(
            Guid releaseId)
        {
            return await _persistenceHelper
                .CheckEntityExists<Release>(releaseId, HydrateReleaseForReleaseViewModel)
                .OnSuccess(release => _contentService.GetUnattachedContentBlocksAsync<DataBlock>(releaseId)
                .OnSuccess(blocks => _fileStorageService.ListPublicFilesPreview(
                        releaseId, _releaseService.GetReferencedReleaseFileVersions(releaseId, ReleaseFileTypes.Data, ReleaseFileTypes.Ancillary))
                .OnSuccess(publicFiles =>
                {
                    var releaseViewModel = _mapper.Map<ReleaseViewModel>(release);
                    releaseViewModel.DownloadFiles = publicFiles;

                    return new ManageContentPageViewModel
                    {
                        Release = releaseViewModel,
                        AvailableDataBlocks = blocks
                    };
                })));
        }
        
        private static IQueryable<Release> HydrateReleaseForReleaseViewModel(IQueryable<Release> values)
        {
            return values
                .Include(r => r.Publication)
                .ThenInclude(publication => publication.Contact)
                .Include(r => r.Publication)
                .ThenInclude(publication => publication.Releases)
                .Include(r => r.Publication)
                .ThenInclude(publication => publication.LegacyReleases)
                .Include(r => r.Publication)
                .ThenInclude(publication => publication.Methodology)
                .Include(r => r.Publication)
                .ThenInclude(publication => publication.Topic.Theme)
                .Include(r => r.Type)
                .Include(r => r.Content)
                .ThenInclude(join => join.ContentSection)
                .ThenInclude(section => section.Content)
                .ThenInclude(content => content.Comments)
                .ThenInclude(comment => comment.CreatedBy)
                .Include(r => r.Updates);
        }
    }
}