﻿using System;
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
using static GovUk.Education.ExploreEducationStatistics.Common.BlobContainerNames;
using static GovUk.Education.ExploreEducationStatistics.Common.Extensions.BlobInfoExtensions;
using static GovUk.Education.ExploreEducationStatistics.Common.Model.FileType;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.FileStoragePathUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.FileStorageUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services
{
    /**
     * Temporary service for migrating blobs to use their database file reference id as their filename
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

        public async Task<Either<ActionResult, Unit>> MigratePrivateFiles(FileType type)
        {
            return await _userService.CheckCanRunMigrations()
                .OnSuccessVoid(async () =>
                {
                    var files = await GetFiles(type);
                    await files.ForEachAsync(async file =>
                    {
                        try
                        {
                            await MovePrivateBlobForFile(file)
                                .OnSuccessDo(() => SetPrivateBlobMetadata(file))
                                .OnSuccessDo(() => MarkFileAsMigrated(file));
                        }
                        catch (Exception e)
                        {
                            _logger.LogError(e, "Caught exception renaming private file");
                        }
                    });
                });
        }

        public async Task<Either<ActionResult, Unit>> MigratePublicFiles(FileType type, bool lenient = false)
        {
            return await _userService.CheckCanRunMigrations()
                .OnSuccessVoid(async () =>
                {
                    var releases = await GetPublicReleases();
                    await releases.ForEachAsync(async release =>
                    {
                        var releaseFiles = await _releaseFileRepository.GetByFileType(release.Id, type);
                        await releaseFiles.ForEachAsync(async releaseFile =>
                        {
                            var file = releaseFile.File;
                            try
                            {
                                if (!lenient ||
                                    await _publicBlobStorageService.CheckBlobExists(
                                        containerName: PublicFilesContainerName,
                                        path: PublicReleasePath(release.Publication.Slug,
                                            release.Slug,
                                            file.Type,
                                            file.Filename)))
                                {
                                    await MovePublicBlobForFile(release, file)
                                        .OnSuccessDo(() => SetPublicBlobMetadata(release, file));
                                }
                            }
                            catch (Exception e)
                            {
                                _logger.LogError(e, "Caught exception renaming public file");
                            }
                        });
                    });
                });
        }

        private async Task SetPrivateBlobMetadata(File file)
        {
            if (file.Type == Ancillary)
            {
                await SetPrivateAncillaryBlobMetadata(file);
            }
        }

        private async Task SetPublicBlobMetadata(Release release, File file)
        {
            if (file.Type == Ancillary)
            {
                await SetPublicAncillaryBlobMetadata(release, file);
            }
        }

        private async Task SetPrivateAncillaryBlobMetadata(File file)
        {
            var blob = await _privateBlobStorageService.GetBlob(
                containerName: PrivateFilesContainerName,
                path: file.Path());

            await _privateBlobStorageService.SetMetadata(
                containerName: PrivateFilesContainerName,
                path: file.Path(),
                metadata: GetAncillaryFileMetaValues(
                    filename: file.Filename,
                    name: blob.Name)
                );
        }

        private async Task SetPublicAncillaryBlobMetadata(Release release, File file)
        {
            var blob = await _publicBlobStorageService.GetBlob(
                containerName: PublicFilesContainerName,
                path: file.PublicPath(release));

            var metadata = GetAncillaryFileMetaValues(
                filename: file.Filename,
                name: blob.Name);

            // Retain the ReleaseDateTime which was added during publishing
            if (blob.Meta.TryGetValue(ReleaseDateTimeKey, out var releaseDateTime))
            {
                metadata.Add(ReleaseDateTimeKey, releaseDateTime);
            }
            else
            {
                _logger.LogWarning("Public blob found without ReleaseDateTime key: {0}", blob.Path);
            }

            await _publicBlobStorageService.SetMetadata(
                containerName: PublicFilesContainerName,
                path: file.PublicPath(release),
                metadata: metadata);
        }

        private async Task<List<File>> GetFiles(FileType type)
        {
            return await _contentDbContext
                .Files
                .Where(file => file.Type == type && !file.FilenameMigrated)
                .ToListAsync();
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

        private async Task MarkFileAsMigrated(File file)
        {
            _contentDbContext.Update(file);
            file.FilenameMigrated = true;
            await _contentDbContext.SaveChangesAsync();
        }

        private async Task<Either<ActionResult, Unit>> MovePrivateBlobForFile(File file)
        {
            var sourcePath = AdminReleasePath(file.ReleaseId, file.Type, file.Filename);

            _logger.LogInformation("Renaming private file: {0}", sourcePath);

            if (!await _privateBlobStorageService.MoveBlob(
                containerName: PrivateFilesContainerName,
                sourcePath: sourcePath,
                destinationPath: file.Path()))
            {
                throw new InvalidOperationException($"Unable to move private blob for file: {file.Id} - {sourcePath}");
            }

            return Unit.Instance;
        }

        private async Task<Either<ActionResult, Unit>> MovePublicBlobForFile(Release release, File file)
        {
            var sourcePath = PublicReleasePath(release.Publication.Slug, release.Slug, file.Type,
                file.Filename);

            _logger.LogInformation("Renaming public file: {0}", sourcePath);

            if (!await _publicBlobStorageService.MoveBlob(
                containerName: PublicFilesContainerName,
                sourcePath: sourcePath,
                destinationPath: file.PublicPath(release)))
            {
                throw new InvalidOperationException($"Unable to move public blob for file: {file.Id} - {sourcePath}");
            }

            return Unit.Instance;
        }
    }
}