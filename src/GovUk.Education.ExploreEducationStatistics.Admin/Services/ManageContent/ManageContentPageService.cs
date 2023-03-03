#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.ManageContent;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.ManageContent;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Chart;
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
        private readonly IPersistenceHelper<ContentDbContext> _persistenceHelper;
        private readonly IMapper _mapper;
        private readonly IDataBlockService _dataBlockService;
        private readonly IMethodologyVersionRepository _methodologyVersionRepository;
        private readonly IReleaseFileService _releaseFileService;
        private readonly IUserService _userService;

        public ManageContentPageService(
            IPersistenceHelper<ContentDbContext> persistenceHelper,
            IMapper mapper,
            IDataBlockService dataBlockService,
            IMethodologyVersionRepository methodologyVersionRepository,
            IReleaseFileService releaseFileService,
            IUserService userService)
        {
            _persistenceHelper = persistenceHelper;
            _mapper = mapper;
            _dataBlockService = dataBlockService;
            _methodologyVersionRepository = methodologyVersionRepository;
            _releaseFileService = releaseFileService;
            _userService = userService;
        }

        public async Task<Either<ActionResult, ManageContentPageViewModel>> GetManageContentPageViewModel(
            Guid releaseId)
        {
            return await _persistenceHelper
                .CheckEntityExists<Release>(releaseId, HydrateReleaseQuery)
                .OnSuccess(_userService.CheckCanViewRelease)
                .OnSuccessCombineWith(release => _dataBlockService.GetUnattachedDataBlocks(releaseId))
                .OnSuccessCombineWith(releaseAndBlocks => _releaseFileService.ListAll(
                    releaseId,
                    Ancillary,
                    FileType.Data))
                .OnSuccess(async releaseBlocksAndFiles =>
                {
                    var (release, unattachedDataBlocks, files) = releaseBlocksAndFiles;

                    var methodologies = await _methodologyVersionRepository.GetLatestVersionByPublication(release.PublicationId);

                    var releaseViewModel = _mapper.Map<ManageContentPageViewModel.ReleaseViewModel>(release);
                    releaseViewModel.DownloadFiles = files.ToList();
                    releaseViewModel.Publication.Methodologies =
                        _mapper.Map<List<IdTitleViewModel>>(methodologies);

                    // TODO EES-3319 - remove backwards-compatibility for Map Configuration without its
                    // own Boundary Level selection
                    releaseViewModel.Content.ForEach(c => c.Content.ForEach(contentBlock =>
                    {
                        if (contentBlock is DataBlockViewModel dataBlock)
                        {
                            dataBlock.Charts.ForEach(chart =>
                            {
                                if (chart is MapChart { BoundaryLevel: null } mapChart)
                                {
                                    mapChart.BoundaryLevel = dataBlock.Query.BoundaryLevel;
                                }
                            });
                        }
                    }));

                    return new ManageContentPageViewModel
                    {
                        Release = releaseViewModel,
                        UnattachedDataBlocks = unattachedDataBlocks
                    };
                });
        }

        private IQueryable<Release> HydrateReleaseQuery(IQueryable<Release> queryable)
        {
            // Using `AsSplitQuery` as the generated SQL without it is incredibly
            // inefficient. Previously, we had dealt with this by splitting out
            // individual queries and hydrating the release manually.
            // We should keep an eye on this in case `AsSplitQuery` is not as
            // performant as running individual queries, and revert this if required.
            return queryable
                .AsSplitQuery()
                .Include(r => r.Publication)
                .ThenInclude(publication => publication.Contact)
                .Include(r => r.Publication)
                .ThenInclude(publication => publication.Releases)
                .Include(r => r.Publication)
                .ThenInclude(publication => publication.LegacyReleases)
                .Include(r => r.Publication)
                .ThenInclude(publication => publication.Topic.Theme)
                .Include(r => r.Content)
                .ThenInclude(join => join.ContentSection)
                .ThenInclude(section => section.Content)
                .ThenInclude(content => content.Comments)
                .ThenInclude(comment => comment.CreatedBy)
                .Include(r => r.Content)
                .ThenInclude(join => join.ContentSection)
                .ThenInclude(section => section.Content)
                .ThenInclude(content => content.LockedBy)
                .Include(r => r.Content)
                .ThenInclude(join => join.ContentSection)
                .ThenInclude(section => section.Content)
                .ThenInclude(contentBlock => (contentBlock as EmbedBlockLink)!.EmbedBlock)
                .Include(r => r.KeyStatistics)
                .Include(r => r.Updates);
        }
    }
}
