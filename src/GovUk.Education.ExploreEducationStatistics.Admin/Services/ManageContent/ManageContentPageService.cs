#nullable enable
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.ManageContent;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.ManageContent;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static GovUk.Education.ExploreEducationStatistics.Common.Model.FileType;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.ManageContent;

public class ManageContentPageService : IManageContentPageService
{
    private readonly ContentDbContext _contentDbContext;
    private readonly IPersistenceHelper<ContentDbContext> _persistenceHelper;
    private readonly IMapper _mapper;
    private readonly IDataBlockService _dataBlockService;
    private readonly IMethodologyVersionRepository _methodologyVersionRepository;
    private readonly IReleaseFileService _releaseFileService;
    private readonly IReleaseRepository _releaseRepository;
    private readonly IUserService _userService;

    public ManageContentPageService(
        ContentDbContext contentDbContext,
        IPersistenceHelper<ContentDbContext> persistenceHelper,
        IMapper mapper,
        IDataBlockService dataBlockService,
        IMethodologyVersionRepository methodologyVersionRepository,
        IReleaseFileService releaseFileService,
        IReleaseRepository releaseRepository,
        IUserService userService)
    {
        _contentDbContext = contentDbContext;
        _persistenceHelper = persistenceHelper;
        _mapper = mapper;
        _dataBlockService = dataBlockService;
        _methodologyVersionRepository = methodologyVersionRepository;
        _releaseFileService = releaseFileService;
        _releaseRepository = releaseRepository;
        _userService = userService;
    }

    public async Task<Either<ActionResult, ManageContentPageViewModel>> GetManageContentPageViewModel(
        Guid releaseVersionId,
        bool isPrerelease = false)
    {
        return await _persistenceHelper
            .CheckEntityExists<ReleaseVersion>(releaseVersionId, HydrateReleaseQuery)
            .OnSuccess(_userService.CheckCanViewReleaseVersion)
            .OnSuccessCombineWith(_ => _dataBlockService.GetUnattachedDataBlocks(releaseVersionId))
            .OnSuccessCombineWith(_ => _releaseFileService.ListAll(
                releaseVersionId,
                Ancillary,
                FileType.Data))
            .OnSuccess(async releaseVersionBlocksAndFiles =>
            {
                var (releaseVersion, unattachedDataBlocks, files) = releaseVersionBlocksAndFiles;

                var methodologyVersions =
                    await _methodologyVersionRepository.GetLatestVersionByPublication(releaseVersion.PublicationId);

                if (isPrerelease)
                {
                    // Get latest approved version
                    methodologyVersions = await methodologyVersions
                        .ToAsyncEnumerable()
                        .SelectAwait(async version =>
                        {
                            if (version.Status == MethodologyApprovalStatus.Approved)
                            {
                                return version;
                            }

                            if (version.PreviousVersionId == null)
                            {
                                return null;
                            }

                            // If there is a previous version, it must be approved, because cannot
                            // create an amendment for an unpublished version
                            return await _contentDbContext.MethodologyVersions
                                .FirstAsync(mv => mv.Id == version.PreviousVersionId);
                        })
                        .WhereNotNull()
                        .ToListAsync();
                }

                var releaseViewModel = _mapper.Map<ManageContentPageViewModel.ReleaseViewModel>(releaseVersion);

                var publishedReleases =
                    await _releaseRepository.ListPublishedReleases(releaseVersion.PublicationId);

                // Hydrate Publication.ReleaseSeries
                releaseViewModel.Publication.ReleaseSeries =
                    BuildReleaseSeriesItemViewModels(releaseVersion.Publication, publishedReleases);

                releaseViewModel.DownloadFiles = files.ToList();
                releaseViewModel.Publication.Methodologies =
                    _mapper.Map<List<IdTitleViewModel>>(methodologyVersions);

                return new ManageContentPageViewModel
                {
                    Release = releaseViewModel,
                    UnattachedDataBlocks = unattachedDataBlocks
                };
            });
    }

    private static List<ReleaseSeriesItemViewModel> BuildReleaseSeriesItemViewModels(
        Publication publication,
        List<Release> publishedReleases)
    {
        var publishedReleasesById = publishedReleases.ToDictionary(r => r.Id);
        return publication.ReleaseSeries
            // Only include release series items for legacy links and published releases
            .Where(rsi => rsi.IsLegacyLink || publishedReleasesById.ContainsKey(rsi.ReleaseId!.Value))
            .Select(rsi =>
            {
                if (rsi.IsLegacyLink)
                {
                    return new ReleaseSeriesItemViewModel
                    {
                        Description = rsi.LegacyLinkDescription!,
                        LegacyLinkUrl = rsi.LegacyLinkUrl
                    };
                }

                var release = publishedReleasesById[rsi.ReleaseId!.Value];

                return new ReleaseSeriesItemViewModel
                {
                    Description = release.Title,
                    ReleaseId = release.Id,
                    ReleaseSlug = release.Slug
                };
            })
            .ToList();
    }

    private IQueryable<ReleaseVersion> HydrateReleaseQuery(IQueryable<ReleaseVersion> queryable)
    {
        // Using `AsSplitQuery` as the generated SQL without it is incredibly
        // inefficient. Previously, we had dealt with this by splitting out
        // individual queries and hydrating the release manually.
        // We should keep an eye on this in case `AsSplitQuery` is not as
        // performant as running individual queries, and revert this if required.
        return queryable
            .AsSplitQuery()
            .Include(rv => rv.Release)
            .ThenInclude(r => r.Publication)
            .ThenInclude(p => p.Contact)
            .Include(rv => rv.Release)
            .ThenInclude(r => r.Publication)
            .ThenInclude(p => p.Theme)
            .Include(rv => rv.Content)
            .ThenInclude(cs => cs.Content)
            .ThenInclude(cb => cb.Comments)
            .ThenInclude(c => c.CreatedBy)
            .Include(rv => rv.Content)
            .ThenInclude(cs => cs.Content)
            .ThenInclude(cb => cb.LockedBy)
            .Include(rv => rv.Content)
            .ThenInclude(cs => cs.Content)
            .ThenInclude(cb => (cb as EmbedBlockLink)!.EmbedBlock)
            .Include(rv => rv.KeyStatistics)
            .Include(rv => rv.Updates);
    }
}
