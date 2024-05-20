using GovUk.Education.ExploreEducationStatistics.Common;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Extensions;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Services.Interfaces;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Functions;

public class CopyCsvFilesFunction(
    ILogger<CopyCsvFilesFunction> logger,
    ContentDbContext contentDbContext,
    PublicDataDbContext publicDataDbContext,
    IDataSetVersionPathResolver dataSetVersionPathResolver,
    IPrivateBlobStorageService privateBlobStorageService)
{
    [Function(nameof(CopyCsvFiles))]
    public async Task CopyCsvFiles(
        [ActivityTrigger] Guid dataSetVersionId,
        Guid instanceId,
        CancellationToken cancellationToken)
    {
        var dataSetVersion = await GetDataSetVersion(
            dataSetVersionId: dataSetVersionId,
            instanceId: instanceId,
            cancellationToken);

        await UpdateStage(dataSetVersion, instanceId, DataSetVersionImportStage.CopyingCsvFiles, cancellationToken);

        var csvDataFile = await contentDbContext.ReleaseFiles
            .Where(rf => rf.Id == dataSetVersion.ReleaseFileId)
            .Select(rf => rf.File)
            .SingleAsync(cancellationToken);

        var csvMetaFile = await contentDbContext.Files
            .SingleAsync(f => f.SubjectId == csvDataFile.SubjectId && f.Type == FileType.Metadata,
                cancellationToken: cancellationToken);

        await CopyCsvFile(csvDataFile, dataSetVersionPathResolver.CsvDataPath(dataSetVersion), cancellationToken);
        await CopyCsvFile(csvMetaFile, dataSetVersionPathResolver.CsvMetadataPath(dataSetVersion), cancellationToken);
    }

    private async Task CopyCsvFile(
        GovUk.Education.ExploreEducationStatistics.Content.Model.File csvFile,
        string destinationPath,
        CancellationToken cancellationToken)
    {
        var destinationDirectoryPath = Path.GetDirectoryName(destinationPath);
        if (destinationDirectoryPath != null && !Directory.Exists(destinationDirectoryPath))
        {
            Directory.CreateDirectory(destinationDirectoryPath);
        }

        if (File.Exists(destinationPath))
        {
            logger.LogWarning("Destination csv file '{destination}' already exists and will be overwritten.",
                destinationPath);
        }

        var blobStream = await privateBlobStorageService.StreamBlob(
            BlobContainers.PrivateReleaseFiles,
            csvFile.Path(),
            cancellationToken: cancellationToken);

        await using var fileStream = new FileStream(destinationPath, FileMode.Create, FileAccess.Write);
        await CompressionUtils.CompressToStream(blobStream, fileStream, ContentEncodings.Gzip, cancellationToken);
    }

    private async Task<DataSetVersion> GetDataSetVersion(
        Guid dataSetVersionId,
        Guid instanceId,
        CancellationToken cancellationToken)
    {
        return await publicDataDbContext.DataSetVersions
            .Include(dsv => dsv.Imports.Where(i => i.InstanceId == instanceId))
            .SingleAsync(dsv => dsv.Id == dataSetVersionId, cancellationToken: cancellationToken);
    }

    private async Task UpdateStage(
        DataSetVersion dataSetVersion,
        Guid instanceId,
        DataSetVersionImportStage stage,
        CancellationToken cancellationToken)
    {
        var dataSetVersionImport = dataSetVersion.Imports.Single(i => i.InstanceId == instanceId);

        dataSetVersionImport.Stage = stage;
        await publicDataDbContext.SaveChangesAsync(cancellationToken);
    }
}
