#nullable enable
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
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
using static GovUk.Education.ExploreEducationStatistics.Common.Model.FileType;
using static GovUk.Education.ExploreEducationStatistics.Common.Validators.FileTypeValidationUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services
{
    public class DataArchiveValidationService : IDataArchiveValidationService
    {
        private readonly IFileTypeService _fileTypeService;
        private readonly IFileUploadsValidatorService _fileUploadsValidatorService;

        public const int MaxFilenameSize = 150;

        public DataArchiveValidationService(
            IFileTypeService fileTypeService,
            IFileUploadsValidatorService fileUploadsValidatorService)
        {
            _fileTypeService = fileTypeService;
            _fileUploadsValidatorService = fileUploadsValidatorService;
        }

        public async Task<Either<ActionResult, ArchiveDataSetFile>> ValidateDataArchiveFile(
            Guid releaseVersionId,
            string dataSetTitle,
            IFormFile zipFile,
            Content.Model.File? replacingFile = null)
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
                return Common.Validators.ValidationUtils.ValidationResult(new ErrorViewModel
                {
                    Code = ValidationMessages.DataZipShouldContainTwoFiles.Code,
                    Message = ValidationMessages.DataZipShouldContainTwoFiles.Message,
                });
            }

            var file1 = archive.Entries[0];
            var file2 = archive.Entries[1];

            var dataFile = file1.Name.EndsWith(".meta.csv") ? file2 : file1;
            var metaFile = file1.Name.EndsWith(".meta.csv") ? file1 : file2;

            if (errors.Count > 0)
            {
                return Common.Validators.ValidationUtils.ValidationResult(errors);
            }

            var archiveDataSet = new ArchiveDataSetFile(
                dataSetTitle,
                dataFile.FullName,
                dataFile.Length,
                metaFile.FullName,
                metaFile.Length);

            await using (var dataFileStream = dataFile.Open())
            await using (var metaFileStream = metaFile.Open())
            {
                errors.AddRange(await _fileUploadsValidatorService
                    .ValidateDataSetFilesForUpload(
                        releaseVersionId,
                        archiveDataSet,
                        dataFileStream,
                        metaFileStream,
                        replacingFile));
            }

            if (errors.Count > 0)
            {
                return Common.Validators.ValidationUtils.ValidationResult(errors);
            }

            return archiveDataSet;
        }

        public async Task<Either<ActionResult, List<ArchiveDataSetFile>>> ValidateBulkDataArchiveFile(
            Guid releaseVersionId,
            IFormFile zipFile)
        {
            var errors = await IsValidZipFile(zipFile);
            if (errors.Count > 0)
            {
                return Common.Validators.ValidationUtils.ValidationResult(errors);
            }

            await using var stream = zipFile.OpenReadStream();
            using var archive = new ZipArchive(stream);

            var unprocessedArchiveFiles = archive.Entries.ToList();

            var dataSetNamesFile = archive.GetEntry("dataset_names.csv");

            if (dataSetNamesFile == null)
            {
                return Common.Validators.ValidationUtils.ValidationResult(new ErrorViewModel
                    {
                        Code = ValidationMessages.BulkDataZipMustContainDatasetNamesCsv.Code,
                        Message = ValidationMessages.BulkDataZipMustContainDatasetNamesCsv.Message,
                    });
            }

            unprocessedArchiveFiles.Remove(dataSetNamesFile);

            await using var dataSetNamesStream = dataSetNamesFile.Open();
            using var dataSetNamesReader = new StreamReader(dataSetNamesStream);
            using var dataSetNamesCsvReader = new CsvReader(dataSetNamesReader, CultureInfo.InvariantCulture);

            try
            {
                await dataSetNamesCsvReader.ReadAsync();
                dataSetNamesCsvReader.ReadHeader();
            }
            catch (ReaderException e)
            {
                return Common.Validators.ValidationUtils.ValidationResult(
                    ValidationMessages.GenerateErrorDatasetNamesCsvReaderException(e.ToString())); // @MarkFix do we want an exception posting?
            }

            var headers = dataSetNamesCsvReader.HeaderRecord?.ToList() ?? new List<string>();

            if (headers is not ["file_name", "dataset_name"])
            {
                return Common.Validators.ValidationUtils.ValidationResult(new ErrorViewModel
                    {
                        Code = ValidationMessages.DatasetNamesCsvIncorrectHeaders.Code,
                        Message = ValidationMessages.DatasetNamesCsvIncorrectHeaders.Message,
                    });
            }

            using var csvDataReader = new CsvDataReader(dataSetNamesCsvReader);

            var lastLine = false; // Assume one row of data

            // Read dataset_names.csv entries
            var dataSetNamesCsvEntries = new List<(string BaseFilename, string Title)>();
            while (!lastLine)
            {
                var filename = dataSetNamesCsvReader.GetField<string>("file_name");
                var datasetName = dataSetNamesCsvReader.GetField<string>("dataset_name");

                dataSetNamesCsvEntries.Add((BaseFilename: filename, Title: datasetName));

                lastLine = !await dataSetNamesCsvReader.ReadAsync();
            }

            dataSetNamesCsvEntries
                .Select(entry => entry.BaseFilename)
                .Where(baseFilename => baseFilename.EndsWith(".csv"))
                .ForEach(baseFilename =>
                {
                    errors.Add(ValidationMessages.GenerateErrorDatasetNamesCsvFilenamesShouldNotEndDotCsv(baseFilename));
                });

            // Check for duplicate data set titles - because the bulk zip itself main contain duplicates!
            dataSetNamesCsvEntries
                .GroupBy(entry => entry.Title)
                .Where(group => group.Count() > 1)
                .Select(group => group.Key)
                .ForEach(duplicateTitle =>
                {
                    errors.Add(ValidationMessages
                        .GenerateErrorDataSetTitleShouldBeUnique(duplicateTitle));
                });

            // Check for duplicate data set filenames - because the bulk zip itself main contain duplicates!
            dataSetNamesCsvEntries
                .GroupBy(entry  => entry.BaseFilename)
                .Where(group => group.Count() > 1)
                .Select(group => group.Key)
                .ForEach(duplicateFilename =>
                {
                    errors.Add(ValidationMessages
                        .GenerateErrorDatasetNamesCsvFilenamesShouldBeUnique(duplicateFilename));
                });

            if (errors.Count > 0)
            {
                return Common.Validators.ValidationUtils.ValidationResult(errors);
            }

            var dataSetFiles = new List<ArchiveDataSetFile>();
            foreach(var entry in dataSetNamesCsvEntries)
            {
                var dataFile = archive.GetEntry($"{entry.BaseFilename}.csv");
                var metaFile = archive.GetEntry($"{entry.BaseFilename}.meta.csv");

                if (dataFile == null)
                {
                    errors.Add(ValidationMessages.GenerateErrorFileNotFoundInZip($"{entry.BaseFilename}.csv", FileType.Data));
                }
                else
                {
                    unprocessedArchiveFiles.Remove(dataFile);
                }

                if (metaFile == null)
                {
                    errors.Add(ValidationMessages.GenerateErrorFileNotFoundInZip($"{entry.BaseFilename}.meta.csv", Metadata));
                }
                else
                {
                    unprocessedArchiveFiles.Remove(metaFile);
                }

                if (dataFile != null && metaFile != null)
                {
                    var dataArchiveFile = new ArchiveDataSetFile(
                        entry.Title,
                        dataFile.FullName,
                        dataFile.Length,
                        metaFile.FullName,
                        metaFile.Length);

                    await using (var dataFileStream = dataFile.Open())
                    await using (var metaFileStream = metaFile.Open())
                    {
                        errors.AddRange(await _fileUploadsValidatorService.ValidateDataSetFilesForUpload(
                            releaseVersionId,
                            dataArchiveFile,
                            dataFileStream: dataFileStream,
                            metaFileStream: metaFileStream));
                    }

                    dataSetFiles.Add(dataArchiveFile);
                }
            }

            // Check for unused files in ZIP
            if (unprocessedArchiveFiles.Count > 0)
            {
                errors.Add(ValidationMessages.GenerateErrorZipContainsUnusedFiles(
                    unprocessedArchiveFiles
                        .Select(file => file.FullName)
                        .ToList()));
            }

            if (errors.Count > 0)
            {
                return Common.Validators.ValidationUtils.ValidationResult(errors);
            }

            return dataSetFiles;
        }

        private async Task<List<ErrorViewModel>> IsValidZipFile(IFormFile zipFile)
        {
            List<ErrorViewModel> errors = [];

            if (zipFile.FileName.Length > MaxFilenameSize)
            {
                errors.Add(ValidationMessages.GenerateErrorFilenameTooLong(
                        zipFile.FileName, MaxFilenameSize));
            }

            if (!zipFile.FileName.ToLower().EndsWith(".zip"))
            {
                errors.Add(ValidationMessages.GenerateErrorZipFilenameMustEndDotZip(zipFile.FileName));
            }

            if (!await _fileTypeService.HasMatchingMimeType(
                    zipFile,
                    AllowedArchiveMimeTypes
                )
                || !_fileTypeService.HasMatchingEncodingType(zipFile, AllowedArchiveEncodingTypes))
            {
                errors.Add(ValidationMessages.GenerateErrorMustBeZipFile(zipFile.FileName));
            }

            return errors;
        }
    }
}
