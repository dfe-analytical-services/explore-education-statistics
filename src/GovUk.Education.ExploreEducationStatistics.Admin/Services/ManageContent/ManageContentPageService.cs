﻿using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.ManageContent;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.ManageContent;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
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
        private readonly IReleaseFileService _releaseFileService;
        private readonly IContentService _contentService;
        private readonly IPersistenceHelper<ContentDbContext> _persistenceHelper;
        private readonly IUserService _userService;

        public ManageContentPageService(
            IMapper mapper,
            IReleaseFileService releaseFileService,
            IContentService contentService,
            IPersistenceHelper<ContentDbContext> persistenceHelper,
            IUserService userService)
        {
            _mapper = mapper;
            _releaseFileService = releaseFileService;
            _contentService = contentService;
            _persistenceHelper = persistenceHelper;
            _userService = userService;
        }

        public async Task<Either<ActionResult, ManageContentPageViewModel>> GetManageContentPageViewModel(
            Guid releaseId)
        {
            return await _persistenceHelper
                .CheckEntityExists<Release>(releaseId, HydrateReleaseForReleaseViewModel)
                .OnSuccess(_userService.CheckCanViewRelease)
                .OnSuccess(release => _contentService.GetUnattachedContentBlocksAsync<DataBlock>(releaseId)
                    .OnSuccess(blocks => _releaseFileService.ListAll(
                            releaseId,
                            ReleaseFileTypes.Ancillary,
                            ReleaseFileTypes.Data)
                        .OnSuccess(files =>
                        {
                            var releaseViewModel =
                                _mapper.Map<ManageContentPageViewModel.ReleaseViewModel>(release);
                            releaseViewModel.DownloadFiles = files.ToList();

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