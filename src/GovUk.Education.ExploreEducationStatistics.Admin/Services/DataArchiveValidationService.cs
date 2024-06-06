#nullable enable
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CsvHelper;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Validators;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationErrorMessages;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Model.FileType;
using static GovUk.Education.ExploreEducationStatistics.Common.Validators.FileTypeValidationUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services
{
    public class DataArchiveValidationService : IDataArchiveValidationService
    {
        private readonly IFileTypeService _fileTypeService;

        private const int MaxFilenameLength = 150;

        private static readonly Dictionary<FileType, IEnumerable<Regex>> AllowedMimeTypesByFileType =
            new()
            {
                {DataZip, AllowedArchiveMimeTypes}
            };

        public DataArchiveValidationService(IFileTypeService fileTypeService)
        {
            _fileTypeService = fileTypeService;
        }

        public async Task<Either<ActionResult, IDataArchiveFile>> ValidateDataArchiveFile(IFormFile zipFile)
        {
            var errors = await IsValidZipFile(zipFile);
            if (errors.Count > 0)
            {
                return Common.Validators.ValidationUtils.ValidationResult(errors);
            }

            await using var stream = zipFile.OpenReadStream();
            using var archive = new ZipArchive(stream);

            if (archive.Entries.Count != 2)
            {
                return ValidationActionResult(DataZipFileCanOnlyContainTwoFiles);
            }

            var file1 = archive.Entries[0];
            var file2 = archive.Entries[1];

            if (!file1.FullName.EndsWith(".csv") || !file2.FullName.EndsWith(".csv"))
            {
                return ValidationActionResult(DataZipFileDoesNotContainCsvFiles);
            }

            var dataFile = file1.Name.Contains(".meta.") ? file2 : file1;
            var metaFile = file1.Name.Contains(".meta.") ? file1 : file2;

            var filenamesValid = ValidateFilenameLengths(
                dataFile.Name.Length,
                metaFile.Name.Length);

            if (filenamesValid.IsLeft)
            {
                return filenamesValid.Left;
            }

            return new DataArchiveFile(dataFile: dataFile, metaFile: metaFile);
        }

        public async Task<Either<ActionResult, List<BulkDataArchiveFile>>> ValidateBulkDataArchiveFile(IFormFile zipFile)
        {
            var errors = await IsValidZipFile(zipFile);
            if (errors.Count > 0)
            {
                return Common.Validators.ValidationUtils.ValidationResult(errors);
            }

            await using var stream = zipFile.OpenReadStream();
            using var archive = new ZipArchive(stream);

            ZipArchiveEntry? datasetNamesEntry = null;

            var unprocessedArchiveFiles = archive.Entries.ToList();
            foreach (var zipArchiveEntry in archive.Entries)
            {
                if (zipArchiveEntry.FullName == "dataset_names.csv")
                {
                    datasetNamesEntry = zipArchiveEntry;
                    unprocessedArchiveFiles.Remove(zipArchiveEntry);
                    break;
                }
            }

            if (datasetNamesEntry == null)
            {
                return Common.Validators.ValidationUtils.ValidationResult(new ErrorViewModel
                    {
                        Code = ValidationMessages.BulkDataZipMustContainDatasetNamesCsv.Code,
                        Message = ValidationMessages.BulkDataZipMustContainDatasetNamesCsv.Message,
                    });
            }

            var datasetNamesStream = datasetNamesEntry.Open();
            var datasetNamesReader = new StreamReader(datasetNamesStream);
            var datasetNamesCsvReader = new CsvReader(datasetNamesReader, CultureInfo.InvariantCulture);

            try
            {
                await datasetNamesCsvReader.ReadAsync(); // @MarkFix what happens if headers but no data here?
                datasetNamesCsvReader.ReadHeader();
            }
            catch (ReaderException e)
            {
                return Common.Validators.ValidationUtils.ValidationResult(
                    ValidationMessages.GenerateErrorDatasetNamesCsvReaderException(e.ToString()));
            }

            var headers = datasetNamesCsvReader.HeaderRecord?.ToList() ?? new List<string>();

            if (headers is not ["file_name", "dataset_name"])
            {
                return Common.Validators.ValidationUtils.ValidationResult(new ErrorViewModel
                    {
                        Code = ValidationMessages.DatasetNamesCsvIncorrectHeaders.Code,
                        Message = ValidationMessages.DatasetNamesCsvIncorrectHeaders.Message,
                    });
            }

            var results = new List<BulkDataArchiveFile>();

            using var csvDataReader = new CsvDataReader(datasetNamesCsvReader);

            var lastLine = false; // Assume one row of data // @MarkFix yeah? test it

            while (!lastLine)
            {
                var filename = datasetNamesCsvReader.GetField<string>("file_name");
                var datasetName = datasetNamesCsvReader.GetField<string>("dataset_name");

                ZipArchiveEntry? dataFile = null;
                ZipArchiveEntry? metaFile = null;

                foreach (var zipArchiveEntry in archive.Entries)
                {
                    if (zipArchiveEntry.FullName == $"{filename}.csv")
                    {
                        dataFile = zipArchiveEntry;
                        unprocessedArchiveFiles.Remove(zipArchiveEntry);
                        continue;
                    }

                    if (zipArchiveEntry.FullName == $"{filename}.meta.csv")
                    {
                        metaFile = zipArchiveEntry;
                        unprocessedArchiveFiles.Remove(zipArchiveEntry);
                    }
                }

                if (dataFile == null)
                {
                    errors.Add(ValidationMessages.GenerateErrorDataFileNotFoundInZip($"{filename}.csv"));
                }

                if (metaFile == null)
                {
                    errors.Add(ValidationMessages.GenerateErrorMetaFileNotFoundInZip($"{filename}.meta.csv"));
                }

                // @MarkFix More Data/Meta file validation here?

                results.Add(new BulkDataArchiveFile(datasetName, dataFile: dataFile, metaFile: metaFile));

                lastLine = !await datasetNamesCsvReader.ReadAsync();
            }

            // Check filename lengths
            foreach (var zipArchiveEntry in archive.Entries)
            {
                if (zipArchiveEntry.Length > MaxFilenameLength)
                {
                    errors.Add(ValidationMessages.GenerateErrorFileNameTooLong(
                        zipArchiveEntry.FullName, MaxFilenameLength));
                }
            }

            // Check for duplicate data set names
            results
                .GroupBy(file => file.DataSetName)
                .Where(group => group.Count() > 1)
                .Select(group => group.Key)
                .ForEach(duplicateDatasetName =>
                {
                    errors.Add(ValidationMessages
                        .GenerateErrorBulkDataZipContainsDuplicateDatasetNames(duplicateDatasetName));
                });

            // Check for unused files in ZIP
            if (unprocessedArchiveFiles.Count > 0)
            {
                // @MarkFix do we want to do this - probably hit MacOs archive hidden files and fail?
                errors.Add(ValidationMessages.GenerateErrorZipContainsUnusedFiles(
                    unprocessedArchiveFiles
                        .Select(file => file.FullName)
                        .ToList()));
            }

            // Check ZIP contains at least one data set
            if (results.Count == 0)
            {
                errors.Add(new ErrorViewModel
                {
                    Code = ValidationMessages.BulkDataZipShouldContainDataSets.Code,
                    Message = ValidationMessages.BulkDataZipShouldContainDataSets.Message,
                });
            }

            if (errors.Count > 0)
            {
                return Common.Validators.ValidationUtils.ValidationResult(errors);
            }

            return results;
        }

        private async Task<List<ErrorViewModel>> IsValidZipFile(IFormFile file)
        {
            List<ErrorViewModel> errors = [];

            if (file.FileName.Length > MaxFilenameLength)
            {
                errors.Add(ValidationMessages.GenerateErrorFileNameTooLong(
                        file.FileName, MaxFilenameLength));
            }

            if (!file.FileName.ToLower().EndsWith(".zip"))
            {
                errors.Add(ValidationMessages.GenerateErrorZipFilenameMustEndDotZip(file.FileName));
            }

            if (!await _fileTypeService.HasMatchingMimeType(
                    file,
                    AllowedMimeTypesByFileType[DataZip]
                )
                || !_fileTypeService.HasMatchingEncodingType(file, ZipEncodingTypes))
            {
                errors.Add(ValidationMessages.GenerateErrorMustBeZipFile(file.FileName));
            }

            return errors;
        }

        private static Either<ActionResult, Unit> ValidateFilenameLengths(
            int dataFilenameLength,
            int metaFilenameLength)
        {
            if (dataFilenameLength > MaxFilenameLength && metaFilenameLength > MaxFilenameLength)
            {
                return ValidationActionResult(DataZipContentFilenamesTooLong);
            }

            if (dataFilenameLength > MaxFilenameLength)
            {
                return ValidationActionResult(DataFilenameTooLong);
            }

            if (metaFilenameLength > MaxFilenameLength)
            {
                return ValidationActionResult(MetaFilenameTooLong);
            }

            return Unit.Instance;
        }
    }
}
