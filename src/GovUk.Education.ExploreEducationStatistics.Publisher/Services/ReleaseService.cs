#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Utils;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Publisher.Model.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Publisher.Models;
using GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Publisher.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using static GovUk.Education.ExploreEducationStatistics.Common.Model.FileType;
using static GovUk.Education.ExploreEducationStatistics.Common.BlobContainers;
using static GovUk.Education.ExploreEducationStatistics.Publisher.Extensions.PublisherExtensions;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Services
{
    public class ReleaseService : IReleaseService
    {
        private static readonly Regex FilterRegex = new(ContentFilterUtils.CommentsFilterPattern, RegexOptions.Compiled);

        private readonly ContentDbContext _contentDbContext;
        private readonly StatisticsDbContext _statisticsDbContext;
        private readonly PublicStatisticsDbContext _publicStatisticsDbContext;
        private readonly IBlobStorageService _publicBlobStorageService;
        private readonly IMethodologyService _methodologyService;
        private readonly IReleaseSubjectRepository _releaseSubjectRepository;
        private readonly ILogger<ReleaseService> _logger;
        private readonly IMapper _mapper;

        public ReleaseService(ContentDbContext contentDbContext,
            StatisticsDbContext statisticsDbContext,
            PublicStatisticsDbContext publicStatisticsDbContext,
            IBlobStorageService publicBlobStorageService,
            IMethodologyService methodologyService,
            IReleaseSubjectRepository releaseSubjectRepository,
            ILogger<ReleaseService> logger,
            IMapper mapper)
        {
            _contentDbContext = contentDbContext;
            _statisticsDbContext = statisticsDbContext;
            _publicStatisticsDbContext = publicStatisticsDbContext;
            _publicBlobStorageService = publicBlobStorageService;
            _methodologyService = methodologyService;
            _releaseSubjectRepository = releaseSubjectRepository;
            _logger = logger;
            _mapper = mapper;
        }

        public async Task<Release?> Find(Guid id)
        {
            return await _contentDbContext.Releases
                .Include(release => release.Publication)
                .Include(r => r.PreviousVersion)
                .SingleOrDefaultAsync(release => release.Id == id);
        }

        public async Task<Release> Get(Guid id)
        {
            var release = await Find(id);

            if (release == null)
            {
                throw new ArgumentException($"Could not find release: {id}");
            }

            return release;
        }

        public async Task<IEnumerable<Release>> List(IEnumerable<Guid> ids)
        {
            return await _contentDbContext.Releases
                .AsQueryable()
                .Where(release => ids.Contains(release.Id))
                .Include(release => release.Publication)
                .Include(release => release.PreviousVersion)
                .ToAsyncEnumerable()
                .ToListAsync();
        }

        public async Task<IEnumerable<Release>> GetAmendedReleases(IEnumerable<Guid> releaseIds)
        {
            return await _contentDbContext.Releases
                .Include(r => r.PreviousVersion)
                .Include(r => r.Publication)
                .Where(r => releaseIds.Contains(r.Id) && r.PreviousVersionId != null)
                .ToListAsync();
        }

        public async Task<CachedReleaseViewModel> GetReleaseViewModel(Guid id, PublishContext context)
        {
            var release = _contentDbContext.Releases
                .Include(r => r.Content)
                .ThenInclude(releaseContentSection => releaseContentSection.ContentSection)
                .ThenInclude(section => section.Content)
                .Include(r => r.Publication)
                .Include(r => r.Updates)
                .Single(r => r.Id == id);

            var releaseViewModel = _mapper.Map<CachedReleaseViewModel>(release);

            // Filter content blocks to remove any unnecessary information
            releaseViewModel.HeadlinesSection?.Content.ForEach(FilterContentBlock);
            releaseViewModel.SummarySection?.Content.ForEach(FilterContentBlock);
            releaseViewModel.KeyStatisticsSection?.Content.ForEach(FilterContentBlock);
            releaseViewModel.KeyStatisticsSecondarySection?.Content.ForEach(FilterContentBlock);
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
                    // Otherwise it's either the time now or the next scheduled publishing time
                    // This date is set up by the calling function when execution begins
                    releaseViewModel.Published = context.Published;
                }
            }

            return releaseViewModel;
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

        public async Task<Release> GetLatestRelease(Guid publicationId, IEnumerable<Guid> includedReleaseIds)
        {
            var releases = await _contentDbContext.Releases
                .Include(r => r.Publication)
                .Where(release => release.PublicationId == publicationId)
                .ToListAsync();

            return releases
                .Where(release => release.IsReleasePublished(includedReleaseIds))
                .OrderBy(release => release.Year)
                .ThenBy(release => release.TimePeriodCoverage)
                .Last();
        }

        public async Task<CachedReleaseViewModel> GetLatestReleaseViewModel(Guid publicationId,
            IEnumerable<Guid> includedReleaseIds, PublishContext context)
        {
            var latestRelease = await GetLatestRelease(publicationId, includedReleaseIds);
            return await GetReleaseViewModel(latestRelease.Id, context);
        }

        public async Task SetPublishedDates(Guid id, DateTime published)
        {
            var contentRelease = await _contentDbContext.Releases
                .Include(release => release.Publication)
                .SingleOrDefaultAsync(r => r.Id == id);

            var statisticsRelease = await _publicStatisticsDbContext.Release
                .AsQueryable()
                .SingleOrDefaultAsync(r => r.Id == id);

            if (contentRelease == null)
            {
                throw new ArgumentException("Content Release does not exist", nameof(id));
            }

            if (contentRelease.Amendment)
            {
                var previousVersion = await _contentDbContext.Releases.AsNoTracking()
                    .SingleOrDefaultAsync(r => r.Id == contentRelease.PreviousVersionId);

                if (previousVersion?.Published == null)
                {
                    throw new ArgumentException("Previous version of release does not exist or is not live",
                        nameof(contentRelease.PreviousVersionId));
                }

                published = previousVersion.Published.Value;
            }

            _contentDbContext.Releases.Update(contentRelease);
            contentRelease.Published ??= published;
            contentRelease.DataLastPublished = DateTime.UtcNow;

            // Update the Publication published date since we always generate the Publication when generating Release Content
            contentRelease.Publication.Published = published;

            // Set the published date on any methodologies used by this publication that are now publicly accessible
            // as a result of this release being published
            await _methodologyService.SetPublishedDatesByPublication(contentRelease.PublicationId, published);

            await _contentDbContext.SaveChangesAsync();

            // The Release in the statistics database can be absent if no data files were ever created
            if (statisticsRelease != null)
            {
                _publicStatisticsDbContext.Release.Update(statisticsRelease);
                statisticsRelease.Published ??= published;
                await _publicStatisticsDbContext.SaveChangesAsync();
            }
        }

        public async Task<List<File>> GetFiles(Guid releaseId, params FileType[] types)
        {
            return await _contentDbContext
                .ReleaseFiles
                .Include(rf => rf.File)
                .Where(rf => rf.ReleaseId == releaseId)
                .Select(rf => rf.File)
                .Where(file => types.Contains(file.Type))
                .ToListAsync();
        }

        public async Task<List<FileInfo>> GetDownloadFiles(Release release)
        {
            var files = await GetFiles(release.Id, Ancillary, FileType.Data);

            return await files.ToAsyncEnumerable()
                .SelectAwait(async file => await GetPublicFileInfo(release, file))
                .OrderBy(file => file.Name)
                .ToListAsync();
        }

        public async Task CreatePublicStatisticsRelease(Guid releaseId)
        {
            if (!EnvironmentUtils.IsLocalEnvironment())
            {
                var statisticsRelease = await _statisticsDbContext.Release
                    .AsQueryable()
                    .SingleOrDefaultAsync(r => r.Id == releaseId);

                var publicStatisticsRelease = await _publicStatisticsDbContext.Release
                    .AsQueryable()
                    .SingleOrDefaultAsync(r => r.Id == releaseId);

                if (statisticsRelease != null && publicStatisticsRelease == null)
                {
                    await _publicStatisticsDbContext.Release.AddAsync(new Data.Model.Release
                    {
                        Id = statisticsRelease.Id,
                        PublicationId = statisticsRelease.PublicationId,
                        Year = statisticsRelease.Year,
                        TimeIdentifier = statisticsRelease.TimeIdentifier,
                        Slug = statisticsRelease.Slug,
                        PreviousVersionId = statisticsRelease.PreviousVersionId
                        // Published date is omitted here as it will be set when publishing completes
                    });
                    await _publicStatisticsDbContext.SaveChangesAsync();
                }
            }
        }

        public async Task DeletePreviousVersionsStatisticalData(params Guid[] releaseIds)
        {
            var releases = await GetAmendedReleases(releaseIds);
            var previousVersions = releases.Select(r => r.PreviousVersionId)
                .Where(id => id.HasValue)
                .Cast<Guid>()
                .ToList();

            foreach (var previousVersion in previousVersions)
            {
                await _releaseSubjectRepository.SoftDeleteAllReleaseSubjects(previousVersion);
            }

            // Remove Statistical Releases for each of the Content Releases
            // TODO EES-2817 There's a missing foreign key on PreviousVersionId back to Release
            // so this removes the previous versions successfully but leaves PreviousVersionId's that won't exist
            await RemoveStatisticalReleases(previousVersions);

            await _publicStatisticsDbContext.SaveChangesAsync();
        }

        private async Task<FileInfo> GetPublicFileInfo(Release release, File file)
        {
            var exists = await _publicBlobStorageService.CheckBlobExists(
                containerName: PublicReleaseFiles,
                path: file.PublicPath(release));

            if (!exists)
            {
                _logger.LogWarning("Public blob not found for file: {0} at: {1}", file.Id,
                    file.PublicPath(release));
                return file.ToFileInfoNotFound();
            }

            var blob = await _publicBlobStorageService.GetBlob(
                containerName: PublicReleaseFiles,
                path: file.PublicPath(release));

            var releaseFile = await _contentDbContext.ReleaseFiles
                .Include(rf => rf.File)
                .FirstAsync(rf =>
                    rf.ReleaseId == release.Id
                    && rf.FileId == file.Id);

            return releaseFile.ToPublicFileInfo(blob);
        }

        private async Task RemoveStatisticalReleases(IEnumerable<Guid> releaseIds)
        {
            var releases = await _publicStatisticsDbContext.Release
                .AsQueryable()
                .Where(r => releaseIds.Contains(r.Id))
                .ToListAsync();

            _publicStatisticsDbContext.Release.RemoveRange(releases);
        }
    }
}
