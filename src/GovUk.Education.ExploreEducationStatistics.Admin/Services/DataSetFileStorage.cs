#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Public.Data;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Options;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static GovUk.Education.ExploreEducationStatistics.Common.BlobContainers;
using IReleaseVersionRepository = GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.IReleaseVersionRepository;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services;

public class DataSetFileStorage(
    ContentDbContext contentDbContext,
    IPrivateBlobStorageService privateBlobStorageService,
    IReleaseVersionRepository releaseVersionRepository,
    IReleaseDataFileRepository releaseDataFileRepository,
    IDataImportService dataImportService,
    IUserService userService,
    IDataSetVersionService dataSetVersionService,
    IOptions<FeatureFlags> featureFlags,
    IDataSetVersionRepository dataSetVersionRepository) : IDataSetFileStorage
{
    public async Task<DataFileInfo> UploadDataSet(
        Guid releaseVersionId,
        DataSet dataSet,
        CancellationToken cancellationToken)
    {
        var subjectId = await releaseVersionRepository.CreateStatisticsDbReleaseAndSubjectHierarchy(releaseVersionId);

        ReleaseFile? replacedReleaseDataFile = null;

        if (dataSet.ReplacingFile is not null)
        {
            replacedReleaseDataFile = await GetReplacedReleaseFile(releaseVersionId, dataSet.ReplacingFile.Id);

            if (replacedReleaseDataFile is not null)
            {
                replacedReleaseDataFile.File.ReplacedById = dataSet.ReplacingFile.Id;
                await contentDbContext.SaveChangesAsync(cancellationToken);
            }
        }

        var releaseDataFileOrder = await GetNextDataFileOrder(releaseVersionId, replacedReleaseDataFile?.File.Id);

        var dataFile = await releaseDataFileRepository.Create(
            releaseVersionId,
            subjectId,
            dataSet.DataFile.FileName,
            contentLength: dataSet.DataFile.FileStream.Length,
            type: FileType.Data,
            createdById: userService.GetUserId(),
            name: dataSet.Title,
            dataSet.ReplacingFile,
            order: releaseDataFileOrder);

        var dataReleaseFile = await contentDbContext.ReleaseFiles
            .Include(rf => rf.File)
            .ThenInclude(f => f.CreatedBy)
            .SingleAsync(rf =>
                rf.ReleaseVersionId == releaseVersionId &&
                rf.FileId == dataFile.Id,
                cancellationToken);

        var metaFile = await releaseDataFileRepository.Create(
            releaseVersionId,
            subjectId,
            dataSet.MetaFile.FileName,
            contentLength: dataSet.MetaFile.FileStream.Length,
            type: FileType.Metadata,
            createdById: userService.GetUserId());

        await UploadDataSetToReleaseStorage(releaseVersionId, dataFile.Id, metaFile.Id, dataSet, cancellationToken);

        if (featureFlags.Value.EnableReplacementOfPublicApiDataSets &&
            dataSet.ReplacingFile is not null &&
            await contentDbContext.ReleaseFiles.AnyAsync(rf =>
                rf.ReleaseVersionId == releaseVersionId &&
                rf.Name == dataSet.Title &&
                rf.PublicApiDataSetId != null,
                cancellationToken))
        {
            var dataSetVersion = await dataSetVersionRepository.GetDataSetVersion(
                    replacedReleaseDataFile!.PublicApiDataSetId!.Value,
                    replacedReleaseDataFile.PublicApiDataSetVersion!,
                    cancellationToken) ?? throw new ArgumentNullException("The data set version needed to patch as part of this replacement was not found.");
            
            await dataSetVersionService.CreateNextVersion(
                dataReleaseFile.Id,
                replacedReleaseDataFile.PublicApiDataSetId.Value,
                dataSetVersion.Id,
                cancellationToken
            );
        }

        var dataImport = await dataImportService.Import(subjectId, dataFile, metaFile);

        var permissions = await userService.GetDataFilePermissions(dataFile);

        return new DataFileInfo
        {
            Id = dataReleaseFile.FileId,
            FileName = dataReleaseFile.File.Filename,
            Name = dataSet.Title,
            Size = dataReleaseFile.File.DisplaySize(),
            MetaFileId = dataImport.MetaFile.Id,
            MetaFileName = dataImport.MetaFile.Filename,
            ReplacedBy = dataReleaseFile.File.ReplacedById,
            Rows = dataImport.TotalRows,
            UserName = dataReleaseFile.File.CreatedBy?.Email ?? "",
            Status = dataImport.Status,
            Created = dataReleaseFile.File.Created,
            Permissions = permissions,
            PublicApiDataSetId = dataReleaseFile.PublicApiDataSetId,
            PublicApiDataSetVersion = dataReleaseFile.PublicApiDataSetVersionString,
        };
    }

    private async Task UploadDataSetToReleaseStorage(
        Guid releaseVersionId,
        Guid dataFileId,
        Guid metaFileId,
        DataSet dataSet,
        CancellationToken cancellationToken)
    {
        var dataFilePath = $"{FileStoragePathUtils.FilesPath(releaseVersionId, FileType.Data)}{dataFileId}";
        var metaFilePath = $"{FileStoragePathUtils.FilesPath(releaseVersionId, FileType.Metadata)}{metaFileId}";

        await privateBlobStorageService.UploadStream(
            containerName: PrivateReleaseFiles,
            dataFilePath,
            dataSet.DataFile.FileStream,
            contentType: ContentTypes.Csv,
            cancellationToken: cancellationToken);

        await privateBlobStorageService.UploadStream(
            containerName: PrivateReleaseFiles,
            metaFilePath,
            dataSet.MetaFile.FileStream,
            contentType: ContentTypes.Csv,
            cancellationToken: cancellationToken);

        await dataSet.DataFile.FileStream.DisposeAsync();
        await dataSet.MetaFile.FileStream.DisposeAsync();
    }

    public async Task<List<DataSetUploadResultViewModel>> UploadDataSetsToTemporaryStorage(
        Guid releaseVersionId,
        List<DataSet> dataSets,
        CancellationToken cancellationToken)
    {
        var viewModels = new List<DataSetUploadResultViewModel>();

        foreach (var dataSet in dataSets)
        {
            var uploadResult = await UploadDataSetToTemporaryStorage(releaseVersionId, dataSet, cancellationToken);

            viewModels.Add(new DataSetUploadResultViewModel
            {
                Title = dataSet.Title,
                DataFileId = uploadResult.DataFileId,
                DataFileName = dataSet.DataFile.FileName,
                DataFileSize = dataSet.DataFile.FileSize,
                MetaFileId = uploadResult.MetaFileId,
                MetaFileName = dataSet.MetaFile.FileName,
                MetaFileSize = dataSet.MetaFile.FileSize,
                ReplacingFileId = dataSet.ReplacingFile?.Id,
            });
        }

        return viewModels;
    }

    /// <summary>
    /// Upload the supplied data set files to temporary blob storage.
    /// </summary>
    /// <returns>An object consisting of newly generated IDs representing the uploaded files. The IDs are used to locate the files in virtual storage.</returns>
    private async Task<DataSetUploadResult> UploadDataSetToTemporaryStorage(
        Guid releaseVersionId,
        DataSet dataSet,
        CancellationToken cancellationToken)
    {
        var dataFileId = Guid.NewGuid();
        var metaFileId = Guid.NewGuid();
        var dataFilePath = $"{FileStoragePathUtils.FilesPath(releaseVersionId, FileType.Data)}{dataFileId}";
        var metaFilePath = $"{FileStoragePathUtils.FilesPath(releaseVersionId, FileType.Metadata)}{metaFileId}";

        await privateBlobStorageService.UploadStream(
            containerName: PrivateReleaseTempFiles,
            path: dataFilePath,
            stream: dataSet.DataFile.FileStream,
            contentType: ContentTypes.Csv,
            cancellationToken: cancellationToken);

        await privateBlobStorageService.UploadStream(
            containerName: PrivateReleaseTempFiles,
            path: metaFilePath,
            stream: dataSet.MetaFile.FileStream,
            contentType: ContentTypes.Csv,
            cancellationToken: cancellationToken);

        await dataSet.DataFile.FileStream.DisposeAsync();
        await dataSet.MetaFile.FileStream.DisposeAsync();

        return new DataSetUploadResult
        {
            DataFileId = dataFileId,
            MetaFileId = metaFileId
        };
    }

    public async Task<List<ReleaseFile>> MoveDataSetsToPermanentStorage(
        Guid releaseVersionId,
        List<DataSetUploadResultViewModel> dataSets,
        CancellationToken cancellationToken)
    {
        var releaseFiles = new List<ReleaseFile>();

        foreach (var dataSetFile in dataSets)
        {
            var subjectId = await releaseVersionRepository.CreateStatisticsDbReleaseAndSubjectHierarchy(releaseVersionId);

            var replacingFile = dataSetFile.ReplacingFileId is null
                ? null
                : await contentDbContext.Files.FirstAsync(f => f.Id == dataSetFile.ReplacingFileId, cancellationToken);

            ReleaseFile? replacedReleaseDataFile = null;

            if (replacingFile is not null)
            {
                replacedReleaseDataFile = await GetReplacedReleaseFile(releaseVersionId, replacingFile.Id);
            }

            var releaseDataFileOrder = await GetNextDataFileOrder(releaseVersionId, replacedReleaseDataFile?.File.Id);

            var dataFile = await releaseDataFileRepository.Create(
                releaseVersionId,
                subjectId,
                dataSetFile.DataFileName,
                contentLength: dataSetFile.DataFileSize,
                type: FileType.Data,
                createdById: userService.GetUserId(),
                name: dataSetFile.Title,
                replacingDataFile: replacingFile,
                order: releaseDataFileOrder);

            var sourceDataFilePath = FileExtensions.Path(releaseVersionId, FileType.Data, dataSetFile.DataFileId);
            var destinationDataFilePath = FileExtensions.Path(releaseVersionId, FileType.Data, dataFile.Id); // Same path, but a new ID has been generated by the creation step above

            var dataReleaseFile = await contentDbContext.ReleaseFiles
                .Include(rf => rf.File)
                .SingleAsync(rf =>
                    rf.ReleaseVersionId == releaseVersionId &&
                    rf.FileId == dataFile.Id,
                    cancellationToken);

            releaseFiles.Add(dataReleaseFile);

            var metaFile = await releaseDataFileRepository.Create(
                releaseVersionId,
                subjectId,
                dataSetFile.MetaFileName,
                dataSetFile.MetaFileSize,
                type: FileType.Metadata,
                createdById: userService.GetUserId());

            var sourceMetaFilePath = FileExtensions.Path(releaseVersionId, FileType.Metadata, dataSetFile.MetaFileId);
            var destinationMetaFilePath = FileExtensions.Path(releaseVersionId, FileType.Metadata, metaFile.Id); // Same path, but a new ID has been generated by the creation step above

            await dataImportService.Import(subjectId, dataFile, metaFile);

            await privateBlobStorageService.MoveBlob(PrivateReleaseTempFiles, sourceDataFilePath, destinationDataFilePath, PrivateReleaseFiles);
            await privateBlobStorageService.MoveBlob(PrivateReleaseTempFiles, sourceMetaFilePath, destinationMetaFilePath, PrivateReleaseFiles);
        }

        return releaseFiles;
    }

    private async Task<int> GetNextDataFileOrder(
        Guid releaseVersionId,
        Guid? fileToBeReplaceId = null)
    {
        if (fileToBeReplaceId is not null)
        {
            var fileToBeReplacedReleaseFile = await contentDbContext.ReleaseFiles
                .Include(rf => rf.File)
                .SingleAsync(rf =>
                    rf.ReleaseVersionId == releaseVersionId
                    && rf.File.Type == FileType.Data
                    && rf.File.Id == fileToBeReplaceId);

            return fileToBeReplacedReleaseFile.Order;
        }

        var currentMaxOrder = await contentDbContext.ReleaseFiles
            .Include(releaseFile => releaseFile.File)
            .Where(releaseFile =>
                releaseFile.ReleaseVersionId == releaseVersionId &&
                releaseFile.File.Type == FileType.Data &&
                releaseFile.File.ReplacingId == null)
            .MaxAsync(releaseFile => (int?)releaseFile.Order);

        return currentMaxOrder.HasValue ? currentMaxOrder.Value + 1 : 0;
    }

    private async Task<ReleaseFile?> GetReplacedReleaseFile(Guid releaseVersionId, Guid fileToBeReplacedId)
    {
        return await contentDbContext.ReleaseFiles
            .Include(rf => rf.File)
            .SingleAsync(rf =>
                rf.ReleaseVersionId == releaseVersionId
                && rf.File.Type == FileType.Data
                && rf.File.Id == fileToBeReplacedId);
    }
}
