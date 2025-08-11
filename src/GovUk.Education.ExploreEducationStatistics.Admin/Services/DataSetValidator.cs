#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Validators;
using GovUk.Education.ExploreEducationStatistics.Common;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Options;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using File = GovUk.Education.ExploreEducationStatistics.Content.Model.File;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services;

public class DataSetValidator(
    ContentDbContext contentDbContext,
    IOptions<FeatureFlagsOptions> featureFlags) : IDataSetValidator
{
    public async Task<Either<List<ErrorViewModel>, DataSet>> ValidateDataSet(DataSetDto dataSet)
    {
        var errors = new List<ErrorViewModel>();

        var dataFile = dataSet.DataFile;
        var metaFile = dataSet.MetaFile;

        if (dataFile is null || metaFile is null)
        {
            errors.Add(ValidationMessages.GenerateErrorDataSetFileNamesShouldMatchConvention());
            return errors;
        }

        var validator = new DataSetDto.Validator();

        var result = validator.Validate(dataSet);

        if (!result.IsValid)
        {
            errors.AddRange(result.Errors.Select(e => new ErrorViewModel
            {
                Code = e.ErrorCode,
                Message = e.ErrorMessage,
            }));
        }

        var fileToBeReplaced = (File?)null;

        await GetReplacingFileIfExists(dataSet.ReleaseVersionId, dataSet.Title)
            .OnFailureDo(errors.Add)
            .OnSuccessDo(file => fileToBeReplaced = file)
            .OnSuccessVoid();

        if (fileToBeReplaced is not null)
        {
            var releaseFileWithApiDataSet = await GetReplacingFileWithApiDataSetIfExists(dataSet.ReleaseVersionId, dataSet.Title);

            if (releaseFileWithApiDataSet != null && !featureFlags.Value.EnableReplacementOfPublicApiDataSets)
            {
                if (!releaseFileWithApiDataSet.ReleaseVersion.Amendment)
                {
                    errors.Add(ValidationMessages.GenerateErrorCannotReplaceDraftApiDataSet(dataSet.Title));
                }
                else
                {
                    errors.Add(ValidationMessages.GenerateErrorCannotReplaceDataSetWithApiDataSet(dataSet.Title));
                }

                return errors;
            }
        }

        return errors.Count > 0
            ? errors
            : new DataSet
            {
                Title = dataSet.Title,
                DataFile = dataFile,
                MetaFile = metaFile,
                ReplacingFile = fileToBeReplaced,
            };
    }

    public async Task<Either<List<ErrorViewModel>, DataSetIndex>> ValidateBulkDataZipIndexFile(
        Guid releaseVersionId,
        FileDto indexFile,
        List<FileDto> dataSetFiles)
    {
        var errors = new List<ErrorViewModel>();
        var validator = new FileDto.Validator();

        var result = validator.Validate(indexFile);

        if (!result.IsValid)
        {
            errors.AddRange(result.Errors.Select(e => new ErrorViewModel
            {
                Code = e.ErrorCode,
                Message = e.ErrorMessage,
            }));
        }

        var headers = await CsvUtils.GetCsvHeaders(indexFile.FileStreamProvider(), leaveOpen: true);

        if (headers is not ["file_name", "dataset_name"])
        {
            errors.Add(ValidationMessages.GenerateErrorDataSetNamesCsvIncorrectHeaders());
            return errors;
        }

        var fileNameIndex = headers[0] == "file_name" ? 0 : 1;
        var datasetNameIndex = headers[0] == "dataset_name" ? 0 : 1;

        var rows = await CsvUtils.GetCsvRows(indexFile.FileStreamProvider());
        var dataSetIndex = new DataSetIndex { ReleaseVersionId = releaseVersionId };

        var indexFileEntries = new List<(string BaseFilename, string Title)>();

        foreach (var row in rows)
        {
            var dataSetName = row[datasetNameIndex].Trim();
            var dataFileName = row[fileNameIndex].Replace(".csv", ""); // File names should exclude extensions, but better to replace than return an error

            await GetReplacingFileIfExists(releaseVersionId, dataSetName)
                .OnFailureDo(errors.Add)
                .OnSuccessDo(_ =>
                {
                    dataSetIndex.DataSetIndexItems.Add(new()
                    {
                        DataSetTitle = dataSetName,
                        DataFileName = $"{dataFileName}{Constants.DataSet.DataFileExtension}",
                        MetaFileName = $"{dataFileName}{Constants.DataSet.MetaFileExtension}",
                    });

                    indexFileEntries.Add((BaseFilename: dataFileName, Title: dataSetName));
                })
            .OnSuccessVoid();
        }

        if (errors.Count != 0)
        {
            return errors;
        }

        errors.AddRange(CheckIndexFileForDuplicationErrors(indexFileEntries));
        errors.AddRange(CheckBulkDataZipForMissingFiles(dataSetIndex.DataSetIndexItems, dataSetFiles));
        errors.AddRange(CheckBulkDataZipForUnusedFiles(dataSetIndex.DataSetIndexItems, dataSetFiles));

        return errors.Count > 0
            ? errors
            : dataSetIndex;
    }

    private static List<ErrorViewModel> CheckBulkDataZipForMissingFiles(
        List<DataSetIndexItem> indexItems,
        List<FileDto> dataSetFiles)
    {
        var errors = new List<ErrorViewModel>();

        var indexItemDataFileNames = indexItems.Select(item => item.DataFileName);
        var indexItemMetaFileNames = indexItems.Select(item => item.MetaFileName);

        var dataSetFileNames = dataSetFiles.Select(item => item.FileName);

        errors.AddRange(indexItemDataFileNames
            .Where(fileName => !dataSetFileNames.Contains(fileName))
            .Select(fileName => ValidationMessages.GenerateErrorFileNotFoundInZip(fileName, FileType.Data)));

        errors.AddRange(indexItemMetaFileNames
            .Where(fileName => !dataSetFileNames.Contains(fileName))
            .Select(fileName => ValidationMessages.GenerateErrorFileNotFoundInZip(fileName, FileType.Metadata)));

        return errors;
    }

    /// <summary>
    /// Retrieve a replacement <see cref="File" /> for the specified release version and data set name, if it exists.
    /// </summary>
    /// <returns>The replacement <see cref="File" /> if present, or an error model if multiple results were found. The latter indicates a replacement is currently in progress and should halt further processing.</returns>
    private async Task<Either<ErrorViewModel, File?>> GetReplacingFileIfExists(
        Guid releaseVersionId,
        string dataSetName)
    {
        // We replace files with the same title. If there is no ReleaseFile with the same title, it's a new data set.
        var releaseFileToBeReplaced = await contentDbContext.ReleaseFiles
            .Include(rf => rf.File)
            .Where(rf =>
                rf.ReleaseVersionId == releaseVersionId &&
                rf.File.Type == FileType.Data &&
                rf.Name == dataSetName)
            .ToListAsync();

        return releaseFileToBeReplaced.Count > 1
            ? ValidationMessages.GenerateErrorDataReplacementAlreadyInProgress()
            : releaseFileToBeReplaced.SingleOrDefault()?.File;
    }

    private async Task<ReleaseFile?> GetReplacingFileWithApiDataSetIfExists(
        Guid releaseVersionId,
        string dataSetName)
    {
        return await contentDbContext.ReleaseFiles
            .SingleOrDefaultAsync(rf =>
                rf.ReleaseVersionId == releaseVersionId &&
                rf.Name == dataSetName &&
                rf.PublicApiDataSetId != null);
    }

    private static List<ErrorViewModel> CheckIndexFileForDuplicationErrors(List<(string BaseFilename, string Title)> dataSetNamesCsvEntries)
    {
        var errors = new List<ErrorViewModel>();

        dataSetNamesCsvEntries
            .GroupBy(entry => entry.Title)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key)
            .ToList()
            .ForEach(duplicateTitle => errors.Add(ValidationMessages.GenerateErrorDataSetTitleShouldBeUnique(duplicateTitle)));

        dataSetNamesCsvEntries
            .GroupBy(entry => entry.BaseFilename)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key)
            .ToList()
            .ForEach(duplicateFilename => errors.Add(ValidationMessages.GenerateErrorDataSetNamesCsvFilenamesShouldBeUnique(duplicateFilename)));

        return errors;
    }

    private static List<ErrorViewModel> CheckBulkDataZipForUnusedFiles(
        List<DataSetIndexItem> indexItems,
        List<FileDto> dataSetFiles)
    {
        var indexItemDataFileNames = indexItems.Select(item => item.DataFileName);
        var indexItemMetaFileNames = indexItems.Select(item => item.MetaFileName);

        var remainingFiles = dataSetFiles
            .Where(file =>
                !indexItemDataFileNames.Contains(file.FileName) &&
                !indexItemMetaFileNames.Contains(file.FileName))
            .ToList();

        if (remainingFiles.Count > 0)
        {
            var remainingFileNames = remainingFiles
                .Select(file => file.FileName)
                .ToList();

            return [ValidationMessages.GenerateErrorZipContainsUnusedFiles(remainingFileNames)];
        }

        return [];
    }
}
