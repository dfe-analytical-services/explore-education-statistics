using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.ManageContent;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Methodologies;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.ManageContent;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.Methodology;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static GovUk.Education.ExploreEducationStatistics.Common.Model.FileType;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.ManageContent
{
    public class ManageContentPageService : IManageContentPageService
    {
        private readonly IPersistenceHelper<ContentDbContext> _persistenceHelper;
        private readonly IMapper _mapper;
        private readonly IContentService _contentService;
        private readonly IMethodologyRepository _methodologyRepository;
        private readonly IReleaseFileService _releaseFileService;
        private readonly IUserService _userService;

        public ManageContentPageService(IPersistenceHelper<ContentDbContext> persistenceHelper,
            IMapper mapper,
            IContentService contentService,
            IMethodologyRepository methodologyRepository,
            IReleaseFileService releaseFileService,
            IUserService userService)
        {
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
                .CheckEntityExists<Release>(releaseId, HydrateReleaseForReleaseViewModel)
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
                        _mapper.Map<List<MethodologyTitleViewModel>>(methodologies);

                    return new ManageContentPageViewModel
                    {
                        Release = releaseViewModel,
                        AvailableDataBlocks = blocks
                    };
                });
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
