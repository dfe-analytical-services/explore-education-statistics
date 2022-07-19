#nullable enable
using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Extensions;
using GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Services;

/**
 * Service used to migrate files in EES-3547
 * TODO Remove in EES-3552
 */
public class FileMigrationService : IFileMigrationService
{
    private readonly ContentDbContext _contentDbContext;
    private readonly IPersistenceHelper<ContentDbContext> _contentPersistenceHelper;
    private readonly IBlobStorageService _privateBlobStorageService;

    public FileMigrationService(
        ContentDbContext contentDbContext,
        IPersistenceHelper<ContentDbContext> contentPersistenceHelper,
        IBlobStorageService privateBlobStorageService)
    {
        _contentDbContext = contentDbContext;
        _contentPersistenceHelper = contentPersistenceHelper;
        _privateBlobStorageService = privateBlobStorageService;
    }

    public async Task<Either<ActionResult, Unit>> MigrateFile(Guid id)
    {
        return await _contentPersistenceHelper
            .CheckEntityExists<File>(id)
            .OnSuccess(async file =>
            {
                // Assume we can find all files within private blob containers
                // and therefore we can use file.Path and ignore file.PublicPath
                var privatePath = file.Path();

                var privateContainer = await GetPrivateContainer(file);

                var blob = await _privateBlobStorageService.FindBlob(privateContainer, privatePath);

                if (blob == null)
                {
                    return new NotFoundResult();
                }

                _contentDbContext.Files.Update(file);
                file.ContentType = blob.ContentType;
                file.ContentLength = blob.ContentLength;
                await _contentDbContext.SaveChangesAsync();

                return new Either<ActionResult, Unit>(Unit.Instance);
            });
    }

    private async Task<IBlobContainer> GetPrivateContainer(File file)
    {
        switch (file.Type)
        {
            case FileType.Ancillary:
            case FileType.Chart:
            case FileType.Data:
            case FileType.DataGuidance:
            case FileType.DataZip:
            case FileType.Metadata:
                return BlobContainers.PrivateReleaseFiles;
            case FileType.Image:
                return await GetPrivateImageFileContainer(file);
            case FileType.AllFilesZip:
                // Not expecting any 'AllFilesZip' database entries
            default:
                throw new ArgumentOutOfRangeException(nameof(file), $"Unexpected file type '{file.Type}'.");
        }
    }

    private async Task<IBlobContainer> GetPrivateImageFileContainer(File file)
    {
        if (file.Type != FileType.Image)
        {
            throw new ArgumentException("Expecting image file", nameof(file));
        }

        // Determine if it's a release or a methodology image based on what type of links to the file exist

        var existsReleaseFile = await _contentDbContext.ReleaseFiles.AnyAsync(rf => rf.FileId == file.Id);
        if (existsReleaseFile)
        {
            return BlobContainers.PrivateReleaseFiles;
        }

        var existsMethodologyFile = await _contentDbContext.MethodologyFiles.AnyAsync(mf => mf.FileId == file.Id);
        if (existsMethodologyFile)
        {
            return BlobContainers.PrivateMethodologyFiles;
        }

        throw new InvalidOperationException($"No release or methodology link found for image file '{file.Id}'.");
    }
}
