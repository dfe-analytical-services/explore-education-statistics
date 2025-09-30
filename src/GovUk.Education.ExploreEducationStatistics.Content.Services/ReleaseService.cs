using System.Text.RegularExpressions;
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Security.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FileInfo = GovUk.Education.ExploreEducationStatistics.Common.Model.FileInfo;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services;

public class ReleaseService : IReleaseService
{
    private readonly ContentDbContext _contentDbContext;
    private readonly IReleaseFileRepository _releaseFileRepository;
    private readonly IReleaseVersionRepository _releaseVersionRepository;
    private readonly IUserService _userService;
    private readonly IMapper _mapper;

    private static readonly Regex CommentsRegex = new(
        ContentFilterUtils.CommentsFilterPattern,
        RegexOptions.Compiled
    );

    public ReleaseService(
        ContentDbContext contentDbContext,
        IReleaseFileRepository releaseFileRepository,
        IReleaseVersionRepository releaseVersionRepository,
        IUserService userService,
        IMapper mapper
    )
    {
        _contentDbContext = contentDbContext;
        _releaseFileRepository = releaseFileRepository;
        _releaseVersionRepository = releaseVersionRepository;
        _userService = userService;
        _mapper = mapper;
    }

    public async Task<Either<ActionResult, ReleaseCacheViewModel>> GetRelease(
        string publicationSlug,
        string? releaseSlug = null
    )
    {
        return await _contentDbContext
            .Publications.SingleOrNotFoundAsync(p => p.Slug == publicationSlug)
            .OnSuccess(async publication =>
            {
                // If no release is requested use the publication's latest published release version,
                // otherwise use the latest published version of the requested release
                var latestReleaseVersionId =
                    releaseSlug == null
                        ? publication.LatestPublishedReleaseVersionId
                        : (
                            await _releaseVersionRepository.GetLatestPublishedReleaseVersionByReleaseSlug(
                                publication.Id,
                                releaseSlug
                            )
                        )?.Id;

                return latestReleaseVersionId.HasValue
                    ? await GetRelease(latestReleaseVersionId.Value)
                    : new NotFoundResult();
            });
    }

    public async Task<Either<ActionResult, ReleaseCacheViewModel>> GetRelease(
        Guid releaseVersionId,
        DateTime? expectedPublishDate = null
    )
    {
        // Note this method is allowed to return a view model for an unpublished release version so that Publisher
        // can use it to cache a release version in advance of it going live.

        var releaseVersion = await _contentDbContext
            .ReleaseVersions.Include(rv => rv.Release)
            .Include(rv => rv.PublishingOrganisations)
            .Include(rv => rv.Content)
            .ThenInclude(cs => cs.Content)
            .ThenInclude(cb => (cb as EmbedBlockLink)!.EmbedBlock)
            .Include(rv => rv.Updates)
            .Include(rv => rv.KeyStatistics)
            .SingleAsync(rv => rv.Id == releaseVersionId);

        var releaseViewModel = _mapper.Map<ReleaseCacheViewModel>(releaseVersion);

        // Filter content blocks to remove any non-public or unnecessary information
        releaseViewModel.HeadlinesSection?.Content.ForEach(FilterContentBlock);
        releaseViewModel.SummarySection?.Content.ForEach(FilterContentBlock);
        releaseViewModel.KeyStatisticsSecondarySection?.Content.ForEach(FilterContentBlock);
        releaseViewModel.RelatedDashboardsSection?.Content.ForEach(FilterContentBlock);
        releaseViewModel.Content.ForEach(section => section.Content.ForEach(FilterContentBlock));

        releaseViewModel.DownloadFiles = await GetDownloadFiles(releaseVersion);

        // If the view model has no mapped published date because it's not published, set a date
        // based on what we expect it to be when publishing completes
        releaseViewModel.Published ??= await _releaseVersionRepository.GetPublishedDate(
            releaseVersion.Id,
            expectedPublishDate ?? DateTime.UtcNow
        );

        return releaseViewModel;
    }

    public async Task<Either<ActionResult, List<ReleaseSummaryViewModel>>> List(
        string publicationSlug
    )
    {
        return await _contentDbContext
            .Publications.SingleOrNotFoundAsync(p => p.Slug == publicationSlug)
            .OnSuccess(_userService.CheckCanViewPublication)
            .OnSuccess(async publication =>
            {
                var publishedReleaseVersions =
                    await _releaseVersionRepository.ListLatestReleaseVersions(
                        publication.Id,
                        publishedOnly: true
                    );

                return await publishedReleaseVersions
                    .ToAsyncEnumerable()
                    .SelectAwait(async releaseVersion =>
                    {
                        await _contentDbContext
                            .ReleaseVersions.Entry(releaseVersion)
                            .Reference(rv => rv.Release)
                            .LoadAsync();

                        return new ReleaseSummaryViewModel(
                            releaseVersion,
                            latestPublishedRelease: releaseVersion.Id
                                == publication.LatestPublishedReleaseVersionId
                        );
                    })
                    .ToListAsync();
            });
    }

    private static void FilterContentBlock(IContentBlockViewModel block)
    {
        switch (block)
        {
            case HtmlBlockViewModel htmlBlock:
                htmlBlock.Body = CommentsRegex.Replace(htmlBlock.Body, string.Empty);
                break;
        }
    }

    private async Task<List<FileInfo>> GetDownloadFiles(ReleaseVersion releaseVersion)
    {
        var files = await _releaseFileRepository.GetByFileType(
            releaseVersion.Id,
            types: [FileType.Ancillary, FileType.Data]
        );

        return files.Select(rf => rf.ToPublicFileInfo()).OrderBy(file => file.Name).ToList();
    }
}
