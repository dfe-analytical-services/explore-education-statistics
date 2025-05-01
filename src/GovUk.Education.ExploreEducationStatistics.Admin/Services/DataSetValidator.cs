#nullable enable
using FluentValidation;
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Validators;
using GovUk.Education.ExploreEducationStatistics.Common;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using File = GovUk.Education.ExploreEducationStatistics.Content.Model.File;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services
{
    public class DataSetValidator(ContentDbContext context) : IDataSetValidator
    {
        public const int MaxFilenameSize = 150;

        private readonly List<ErrorViewModel> _errors = [];

        public async Task<Either<List<ErrorViewModel>, DataSet>> ValidateDataSet(
            DataSetDto dataSet,
            bool performAutoReplacement = false) // TODO (EES-5708): This flag will be removed once upload methods are aligned. Currently auto-replacement is only available via bulk uploads
        {
            var dataFile = dataSet.DataFile;
            var metaFile = dataSet.MetaFile;

            if (dataFile is null || metaFile is null)
            {
                _errors.Add(ValidationMessages.GenerateErrorDataSetFileNamesShouldMatchConvention());
                return _errors;
            }

            var validator = new DataSetDto.Validator();

            var result = validator.Validate(dataSet);

            if (!result.IsValid)
            {
                _errors.AddRange(result.Errors.Select(e => new ErrorViewModel
                {
                    Code = e.ErrorCode,
                    Message = e.ErrorMessage,
                }));
            }

            var isReplacement = dataSet.ReplacingFile != null;

            ValidateDataSetTitleDuplication(dataSet.ReleaseVersionId, dataSet.Title, isReplacement);

            ValidateDataFileNames(dataSet.ReleaseVersionId, dataFile.FileName, dataSet.ReplacingFile);

            var fileToBeReplaced = (File?)null;

            if (isReplacement)
            {
                var releaseFileWithApiDataSet = await GetReplacingFileWithApiDataSetIfExists(dataSet.ReleaseVersionId, dataSet.Title);

                if (releaseFileWithApiDataSet != null)
                {
                    _errors.Add(ValidationMessages.GenerateErrorCannotReplaceDataSetWithApiDataSet(dataSet.Title));
                }

                // TODO (EES-5708): This condition can be removed once upload methods are aligned
                // Auto-replacement is currently only available for bulk zip uploads (EES-5708)
                if (performAutoReplacement)
                {
                    fileToBeReplaced = await GetReplacingFileIfExists(dataSet.ReleaseVersionId, dataSet.Title);
                }
            }

            return _errors.Count > 0
                ? (Either<List<ErrorViewModel>, DataSet>)_errors
                : (Either<List<ErrorViewModel>, DataSet>)new DataSet
                {
                    Title = dataSet.Title,
                    DataFile = dataFile,
                    MetaFile = metaFile,
                    ReplacingFile = fileToBeReplaced,
                };
        }

        public async Task<Either<List<ErrorViewModel>, DataSetIndex>> ValidateBulkDataZipIndexFile(
            Guid releaseVersionId,
            DataSetFileDto indexFile,
            List<DataSetFileDto> dataSetFiles)
        {
            var validator = new DataSetFileDto.Validator();

            var result = validator.Validate(indexFile);

            if (!result.IsValid)
            {
                _errors.AddRange(result.Errors.Select(e => new ErrorViewModel
                {
                    Code = e.ErrorCode,
                    Message = e.ErrorMessage,
                }));
            }

            var headers = await CsvUtils.GetCsvHeaders(indexFile.FileStream, leaveOpen: true);

            if (headers is not ["file_name", "dataset_name"])
            {
                _errors.Add(ValidationMessages.GenerateErrorDataSetNamesCsvIncorrectHeaders());
                return _errors;
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
                var dataFileName = row[fileNameIndex].Replace(".csv", "");

                var releaseFileToBeReplaced = await GetReplacingFileIfExists(releaseVersionId, dataSetName);

                dataSetIndex.DataSetIndexItems.Add(new()
                {
                    DataSetTitle = dataSetName,
                    DataFileName = $"{dataFileName}{Constants.DataSet.DataFileExtension}",
                    MetaFileName = $"{dataFileName}{Constants.DataSet.MetaFileExtension}",
                    ReplacingFile = releaseFileToBeReplaced,
                });

                indexFileEntries.Add((BaseFilename: dataFileName, Title: dataSetName));
            }

            CheckIndexFileForDuplicationErrors(indexFileEntries);
            CheckBulkDataZipForMissingFiles(dataSetIndex.DataSetIndexItems, dataSetFiles);
            CheckBulkDataZipForUnusedFiles(dataSetIndex.DataSetIndexItems, dataSetFiles);

            return _errors.Count > 0
                ? (Either<List<ErrorViewModel>, DataSetIndex>)_errors
                : (Either<List<ErrorViewModel>, DataSetIndex>)dataSetIndex;
        }

        private void CheckBulkDataZipForMissingFiles(
            List<DataSetIndexItem> indexItems,
            List<DataSetFileDto> dataSetFiles)
        {
            var indexItemDataFileNames = indexItems.Select(item => item.DataFileName);
            var indexItemMetaFileNames = indexItems.Select(item => item.MetaFileName);

            var dataSetFileNames = dataSetFiles.Select(item => item.FileName);

            _errors.AddRange(indexItemDataFileNames
                .Where(fileName => !dataSetFileNames.Contains(fileName))
                .Select(fileName => ValidationMessages.GenerateErrorFileNotFoundInZip(fileName, FileType.Data)));

            _errors.AddRange(indexItemMetaFileNames
                .Where(fileName => !dataSetFileNames.Contains(fileName))
                .Select(fileName => ValidationMessages.GenerateErrorFileNotFoundInZip(fileName, FileType.Metadata)));
        }

        private async Task<File?> GetReplacingFileIfExists(
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

            if (releaseFileToBeReplaced.Count > 1)
            {
                _errors.Add(ValidationMessages.GenerateErrorDataReplacementAlreadyInProgress());
                return null;
            }
            else
            {
                return releaseFileToBeReplaced.SingleOrDefault()?.File;
            }
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

        private void CheckIndexFileForDuplicationErrors(List<(string BaseFilename, string Title)> dataSetNamesCsvEntries)
        {
            dataSetNamesCsvEntries
                .GroupBy(entry => entry.Title)
                .Where(group => group.Count() > 1)
                .Select(group => group.Key)
                .ToList()
                .ForEach(duplicateTitle =>
                {
                    _errors.Add(ValidationMessages.GenerateErrorDataSetTitleShouldBeUnique(duplicateTitle));
                });

            dataSetNamesCsvEntries
                .GroupBy(entry => entry.BaseFilename)
                .Where(group => group.Count() > 1)
                .Select(group => group.Key)
                .ToList()
                .ForEach(duplicateFilename =>
                {
                    _errors.Add(ValidationMessages.GenerateErrorDataSetNamesCsvFilenamesShouldBeUnique(duplicateFilename));
                });
        }

        private void CheckBulkDataZipForUnusedFiles(
            List<DataSetIndexItem> indexItems,
            List<DataSetFileDto> dataSetFiles)
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

                _errors.Add(ValidationMessages.GenerateErrorZipContainsUnusedFiles(remainingFileNames));
            }
        }

        private void ValidateDataSetTitleDuplication(
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
                    _errors.Add(ValidationMessages.GenerateErrorDataSetTitleShouldBeUnique(title));
                }
            }
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

        private void ValidateDataFileNames(
            Guid releaseVersionId,
            string dataFileName,
            File? replacingFile = null)
        {
            // - Original uploads' data filename is not unique if a ReleaseFile exists with the same filename.
            // - With replacement uploads, we can ignore a preexisting ReleaseFile if it is the file being replaced -
            // we only care if the preexisting duplicate ReleaseFile name isn't the file being replaced.
            if (IsFileExisting(releaseVersionId, FileType.Data, dataFileName) &&
                (replacingFile == null || replacingFile.Filename != dataFileName))
            {
                _errors.Add(ValidationMessages.GenerateErrorFilenameNotUnique(dataFileName, FileType.Data));
            }

            // NOTE: We allow duplicate meta file names - meta files aren't included in publicly downloadable
            // zips, so meta files won't be included in the same directory by filename and thereby cannot clash
        }
    }
}
