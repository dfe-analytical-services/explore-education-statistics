#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Validators;
using GovUk.Education.ExploreEducationStatistics.Common;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Options;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using File = GovUk.Education.ExploreEducationStatistics.Content.Model.File;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services
{
    public class DataSetValidator(
        ContentDbContext context,
        IOptions<FeatureFlagsOptions> featureFlags) : IDataSetValidator
    {
        public async Task<Either<List<ErrorViewModel>, DataSet>> ValidateDataSet(
            DataSetDto dataSet,
            bool performAutoReplacement = false) // TODO (EES-5708): This flag will be removed once upload methods are aligned. Currently auto-replacement is only available via bulk uploads
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

            var isReplacement = dataSet.ReplacingFile != null;

            errors.AddRange(ValidateDataSetTitleDuplication(dataSet.ReleaseVersionId, dataSet.Title, isReplacement));
            errors.AddRange(ValidateDataFileNames(dataSet.ReleaseVersionId, dataFile.FileName, dataSet.ReplacingFile));

            var fileToBeReplaced = (File?)null;

            if (isReplacement)
            {
                var releaseFileWithApiDataSet = await GetReplacingFileWithApiDataSetIfExists(dataSet.ReleaseVersionId, dataSet.Title);

                if (releaseFileWithApiDataSet != null && !featureFlags.Value.EnableReplacementOfPublicApiDataSets)
                {
                    errors.Add(ValidationMessages.GenerateErrorCannotReplaceDataSetWithApiDataSet(dataSet.Title));
                    return errors;
                }

                // TODO (EES-5708/6176): The `performAutoReplacement` condition can be removed once upload methods are aligned
                // Auto-replacement is currently only available for bulk zip uploads, and replacements triggered via the UI
                if (performAutoReplacement || featureFlags.Value.EnableReplacementOfPublicApiDataSets)
                {
                    await GetReplacingFileIfExists(dataSet.ReleaseVersionId, dataSet.Title)
                        .OnFailureDo(errors.Add)
                        .OnSuccessVoid(file => fileToBeReplaced = file);
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

            var headers = await CsvUtils.GetCsvHeaders(indexFile.FileStream, leaveOpen: true);

            if (headers is not ["file_name", "dataset_name"])
            {
                errors.Add(ValidationMessages.GenerateErrorDataSetNamesCsvIncorrectHeaders());
                return errors;
            }

            var fileNameIndex = headers[0] == "file_name" ? 0 : 1;
            var datasetNameIndex = headers[0] == "dataset_name" ? 0 : 1;

            indexFile.FileStream.SeekToBeginning();

            var rows = await CsvUtils.GetCsvRows(indexFile.FileStream);
            var dataSetIndex = new DataSetIndex { ReleaseVersionId = releaseVersionId };

            var indexFileEntries = new List<(string BaseFilename, string Title)>();

            foreach (var row in rows)
            {
                var dataSetName = row[datasetNameIndex].Trim();
                var dataFileName = row[fileNameIndex].Replace(".csv", ""); // File names should exclude extensions, but better to replace than return an error

                await GetReplacingFileIfExists(releaseVersionId, dataSetName)
                    .OnFailureDo(errors.Add)
                    .OnSuccessVoid(fileToBeReplaced =>
                    {
                        dataSetIndex.DataSetIndexItems.Add(new()
                        {
                            DataSetTitle = dataSetName,
                            DataFileName = $"{dataFileName}{Constants.DataSet.DataFileExtension}",
                            MetaFileName = $"{dataFileName}{Constants.DataSet.MetaFileExtension}",
                            ReplacingFile = fileToBeReplaced,
                        });

                        indexFileEntries.Add((BaseFilename: dataFileName, Title: dataSetName));
                    });
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

        private async Task<Either<ErrorViewModel, File?>> GetReplacingFileIfExists(
            Guid releaseVersionId,
            string dataSetName)
        {
            // We replace files with the same title. If there is no ReleaseFile with the same title, it's a new data set.
            var releaseFileToBeReplaced = await context.ReleaseFiles
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
            return await context.ReleaseFiles
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

        private List<ErrorViewModel> ValidateDataSetTitleDuplication(
            Guid releaseVersionId,
            string title,
            bool isReplacement)
        {
            if (!isReplacement) // if it's a replacement, we get the title from the replacement which is already validated as unique
            {
                var dataSetNameExists = context.ReleaseFiles
                    .Include(rf => rf.File)
                    .Any(rf =>
                        rf.ReleaseVersionId == releaseVersionId
                        && rf.File.Type == FileType.Data
                        && rf.Name == title);

                if (dataSetNameExists)
                {
                    return [ValidationMessages.GenerateErrorDataSetTitleShouldBeUnique(title)];
                }
            }

            return [];
        }

        private bool IsFileExisting(
            Guid releaseVersionId,
            FileType type,
            string filename)
        {
            return context
                .ReleaseFiles
                .Include(rf => rf.File)
                .Where(rf => rf.ReleaseVersionId == releaseVersionId && rf.File.Type == type)
                .AsEnumerable()
                .Any(rf => string.Equals(rf.File.Filename, filename, StringComparison.CurrentCultureIgnoreCase));
        }

        private List<ErrorViewModel> ValidateDataFileNames(
            Guid releaseVersionId,
            string dataFileName,
            File? replacingFile = null)
        {
            // Original uploads' data filename is not unique if a ReleaseFile exists with the same filename.
            // With replacement uploads, we can ignore a preexisting ReleaseFile if it is the file being replaced -
            // we only care if the preexisting duplicate ReleaseFile name isn't the file being replaced.
            if (IsFileExisting(releaseVersionId, FileType.Data, dataFileName) &&
                (replacingFile == null || !replacingFile.Filename.Equals(dataFileName, StringComparison.CurrentCultureIgnoreCase)))
            {
                return [ValidationMessages.GenerateErrorFileNameNotUnique(dataFileName, FileType.Data)];
            }

            // NOTE: We allow duplicate meta file names - meta files aren't included in publicly downloadable
            // zips, so meta files won't be included in the same directory by filename and thereby cannot clash
            return [];
        }
    }
}
