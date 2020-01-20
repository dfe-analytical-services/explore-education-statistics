using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Utils;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.ManageContent;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.ManageContent;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
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
        private readonly IPersistenceHelper<ContentDbContext> _persistenceHelper;

        public ManageContentPageService(
            IMapper mapper,
            IFileStorageService fileStorageService, 
            IContentService contentService, 
            IPersistenceHelper<ContentDbContext> persistenceHelper)
        {
            _mapper = mapper;
            _fileStorageService = fileStorageService;
            _contentService = contentService;
            _persistenceHelper = persistenceHelper;
        }

        public async Task<Either<ActionResult, ManageContentPageViewModel>> GetManageContentPageViewModelAsync(
            Guid releaseId)
        {
            return await _persistenceHelper
                .CheckEntityExists<Release>(releaseId, HydrateReleaseForReleaseViewModel)
                .OnSuccess(release => _contentService.GetUnattachedContentBlocksAsync<DataBlock>(releaseId)
                .OnSuccess(blocks => _fileStorageService.ListPublicFilesPreview(releaseId)
                .OnSuccess(publicFiles =>
                {
                    var releaseViewModel = _mapper.Map<ReleaseViewModel>(release);
                    releaseViewModel.DownloadFiles = publicFiles;

                    // TODO EES-147 Every release needs an update
                    if (releaseViewModel.Updates.Count == 0)
                    {
                        releaseViewModel.Updates.Add(new ReleaseNoteViewModel
                        {
                            Id = new Guid("262cf6c8-db96-40d8-8fb1-b55028a9f55b"),
                            On = new DateTime(2019, 12, 01),
                            Reason = "First published"
                        });
                    }

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
                .Include(r => r.Updates);
        }
    }
}