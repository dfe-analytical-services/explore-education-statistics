#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
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

namespace GovUk.Education.ExploreEducationStatistics.Content.Services
{
    public class ReleaseService : IReleaseService
    {
        private readonly ContentDbContext _contentDbContext;
        private readonly IPersistenceHelper<ContentDbContext> _persistenceHelper;
        private readonly IReleaseFileRepository _releaseFileRepository;
        private readonly IReleaseVersionRepository _releaseVersionRepository;
        private readonly IUserService _userService;
        private readonly IMapper _mapper;

        private static readonly Regex CommentsRegex =
            new(ContentFilterUtils.CommentsFilterPattern, RegexOptions.Compiled);

        public ReleaseService(
            ContentDbContext contentDbContext,
            IPersistenceHelper<ContentDbContext> persistenceHelper,
            IReleaseFileRepository releaseFileRepository,
            IReleaseVersionRepository releaseVersionRepository,
            IUserService userService,
            IMapper mapper)
        {
            _contentDbContext = contentDbContext;
            _persistenceHelper = persistenceHelper;
            _releaseFileRepository = releaseFileRepository;
            _releaseVersionRepository = releaseVersionRepository;
            _userService = userService;
            _mapper = mapper;
        }

        public async Task<Either<ActionResult, ReleaseCacheViewModel>> GetRelease(string publicationSlug,
            string? releaseSlug = null)
        {
            return await _persistenceHelper.CheckEntityExists<Publication>(q =>
                    q.Where(p => p.Slug == publicationSlug))
                .OnSuccess(async publication =>
                {
                    // If no release is requested get the latest published release version
                    if (releaseSlug == null)
                    {
                        return await _releaseVersionRepository.GetLatestPublishedReleaseVersion(publication.Id)
                            .OrNotFound();
                    }

                    // Otherwise get the latest published version of the requested release
                    return await _releaseVersionRepository.GetLatestPublishedReleaseVersion(publication.Id, releaseSlug)
                        .OrNotFound();
                })
                .OnSuccess(releaseVersion => GetRelease(releaseVersion.Id));
        }

        public async Task<Either<ActionResult, ReleaseCacheViewModel>> GetRelease(Guid releaseVersionId,
            DateTime? expectedPublishDate = null)
        {
            // Note this method is allowed to return a view model for an unpublished release version so that Publisher
            // can use it to cache a release version in advance of it going live.

            var releaseVersion = _contentDbContext
                .ReleaseVersions
                .Include(rv => rv.Content)
                .ThenInclude(section => section.Content)
                .ThenInclude(block => (block as EmbedBlockLink)!.EmbedBlock)
                .Include(rv => rv.Updates)
                .Include(rv => rv.KeyStatistics)
                .Single(rv => rv.Id == releaseVersionId);

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
            releaseViewModel.Published ??= await _releaseVersionRepository.GetPublishedDate(releaseVersion.Id,
                expectedPublishDate ?? DateTime.UtcNow);

            return releaseViewModel;
        }

        public async Task<Either<ActionResult, List<ReleaseSummaryViewModel>>> List(string publicationSlug)
        {
            return await _persistenceHelper.CheckEntityExists<Publication>(
                    q => q
                        .Where(p => p.Slug == publicationSlug)
                )
                .OnSuccess(_userService.CheckCanViewPublication)
                .OnSuccess(async publication =>
                {
                    var publishedReleaseVersions =
                        await _releaseVersionRepository.ListLatestPublishedReleaseVersions(publication.Id);
                    return publishedReleaseVersions
                        .Select(releaseVersion => new ReleaseSummaryViewModel(releaseVersion,
                            latestPublishedRelease: releaseVersion.Id == publication.LatestPublishedReleaseVersionId))
                        .ToList();
                });
        }

        private static void FilterContentBlock(IContentBlockViewModel block)
        {
            switch (block)
            {
                case HtmlBlockViewModel htmlBlock:
                    htmlBlock.Body = CommentsRegex.Replace(htmlBlock.Body, string.Empty);
                    break;

                case MarkDownBlockViewModel markdownBlock:
                    markdownBlock.Body = CommentsRegex.Replace(markdownBlock.Body, string.Empty);
                    break;
            }
        }

        private async Task<List<FileInfo>> GetDownloadFiles(ReleaseVersion releaseVersion)
        {
            var files = await _releaseFileRepository.GetByFileType(releaseVersion.Id, types: FileType.Ancillary,
                FileType.Data);
            return files
                .Select(rf => rf.ToPublicFileInfo())
                .OrderBy(file => file.Name)
                .ToList();
        }
    }
}
