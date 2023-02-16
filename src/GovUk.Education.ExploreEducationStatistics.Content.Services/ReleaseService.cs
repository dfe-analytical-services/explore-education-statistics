#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AutoMapper;
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
using GovUk.Education.ExploreEducationStatistics.Content.Services.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services
{
    public class ReleaseService : IReleaseService
    {
        private readonly ContentDbContext _contentDbContext;
        private readonly IPersistenceHelper<ContentDbContext> _persistenceHelper;
        private readonly IReleaseFileRepository _releaseFileRepository;
        private readonly IReleaseRepository _releaseRepository;
        private readonly IUserService _userService;
        private readonly IMapper _mapper;

        private static readonly Regex CommentsRegex =
            new(ContentFilterUtils.CommentsFilterPattern, RegexOptions.Compiled);

        public ReleaseService(
            ContentDbContext contentDbContext,
            IPersistenceHelper<ContentDbContext> persistenceHelper,
            IReleaseFileRepository releaseFileRepository,
            IReleaseRepository releaseRepository,
            IUserService userService,
            IMapper mapper)
        {
            _contentDbContext = contentDbContext;
            _persistenceHelper = persistenceHelper;
            _releaseFileRepository = releaseFileRepository;
            _releaseRepository = releaseRepository;
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
                    // If no release is requested get the latest published release
                    if (releaseSlug == null)
                    {
                        return await _releaseRepository.GetLatestPublishedRelease(publication.Id);
                    }

                    // Otherwise get the latest published version of the requested release
                    await _contentDbContext.Entry(publication)
                        .Collection(p => p.Releases)
                        .LoadAsync();

                    var latestPublishedVersionOfRelease = publication.Releases.SingleOrDefault(r =>
                        r.Slug == releaseSlug && r.IsLatestPublishedVersionOfRelease());

                    return latestPublishedVersionOfRelease ?? new Either<ActionResult, Release>(new NotFoundResult());
                }).OnSuccess(release => GetRelease(release.Id));
        }

        public async Task<Either<ActionResult, ReleaseCacheViewModel>> GetRelease(Guid releaseId,
            DateTime? expectedPublishDate = null)
        {
            // Note this method is allowed to return an unpublished Release so that Publisher can use it
            // to cache a release in advance of it going live.

            var release = _contentDbContext.Releases
                .Include(r => r.Content)
                .ThenInclude(releaseContentSection => releaseContentSection.ContentSection)
                .ThenInclude(section => section.Content)
                .ThenInclude(contentBlock => (contentBlock as EmbedBlockLink)!.EmbedBlock)
                .Include(r => r.Updates)
                .Include(r => r.KeyStatistics)
                .Single(r => r.Id == releaseId);

            var releaseViewModel = _mapper.Map<ReleaseCacheViewModel>(release);

            // Filter content blocks to remove any non-public or unnecessary information
            releaseViewModel.HeadlinesSection?.Content.ForEach(FilterContentBlock);
            releaseViewModel.SummarySection?.Content.ForEach(FilterContentBlock);
            releaseViewModel.KeyStatisticsSecondarySection?.Content.ForEach(FilterContentBlock);
            releaseViewModel.RelatedDashboardsSection?.Content.ForEach(FilterContentBlock);
            releaseViewModel.Content.ForEach(section => section.Content.ForEach(FilterContentBlock));

            releaseViewModel.DownloadFiles = await GetDownloadFiles(release);

            // If the view model has no mapped published date because it's not published, set a date
            // based on what we expect it to be when publishing completes
            releaseViewModel.Published ??= await _releaseRepository.GetPublishedDate(release.Id,
                expectedPublishDate ?? DateTime.UtcNow);

            return releaseViewModel;
        }

        public async Task<Either<ActionResult, List<ReleaseSummaryViewModel>>> List(string publicationSlug)
        {
            return await _persistenceHelper.CheckEntityExists<Publication>(
                    q => q
                        .Include(p => p.Releases)
                        .Where(p => p.Slug == publicationSlug)
                )
                .OnSuccess(_userService.CheckCanViewPublication)
                .OnSuccess(publication => publication.GetPublishedReleases()
                    .OrderByDescending(r => r.Year)
                    .ThenByDescending(r => r.TimePeriodCoverage)
                    .Select(release => new ReleaseSummaryViewModel(release))
                    .ToList()
                );
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

        private async Task<List<FileInfo>> GetDownloadFiles(Release release)
        {
            var files = await _releaseFileRepository.GetByFileType(release.Id, FileType.Ancillary, FileType.Data);
            return files
                .Select(rf => rf.ToPublicFileInfo())
                .OrderBy(file => file.Name)
                .ToList();
        }
    }
}
