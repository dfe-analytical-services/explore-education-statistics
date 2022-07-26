#nullable enable
using System;
using System.Linq;
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
using Microsoft.Extensions.Logging;

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
    private readonly ILogger<FileMigrationService> _logger;

    public FileMigrationService(ContentDbContext contentDbContext,
        IPersistenceHelper<ContentDbContext> contentPersistenceHelper,
        IBlobStorageService privateBlobStorageService,
        ILogger<FileMigrationService> logger)
    {
        _contentDbContext = contentDbContext;
        _contentPersistenceHelper = contentPersistenceHelper;
        _privateBlobStorageService = privateBlobStorageService;
        _logger = logger;
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

                if (file.Type == FileType.Data)
                {
                    // Also check the corresponding DataImport has a positive value for TotalRows set
                    return await UpdateDataImportIfNecessary(file, blob)
                        .OnSuccessVoid(async () =>
                        {
                            await _contentDbContext.SaveChangesAsync();
                        });
                }

                await _contentDbContext.SaveChangesAsync();
                return Unit.Instance;
            });
    }

    private async Task<Either<ActionResult, Unit>> UpdateDataImportIfNecessary(File file, BlobInfo blob)
    {
        if (file.Type != FileType.Data)
        {
            throw new ArgumentException("Expecting data file", nameof(file));
        }

        return await _contentPersistenceHelper
            .CheckEntityExists<DataImport>(q =>
                q.Where(dataImport => dataImport.FileId == file.Id))
            .OnSuccessVoid(async dataImport =>
            {
                // Update DataImport if TotalRows is not positive 
                if (dataImport.TotalRows < 1)
                {
                    if (blob.Meta.TryGetValue("NumberOfRows", out var numberOfRowsStringVal))
                    {
                        if (int.TryParse(numberOfRowsStringVal, out var numberOfRowsIntVal))
                        {
                            _contentDbContext.DataImports.Update(dataImport);
                            dataImport.TotalRows = numberOfRowsIntVal;
                        }
                        else
                        {
                            _logger.LogWarning(
                                "Could not convert NumberOfRows metadata property to int for blob: {path}", blob.Path);
                        }
                    }
                    else
                    {
                        _logger.LogWarning("NumberOfRows metadata property not found for blob: {path}", blob.Path);
                    }
                }
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
