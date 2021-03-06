﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.ManageContent;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.ManageContent;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static GovUk.Education.ExploreEducationStatistics.Common.Model.FileType;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.ManageContent
{
    public class ManageContentPageService : IManageContentPageService
    {
        private readonly ContentDbContext _contentDbContext;
        private readonly IPersistenceHelper<ContentDbContext> _persistenceHelper;
        private readonly IMapper _mapper;
        private readonly IContentService _contentService;
        private readonly IMethodologyRepository _methodologyRepository;
        private readonly IReleaseFileService _releaseFileService;
        private readonly IUserService _userService;

        public ManageContentPageService(ContentDbContext contentDbContext,
            IPersistenceHelper<ContentDbContext> persistenceHelper,
            IMapper mapper,
            IContentService contentService,
            IMethodologyRepository methodologyRepository,
            IReleaseFileService releaseFileService,
            IUserService userService)
        {
            _contentDbContext = contentDbContext;
            _persistenceHelper = persistenceHelper;
            _mapper = mapper;
            _contentService = contentService;
            _methodologyRepository = methodologyRepository;
            _releaseFileService = releaseFileService;
            _userService = userService;
        }

        public async Task<Either<ActionResult, ManageContentPageViewModel>> GetManageContentPageViewModel(
            Guid releaseId)
        {
            return await _persistenceHelper
                .CheckEntityExists<Release>(query => query
                        .Include(r => r.Type)
                        .Where(r => r.Id == releaseId)
                )
                .OnSuccess(HydrateReleaseForReleaseViewModel)
                .OnSuccess(_userService.CheckCanViewRelease)
                .OnSuccessCombineWith(release => _contentService.GetUnattachedContentBlocksAsync<DataBlock>(releaseId))
                .OnSuccessCombineWith(releaseAndBlocks => _releaseFileService.ListAll(
                    releaseId,
                    Ancillary,
                    FileType.Data))
                .OnSuccess(async releaseBlocksAndFiles =>
                {
                    var (release, blocks, files) = releaseBlocksAndFiles;

                    var methodologies = await _methodologyRepository.GetLatestByPublication(release.PublicationId);

                    var releaseViewModel = _mapper.Map<ManageContentPageViewModel.ReleaseViewModel>(release);
                    releaseViewModel.DownloadFiles = files.ToList();
                    releaseViewModel.Publication.Methodologies =
                        _mapper.Map<List<TitleAndIdViewModel>>(methodologies);

                    return new ManageContentPageViewModel
                    {
                        Release = releaseViewModel,
                        AvailableDataBlocks = blocks
                    };
                });
        }

        private async Task<Release> HydrateReleaseForReleaseViewModel(Release release)
        {
            release.Updates = await _contentDbContext.Update
                .Where(u => u.ReleaseId == release.Id)
                .ToListAsync();

            var publication = await _contentDbContext.Publications
                .Include(p => p.Releases)
                .Include(p => p.Topic.Theme)
                .Include(p => p.Contact)
                .SingleAsync(p => p.Releases.Contains(release));
            release.Publication = publication;

            await _contentDbContext.Entry(release.Publication)
                .Collection(p => p.LegacyReleases)
                .LoadAsync();

            await _contentDbContext.Entry(release.Publication)
                .Collection(p => p.Releases)
                .LoadAsync();

            var content = await _contentDbContext.ReleaseContentSections
                .Where(rcs => rcs.ReleaseId == release.Id)
                .Include(rcs => rcs.ContentSection)
                .ThenInclude(cs => cs.Content)
                .ToListAsync();
            release.Content = content;

            await release.Content.ForEachAsync(async rcs =>
            {
                await rcs.ContentSection.Content.ForEachAsync(async cb =>
                {
                    cb.Comments = await _contentDbContext.Comment
                        .Where(c => c.ContentBlockId == cb.Id)
                        .Include(c => c.CreatedBy)
                        .ToListAsync();
                });
            });
            return release;
        }
    }
}
