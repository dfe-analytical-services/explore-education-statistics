using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using static GovUk.Education.ExploreEducationStatistics.Common.BlobContainers;
using static GovUk.Education.ExploreEducationStatistics.Common.Model.FileType;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.Extensions.FileExtensions;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services
{
    /**
     * Temporary service for migrating blobs to use a new file structure
     */
    public class MigrateFilesService : IMigrateFilesService
    {
        private readonly ContentDbContext _contentDbContext;
        private readonly IBlobStorageService _privateBlobStorageService;
        private readonly IBlobStorageService _publicBlobStorageService;
        private readonly IReleaseFileRepository _releaseFileRepository;
        private readonly IUserService _userService;
        private readonly ILogger<MigrateFilesService> _logger;

        public MigrateFilesService(ContentDbContext contentDbContext,
            IBlobStorageService privateBlobStorageService,
            IBlobStorageService publicBlobStorageService,
            IReleaseFileRepository releaseFileRepository,
            IUserService userService,
            ILogger<MigrateFilesService> logger)
        {
            _contentDbContext = contentDbContext;
            _privateBlobStorageService = privateBlobStorageService;
            _publicBlobStorageService = publicBlobStorageService;
            _releaseFileRepository = releaseFileRepository;
            _userService = userService;
            _logger = logger;
        }

        public async Task<Either<ActionResult, Unit>> MigratePrivateFiles(params FileType[] type)
        {
            return await _userService.CheckCanRunMigrations()
                .OnSuccessVoid(async () =>
                {
                    // Get all private files unless they are already migrated
                    var files = await GetPrivateFiles(type);
                    await files.ForEachAsync(async file =>
                    {
                        try
                        {
                            await MovePrivateBlobForFile(file)
                                .OnSuccessDo(() => SetPrivateBlobMetadata(file))
                                .OnSuccessDo(() => MarkFileAsPrivateMigrated(file));
                        }
                        catch (Exception e)
                        {
                            _logger.LogError(e, "Caught exception moving private file");
                        }
                    });
                });
        }

        public async Task<Either<ActionResult, Unit>> MigratePublicFiles(FileType type)
        {
            if (!PublicFileTypes.Contains(type))
            {
                throw new ArgumentOutOfRangeException(nameof(type), type, "Cannot migrate public files for file type");
            }

            return await _userService.CheckCanRunMigrations()
                .OnSuccessVoid(async () =>
                {
                    // Get all public Releases
                    var releases = await GetPublicReleases();
                    await releases.ForEachAsync(async release =>
                    {
                        // Get all files linked to that Release unless they are already migrated
                        var files = await GetPublicFiles(release.Id, type);
                        await files.ForEachAsync(async file =>
                        {
                            try
                            {
                                await MovePublicBlobForFile(release, file)
                                    .OnSuccessDo(() => MarkFileAsPublicMigrated(file));
                            }
                            catch (Exception e)
                            {
                                _logger.LogError(e, "Caught exception moving public file");
                            }
                        });

                        if (type == Ancillary)
                        {
                            try
                            {
                                await MovePublicAllFilesZipFile(release);
                            }
                            catch (Exception e)
                            {
                                _logger.LogError(e, "Caught exception moving public All Files zip file");
                            }
                        }
                    });
                });
        }

        private async Task SetPrivateBlobMetadata(File file)
        {
            // Transform the blob properties if required depending on file type
            switch (file.Type)
            {
                case Metadata:
                    await SetPrivateMetadataFileBlobProperties(file);
                    return;
            }
        }

        private async Task SetPrivateMetadataFileBlobProperties(File file)
        {
            if (file.Type != Metadata)
            {
                throw new ArgumentException("Expected file of type Metadata");
            }

            // Take this opportunity to wipe the properties of the blob now that no properties are used for this type

            await _privateBlobStorageService.SetMetadata(
                containerName: PrivateReleaseFiles,
                path: file.MigratedPath(),
                metadata: new Dictionary<string, string>()
            );
        }

        private async Task<List<File>> GetPrivateFiles(params FileType[] types)
        {
            return await _contentDbContext
                .Files
                .Where(file => types.Contains(file.Type) && !file.PrivateBlobPathMigrated)
                .ToListAsync();
        }

        private async Task<List<File>> GetPublicFiles(Guid releaseId, FileType type)
        {
            var releaseFiles = await _releaseFileRepository.GetByFileType(releaseId, type);

            return releaseFiles.Where(rf => !rf.File.PublicBlobPathMigrated)
                .Select(rf => rf.File)
                .ToList();
        }

        private async Task<List<Release>> GetPublicReleases()
        {
            var publications = await _contentDbContext.Publications
                .Include(publication => publication.Releases)
                .ToListAsync();

            return publications.SelectMany(publication => publication.Releases)
                .Where(release => release.IsLatestPublishedVersionOfRelease())
                .ToList();
        }

        private async Task<Either<ActionResult, Unit>> MovePrivateBlobForFile(File file)
        {
            var sourcePath = file.Path();
            var destinationPath = file.MigratedPath();

            if (!await _publicBlobStorageService.CheckBlobExists(
                containerName: PrivateReleaseFiles,
                path: sourcePath))
            {
                _logger.LogError("Private blob not found for file: {0} - {1}", file.Id, sourcePath);
                return new NotFoundResult();
            }

            _logger.LogInformation("Moving private file: {0} - {1} -> {2}", file.Id, sourcePath, destinationPath);

            if (!await _privateBlobStorageService.MoveBlob(
                containerName: PrivateReleaseFiles,
                sourcePath: sourcePath,
                destinationPath: destinationPath))
            {
                throw new InvalidOperationException(
                    $"Unable to move private blob for file: {file.Id} - {sourcePath} -> {destinationPath}");
            }

            return Unit.Instance;
        }

        private async Task<Either<ActionResult, Unit>> MovePublicBlobForFile(Release release, File file)
        {
            var sourcePath = file.PublicPath(release);
            var destinationPath = file.MigratedPublicPath(release);

            if (!await _publicBlobStorageService.CheckBlobExists(
                containerName: PublicReleaseFiles,
                path: sourcePath))
            {
                _logger.LogError("Public blob not found for file: {0} - {1}", file.Id, sourcePath);
                return new NotFoundResult();
            }

            _logger.LogInformation("Moving public file: {0} - {1} -> {2}", file.Id, sourcePath, destinationPath);

            if (!await _publicBlobStorageService.MoveBlob(
                containerName: PublicReleaseFiles,
                sourcePath: sourcePath,
                destinationPath: destinationPath))
            {
                throw new InvalidOperationException(
                    $"Unable to move public blob for file: {file.Id} - {sourcePath} -> {destinationPath}");
            }

            return Unit.Instance;
        }

        private async Task MovePublicAllFilesZipFile(Release release)
        {
            var sourcePath = LegacyAllFilesZipPath(release);
            var destinationPath = release.AllFilesZipPath();

            if (!await _publicBlobStorageService.CheckBlobExists(
                containerName: PublicReleaseFiles,
                path: sourcePath))
            {
                _logger.LogError("Public blob not found for All Files zip file: {0}", sourcePath);
                return;
            }

            _logger.LogInformation("Moving All Files zip file: {1} -> {2}", sourcePath, destinationPath);

            if (!await _publicBlobStorageService.MoveBlob(
                containerName: PublicReleaseFiles,
                sourcePath: sourcePath,
                destinationPath: destinationPath))
            {
                throw new InvalidOperationException(
                    $"Unable to move All Files zip file: {sourcePath} -> {destinationPath}");
            }
        }

        private async Task MarkFileAsPrivateMigrated(File file)
        {
            _contentDbContext.Update(file);
            file.PrivateBlobPathMigrated = true;
            await _contentDbContext.SaveChangesAsync();
        }

        private async Task MarkFileAsPublicMigrated(File file)
        {
            _contentDbContext.Update(file);
            file.PublicBlobPathMigrated = true;
            await _contentDbContext.SaveChangesAsync();
        }

        private static string LegacyAllFilesZipPath(Release release)
        {
            if (release.Publication == null)
            {
                throw new ArgumentException("Release must be hydrated with Publication to create legacy all files zip path");
            }

            return
                $"{release.Publication.Slug}/{release.Slug}/{Ancillary.GetEnumLabel()}/{release.Publication.Slug}_{release.Slug}.zip";
        }
    }
}
