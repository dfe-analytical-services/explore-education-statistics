using GovUk.Education.ExploreEducationStatistics.Common;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Extensions;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Services.Interfaces;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using static GovUk.Education.ExploreEducationStatistics.Public.Data.Model.DataSetVersionImportStage;
using File = GovUk.Education.ExploreEducationStatistics.Content.Model.File;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Functions;

public class CopyCsvFilesFunction(
    ILogger<CopyCsvFilesFunction> logger,
    ContentDbContext contentDbContext,
    PublicDataDbContext publicDataDbContext,
    IDataSetVersionPathResolver dataSetVersionPathResolver,
    IPrivateBlobStorageService privateBlobStorageService) : BaseProcessDataSetVersionFunction(publicDataDbContext)
{
    [Function(ActivityNames.CopyCsvFiles)]
    public async Task CopyCsvFiles(
        [ActivityTrigger] Guid instanceId,
        CancellationToken cancellationToken)
    {
        var dataSetVersionImport = await GetDataSetVersionImport(instanceId, cancellationToken);
        var dataSetVersion = dataSetVersionImport.DataSetVersion;

        await UpdateImportStage(dataSetVersionImport, CopyingCsvFiles, cancellationToken);

        var csvDataFile = await contentDbContext.ReleaseFiles
            .AsNoTracking()
            .Where(rf => rf.Id == dataSetVersion.Release.ReleaseFileId)
            .Select(rf => rf.File)
            .SingleAsync(cancellationToken);

        var csvMetaFile = await contentDbContext.Files
            .AsNoTracking()
            .SingleAsync(f => f.SubjectId == csvDataFile.SubjectId && f.Type == FileType.Metadata,
                cancellationToken: cancellationToken);

        await CopyCsvFile(csvDataFile,
            dataSetVersionPathResolver.CsvDataPath(dataSetVersion),
            compress: true,
            cancellationToken);

        await CopyCsvFile(csvMetaFile,
            dataSetVersionPathResolver.CsvMetadataPath(dataSetVersion),
            compress: false,
            cancellationToken);
    }

    private async Task CopyCsvFile(
        File csvFile,
        string destinationPath,
        bool compress,
        CancellationToken cancellationToken)
    {
        var destinationDirectoryPath = Path.GetDirectoryName(destinationPath);
        if (destinationDirectoryPath != null && !Directory.Exists(destinationDirectoryPath))
        {
            Directory.CreateDirectory(destinationDirectoryPath);
        }

        if (System.IO.File.Exists(destinationPath))
        {
            logger.LogWarning("Destination csv file '{DestinationPath}' already exists and will be overwritten.",
                destinationPath);
        }

        var blobStream = await privateBlobStorageService.StreamBlob(
            BlobContainers.PrivateReleaseFiles,
            csvFile.Path(),
            cancellationToken: cancellationToken);

        await using var fileStream = new FileStream(destinationPath, FileMode.Create, FileAccess.Write, FileShare.None);

        if (compress)
        {
            await CompressionUtils.CompressToStream(blobStream, fileStream, ContentEncodings.Gzip, cancellationToken);
        }
        else
        {
            await blobStream.CopyToAsync(fileStream, cancellationToken);
        }
    }
}
