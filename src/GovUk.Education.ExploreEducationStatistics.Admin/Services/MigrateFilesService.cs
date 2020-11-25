using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Extensions;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using static GovUk.Education.ExploreEducationStatistics.Common.BlobContainerNames;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.FileStoragePathUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services
{
    /**
     * Temporary service for migrating blobs to use their database file reference id as their filename
     */
    public class MigrateFilesService : IMigrateFilesService
    {
        private readonly ContentDbContext _contentDbContext;
        private readonly IBlobStorageService _blobStorageService;
        private readonly IUserService _userService;
        private readonly ILogger<IBlobStorageService> _logger;

        public MigrateFilesService(ContentDbContext contentDbContext,
            IBlobStorageService blobStorageService,
            IUserService userService,
            ILogger<IBlobStorageService> logger)
        {
            _contentDbContext = contentDbContext;
            _blobStorageService = blobStorageService;
            _userService = userService;
            _logger = logger;
        }

        public async Task<Either<ActionResult, Unit>> MigrateFilenames(ReleaseFileTypes type)
        {
            return await _userService.CheckCanRunMigrations()
                .OnSuccessVoid(async () =>
                {
                    var files = await GetFiles(type);
                    await files.ForEachAsync(async file =>
                    {
                        try
                        {
                            _logger.LogInformation("Renaming file: {filename}", file.Filename);
                            if (await _blobStorageService.MoveBlob(
                                containerName: PrivateFilesContainerName,
                                sourcePath: AdminReleasePath(file.ReleaseId, file.ReleaseFileType, file.Filename),
                                destinationPath: file.Path()))
                            {
                                await MarkFileAsMigrated(file);
                            }
                        }
                        catch (Exception e)
                        {
                            _logger.LogError(e, "Caught exception renaming file");
                        }
                    });
                });
        }

        private async Task<List<ReleaseFileReference>> GetFiles(ReleaseFileTypes type)
        {
            return await _contentDbContext
                .ReleaseFileReferences
                .Where(file => file.ReleaseFileType == type && !file.FilenameMigrated)
                .ToListAsync();
        }

        private async Task MarkFileAsMigrated(ReleaseFileReference file)
        {
            _contentDbContext.Update(file);
            file.FilenameMigrated = true;
            await _contentDbContext.SaveChangesAsync();
        }
    }
}