#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Extensions;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Publisher.Model.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Publisher.Models;
using GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using static GovUk.Education.ExploreEducationStatistics.Common.Model.FileType;
using static GovUk.Education.ExploreEducationStatistics.Common.BlobContainers;
using static GovUk.Education.ExploreEducationStatistics.Publisher.Extensions.PublisherExtensions;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Services
{
    public class ReleaseService : IReleaseService
    {
        private readonly ContentDbContext _contentDbContext;
        private readonly PublicStatisticsDbContext _publicStatisticsDbContext;
        private readonly IBlobStorageService _publicBlobStorageService;
        private readonly IMethodologyService _methodologyService;
        private readonly IReleaseSubjectRepository _releaseSubjectRepository;
        private readonly ILogger<ReleaseService> _logger;
        private readonly IMapper _mapper;

        public ReleaseService(ContentDbContext contentDbContext,
            PublicStatisticsDbContext publicStatisticsDbContext,
            IBlobStorageService publicBlobStorageService,
            IMethodologyService methodologyService,
            IReleaseSubjectRepository releaseSubjectRepository,
            ILogger<ReleaseService> logger,
            IMapper mapper)
        {
            _contentDbContext = contentDbContext;
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
                .Where(release => ids.Contains(release.Id))
                .Include(release => release.Publication)
                .Include(release => release.PreviousVersion)
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
                .Include(r => r.Type)
                .Include(r => r.Content)
                .ThenInclude(releaseContentSection => releaseContentSection.ContentSection)
                .ThenInclude(section => section.Content)
                .Include(r => r.Publication)
                .Include(r => r.Updates)
                .Single(r => r.Id == id);

            var releaseViewModel = _mapper.Map<CachedReleaseViewModel>(release);
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

            // The Release in the statistics database can be absent if no Subjects were created
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

            var filesWithInfo = await files
                .SelectAsync(async file => await GetPublicFileInfo(release, file));

            var orderedFiles = filesWithInfo
                .OrderBy(file => file.Name);

            // Prepend the "All files" zip
            var allFilesZip = await GetAllFilesZip(release);
            return orderedFiles.Prepend(allFilesZip).ToList();
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
            await RemoveStatisticalReleases(previousVersions);

            await _publicStatisticsDbContext.SaveChangesAsync();
        }

        private async Task<FileInfo> GetAllFilesZip(Release release)
        {
            var path = release.AllFilesZipPath();

            var exists = await _publicBlobStorageService.CheckBlobExists(
                containerName: PublicReleaseFiles,
                path: path);

            // EES-1755 we should throw an exception here and not be as lenient.
            // Temporarily to collect a list of missing files and not halt publishing while regenerating
            // content for all releases, we log an error and continue.
            if (!exists)
            {
                _logger.LogError("Public blob not found for 'All files' zip at: {0}", path);
                return new FileInfo
                {
                    Id = null,
                    FileName = release.AllFilesZipFileName(),
                    Name = "Unknown",
                    Size = "0.00 B",
                    Type = Ancillary
                };
            }

            var blob = await _publicBlobStorageService.GetBlob(
                containerName: PublicReleaseFiles,
                path: path);

            return new FileInfo
            {
                Id = null,
                FileName = blob.FileName,
                Name = "All files",
                Size = blob.Size,
                Type = Ancillary
            };
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
                .Where(r => releaseIds.Contains(r.Id))
                .ToListAsync();

            _publicStatisticsDbContext.Release.RemoveRange(releases);
        }
    }
}
