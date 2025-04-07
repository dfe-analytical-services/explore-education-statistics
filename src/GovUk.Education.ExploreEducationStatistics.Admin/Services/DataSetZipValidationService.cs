#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Validators;
using GovUk.Education.ExploreEducationStatistics.Common;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services
{
    public class DataSetZipValidationService(ContentDbContext contentDbContext) : IDataSetZipValidationService
    {
        private readonly List<ErrorViewModel> _errors = [];

        // TODO: Move to FileUploadsValidatorService (optionally combine with existing ValidateDataSet function)
        public async Task<Either<ActionResult, List<ZippedDataSet>>> ValidateBulkDataZipFiles(
            Guid releaseVersionId,
            List<DataSetFileDto> dataSetFiles)
        {
            var unprocessedFiles = dataSetFiles;

            var dataSetNames = dataSetFiles.FirstOrDefault(dsf => dsf.FileName == "dataset_names.csv");
            if (dataSetNames is null)
            {
                return Common.Validators.ValidationUtils.ValidationResult(new ErrorViewModel
                {
                    Code = ValidationMessages.BulkDataZipMustContainDataSetNamesCsv.Code,
                    Message = ValidationMessages.BulkDataZipMustContainDataSetNamesCsv.Message,
                });
            }

            unprocessedFiles.Remove(dataSetNames);

            var headers = await CsvUtils.GetCsvHeaders(dataSetNames.FileStream, leaveOpen: true);

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

            dataSetNames.FileStream.SeekToBeginning();
            var rows = await CsvUtils.GetCsvRows(dataSetNames.FileStream);

            var dataSetNamesCsvEntries = new List<(string BaseFilename, string Title)>();
            foreach (var row in rows)
            {
                var filename = row[fileNameIndex].Replace(".csv", "");
                var datasetName = row[datasetNameIndex].Trim();

                if (datasetName.Length > 120)
                {
                    _errors.Add(ValidationMessages.GenerateErrorDataSetTitleTooLong(datasetName, 120));
                }

                dataSetNamesCsvEntries.Add((BaseFilename: filename, Title: datasetName));
            }

            CheckIndexFileForDuplicationErrors(dataSetNamesCsvEntries);

            if (_errors.Count > 0)
            {
                return Common.Validators.ValidationUtils.ValidationResult(_errors);
            }

            var zippedDataSetFiles = new List<ZippedDataSet>();
            foreach (var (BaseFilename, Title) in dataSetNamesCsvEntries)
            {
                var dataFile = unprocessedFiles.FirstOrDefault(f => f.FileName == $"{BaseFilename}{Constants.DataSet.DataFileExtension}");
                var metaFile = unprocessedFiles.FirstOrDefault(f => f.FileName == $"{BaseFilename}{Constants.DataSet.MetaFileExtension}");

                if (dataFile is null)
                {
                    _errors.Add(ValidationMessages.GenerateErrorFileNotFoundInZip($"{BaseFilename}{Constants.DataSet.DataFileExtension}", FileType.Data));
                }
                else
                {
                    unprocessedFiles.Remove(dataFile);
                }

                if (metaFile is null)
                {
                    _errors.Add(ValidationMessages.GenerateErrorFileNotFoundInZip($"{BaseFilename}{Constants.DataSet.MetaFileExtension}", FileType.Metadata));
                }
                else
                {
                    unprocessedFiles.Remove(metaFile);
                }

                if (dataFile is not null && metaFile is not null)
                {
                    try
                    {
                        // We replace files with the same title. If there is no releaseFile with the same title, it's a new data set.
                        var releaseFileToBeReplaced = await contentDbContext.ReleaseFiles
                            .Include(rf => rf.File)
                            .SingleOrDefaultAsync(rf =>
                                rf.ReleaseVersionId == releaseVersionId &&
                                rf.File.Type == FileType.Data &&
                                rf.Name == Title);

                        zippedDataSetFiles.Add(new()
                        {
                            DataFile = dataFile,
                            MetaFile = metaFile,
                            Title = Title,
                            ReplacingFile = releaseFileToBeReplaced?.File,
                        });
                    }
                    catch (InvalidOperationException)
                    {
                        _errors.Add(ValidationMessages.GenerateErrorDataReplacementAlreadyInProgress());
                    }
                }
            }

            if (unprocessedFiles.Count > 0)
            {
                _errors.Add(ValidationMessages.GenerateErrorZipContainsUnusedFiles(
                    unprocessedFiles
                        .Select(file => file.FileName)
                        .ToList()));
            }

            if (_errors.Count > 0)
            {
                return Common.Validators.ValidationUtils.ValidationResult(_errors);
            }

            return zippedDataSetFiles;
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
    }
}
