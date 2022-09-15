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
        private readonly IUserService _userService;
        private readonly IMapper _mapper;

        private static readonly Regex
            FilterRegex = new(ContentFilterUtils.CommentsFilterPattern, RegexOptions.Compiled);

        public ReleaseService(
            ContentDbContext contentDbContext,
            IPersistenceHelper<ContentDbContext> persistenceHelper,
            IReleaseFileRepository releaseFileRepository,
            IUserService userService,
            IMapper mapper)
        {
            _contentDbContext = contentDbContext;
            _persistenceHelper = persistenceHelper;
            _releaseFileRepository = releaseFileRepository;
            _userService = userService;
            _mapper = mapper;
        }

        public async Task<Either<ActionResult, CachedReleaseViewModel>> GetRelease(string publicationSlug,
            string? releaseSlug = null)
        {
            return await _persistenceHelper.CheckEntityExists<Publication>(q =>
                    q.Include(p => p.Releases)
                        .Where(p => p.Slug == publicationSlug)
                )
                .OnSuccess(publication =>
                {
                    // If no release is requested get the latest published version of the latest published release.
                    // Otherwise get the latest published version of the requested release.
                    return releaseSlug == null
                        ? publication.LatestPublishedRelease()
                        : publication.Releases
                            .SingleOrDefault(r => r.Slug == releaseSlug && r.IsLatestPublishedVersionOfRelease());
                }).OnSuccess(async release =>
                {
                    if (release == null)
                    {
                        return new NotFoundResult();
                    }
                    // Build the view model for the published release
                    return await GetRelease(release.Id);
                });
        }

        public async Task<Either<ActionResult, CachedReleaseViewModel>> GetRelease(Guid releaseId,
            DateTime? expectedPublishDate = null)
        {
            // Note this method is allowed to return an unpublished Release so that Publisher can use it
            // to cache a release in advance of it going live.

            var release = _contentDbContext.Releases
                .Include(r => r.Content)
                .ThenInclude(releaseContentSection => releaseContentSection.ContentSection)
                .ThenInclude(section => section.Content)
                .Include(r => r.Updates)
                .Single(r => r.Id == releaseId);

            // TODO EES-3650 Could this be !Live instead or use the scheduled published date?
            if (!release.Published.HasValue && !expectedPublishDate.HasValue)
            {
                throw new ArgumentException("Expected published date must be specified for a non-live release",
                    nameof(expectedPublishDate));
            }

            var releaseViewModel = _mapper.Map<CachedReleaseViewModel>(release);

            // Filter content blocks to remove any unnecessary information
            releaseViewModel.HeadlinesSection?.Content.ForEach(FilterContentBlock);
            releaseViewModel.SummarySection?.Content.ForEach(FilterContentBlock);
            releaseViewModel.KeyStatisticsSection?.Content.ForEach(FilterContentBlock);
            releaseViewModel.KeyStatisticsSecondarySection?.Content.ForEach(FilterContentBlock);
            releaseViewModel.RelatedDashboardsSection?.Content.ForEach(FilterContentBlock);
            releaseViewModel.Content.ForEach(section => section.Content.ForEach(FilterContentBlock));

            releaseViewModel.DownloadFiles = await GetDownloadFiles(release);

            // If the release has no published date yet because it's not live, set the published date in the view model.
            // This is based on what we expect it to eventually be set as in the database when publishing completes
            if (!releaseViewModel.Published.HasValue)
            {
                if (release.Amendment)
                {
                    // For amendments this will be the published date of the previous release
                    var previousVersion = await _contentDbContext.Releases
                        .FindAsync(release.PreviousVersionId);

                    if (!previousVersion.Published.HasValue)
                    {
                        throw new ArgumentException(
                            $"Expected Release {previousVersion.Id} to have a Published date as the previous version of Release {release.Id}");
                    }

                    releaseViewModel.Published = previousVersion.Published;
                }
                else
                {
                    releaseViewModel.Published = expectedPublishDate;
                }
            }

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
                .OnSuccess(publication => publication.Releases
                    .Where(release => release.IsLatestPublishedVersionOfRelease())
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
                    htmlBlock.Body = FilterRegex.Replace(htmlBlock.Body, string.Empty);
                    break;

                case MarkDownBlockViewModel markdownBlock:
                    markdownBlock.Body = FilterRegex.Replace(markdownBlock.Body, string.Empty);
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
