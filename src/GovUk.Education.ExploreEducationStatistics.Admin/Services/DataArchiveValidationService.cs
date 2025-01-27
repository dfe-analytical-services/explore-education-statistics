#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Validators;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services
{
    public class DataArchiveValidationService(
        ContentDbContext contentDbContext,
        IFileTypeService fileTypeService,
        IFileUploadsValidatorService fileUploadsValidatorService)
        : IDataArchiveValidationService
    {
        public const int MaxFilenameSize = 150;

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
                metaFile.FullName,
                dataFile.Length,
                metaFile.Length,
                replacingFile);

            await using (var dataFileStream = dataFile.Open())
            await using (var metaFileStream = metaFile.Open())
            {
                errors.AddRange(await fileUploadsValidatorService
                    .ValidateDataSetFilesForUpload(
                        releaseVersionId,
                        archiveDataSet,
                        dataFileStream,
                        metaFileStream));
            }

            if (errors.Count > 0)
            {
                return Common.Validators.ValidationUtils.ValidationResult(errors);
            }

            return archiveDataSet;
        }

        public async Task<Either<ActionResult, List<ArchiveDataSetFile>>> ValidateBulkDataArchiveFiles(
            Guid releaseVersionId,
            IFormFile zipFile)
        {
            await using var stream = zipFile.OpenReadStream();
            using var archive = new ZipArchive(stream);

            var unprocessedArchiveFiles = archive.Entries.ToList();

            var dataSetNamesFile = archive.GetEntry("dataset_names.csv");

            if (dataSetNamesFile == null)
            {
                return Common.Validators.ValidationUtils.ValidationResult(new ErrorViewModel
                {
                    Code = ValidationMessages.BulkDataZipMustContainDataSetNamesCsv.Code,
                    Message = ValidationMessages.BulkDataZipMustContainDataSetNamesCsv.Message,
                });
            }

            unprocessedArchiveFiles.Remove(dataSetNamesFile);

            List<string> headers;
            await using (var dataSetNamesStream = dataSetNamesFile.Open())
            {
                headers = await CsvUtils.GetCsvHeaders(dataSetNamesStream);
            }

            if (headers is not ["file_name", "dataset_name"])
            {
                return Common.Validators.ValidationUtils.ValidationResult(new ErrorViewModel
                {
                    Code = ValidationMessages.DataSetNamesCsvIncorrectHeaders.Code,
                    Message = ValidationMessages.DataSetNamesCsvIncorrectHeaders.Message,
                });
            }

            var fileNameIndex = headers[0] == "file_name" ? 0 : 1;
            var datasetNameIndex = headers[0] == "dataset_name" ? 0 : 1;

            List<List<string>> rows;
            await using (var dataSetNamesStream = dataSetNamesFile.Open())
            {
                rows = await CsvUtils.GetCsvRows(dataSetNamesStream);
            }

            var errors = new List<ErrorViewModel>();

            var dataSetNamesCsvEntries = new List<(string BaseFilename, string Title)>();
            foreach (var row in rows)
            {
                var filename = row[fileNameIndex];
                var datasetName = row[datasetNameIndex].Trim();

                if (datasetName.Length > 120)
                {
                    errors.Add(ValidationMessages.GenerateErrorDataSetTitleTooLong(datasetName));
                }

                dataSetNamesCsvEntries.Add((BaseFilename: filename, Title: datasetName));
            }

            dataSetNamesCsvEntries
                .Select(entry => entry.BaseFilename)
                .Where(baseFilename => baseFilename.EndsWith(".csv"))
                .ToList()
                .ForEach(baseFilename =>
                {
                    errors.Add(ValidationMessages.GenerateErrorDataSetNamesCsvFilenamesShouldNotEndDotCsv(baseFilename));
                });

            // Check for duplicate data set titles - because the bulk zip itself may contain duplicates!
            dataSetNamesCsvEntries
                .GroupBy(entry => entry.Title)
                .Where(group => group.Count() > 1)
                .Select(group => group.Key)
                .ToList()
                .ForEach(duplicateTitle =>
                {
                    errors.Add(ValidationMessages
                        .GenerateErrorDataSetTitleShouldBeUnique(duplicateTitle));
                });

            // Check for duplicate data set filenames - because the bulk zip itself may contain duplicates!
            dataSetNamesCsvEntries
                .GroupBy(entry => entry.BaseFilename)
                .Where(group => group.Count() > 1)
                .Select(group => group.Key)
                .ToList()
                .ForEach(duplicateFilename =>
                {
                    errors.Add(ValidationMessages
                        .GenerateErrorDataSetNamesCsvFilenamesShouldBeUnique(duplicateFilename));
                });

            if (errors.Count > 0)
            {
                return Common.Validators.ValidationUtils.ValidationResult(errors);
            }

            var dataSetFiles = new List<ArchiveDataSetFile>();
            foreach (var entry in dataSetNamesCsvEntries)
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
                    errors.Add(ValidationMessages.GenerateErrorFileNotFoundInZip($"{entry.BaseFilename}.meta.csv", FileType.Metadata));
                }
                else
                {
                    unprocessedArchiveFiles.Remove(metaFile);
                }

                if (dataFile != null && metaFile != null)
                {
                    try
                    {
                        // We replace files with the same title. If there is no releaseFile with the same title, it's a new data set.
                        var releaseFileToBeReplaced = await contentDbContext.ReleaseFiles
                            .Include(rf => rf.File)
                            .SingleOrDefaultAsync(rf =>
                                rf.ReleaseVersionId == releaseVersionId
                                && rf.File.Type == FileType.Data
                                && rf.Name == entry.Title);

                        var dataArchiveFile = new ArchiveDataSetFile(
                        entry.Title,
                        dataFile.FullName,
                        metaFile.FullName,
                        dataFile.Length,
                        metaFile.Length,
                        releaseFileToBeReplaced?.File);

                        await using (var dataFileStream = dataFile.Open())
                        await using (var metaFileStream = metaFile.Open())
                        {
                            errors.AddRange(await fileUploadsValidatorService.ValidateDataSetFilesForUpload(
                                releaseVersionId,
                                dataArchiveFile,
                                dataFileStream: dataFileStream,
                                metaFileStream: metaFileStream));
                        }

                        dataSetFiles.Add(dataArchiveFile);
                    }
                    catch (InvalidOperationException)
                    {
                        errors.Add(ValidationMessages.GenerateErrorDataReplacementAlreadyInProgress());
                    }
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

        public async Task<List<ErrorViewModel>> IsValidZipFile(IFormFile zipFile)
        {
            List<ErrorViewModel> errors = [];

            if (zipFile is null)
            {
                errors.Add(ValidationMessages.GenerateErrorFileIsNull());
                return errors;
            }

            if (zipFile.FileName.Length > MaxFilenameSize)
            {
                errors.Add(ValidationMessages.GenerateErrorFilenameTooLong(
                    zipFile.FileName, MaxFilenameSize));
            }

            if (!zipFile.FileName.ToLower().EndsWith(".zip"))
            {
                errors.Add(ValidationMessages.GenerateErrorZipFilenameMustEndDotZip(zipFile.FileName));
            }

            if (!await fileTypeService.IsValidZipFile(zipFile))
            {
                errors.Add(ValidationMessages.GenerateErrorMustBeZipFile(zipFile.FileName));
            }

            return errors;
        }
    }
}
