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
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
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

        private const int MaxFilenameSize = 150;

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
            if (!await IsZipFile(zipFile))
            {
                return ValidationActionResult(DataZipMustBeZipFile);
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
                zipFile.FileName.Length,
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
            if (!await IsZipFile(zipFile))
            {
                return ValidationActionResult(DataZipMustBeZipFile);
            }

            await using var stream = zipFile.OpenReadStream();
            using var archive = new ZipArchive(stream);

            ZipArchiveEntry? datasetNamesEntry = null;
            foreach (var zipArchiveEntry in archive.Entries)
            {
                if (zipArchiveEntry.FullName == "dataset_names.csv")
                {
                    datasetNamesEntry = zipArchiveEntry;
                }
            }

            if (datasetNamesEntry == null)
            {
                return ValidationActionResult(DataBulkZipFileMustContainDatasetNamesCsv);
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
                // @MarkFix can we return the error from the exception here?
                return ValidationActionResult(DataBulkZipErrorReadingDatasetNamesCsv);
            }

            var headers = datasetNamesCsvReader.HeaderRecord?.ToList() ?? new List<string>();

            if (headers is not ["file_name", "dataset_name"])
            {
                return ValidationActionResult(DataBulkZipDatasetNamesCsvMustContainTwoColumnsNamedCorrectly);
            }

            var results = new List<BulkDataArchiveFile>();

            using var csvDataReader = new CsvDataReader(datasetNamesCsvReader);

            var lastLine = false; // Assume one row of data // @MarkFix yeah? test it

            while (!lastLine)
            {
                var row = Enumerable
                    .Range(0, csvDataReader.FieldCount)
                    .Select(datasetNamesCsvReader.GetField<string>)
                    .ToList();

                var fileName = row[0];
                var datasetName = row[1];

                ZipArchiveEntry? dataFile = null;
                ZipArchiveEntry? metaFile = null;

                foreach (var zipArchiveEntry in archive.Entries)
                {
                    if (zipArchiveEntry.FullName == $"{fileName}.csv")
                    {
                        dataFile = zipArchiveEntry;
                    }

                    if (zipArchiveEntry.FullName == $"{fileName}.meta.csv")
                    {
                        metaFile = zipArchiveEntry;
                    }
                }

                if (dataFile == null)
                {
                    return ValidationActionResult(DataBulkZipCannotFindDataFile);
                }

                if (metaFile == null)
                {
                    return ValidationActionResult(DataBulkZipCannotFindMetaFile);
                }

                // @MarkFix More Data/Meta file validation here?

                results.Add(new BulkDataArchiveFile(datasetName, dataFile: dataFile, metaFile: metaFile));

                lastLine = !await datasetNamesCsvReader.ReadAsync();
            }

            // @MarkFix do we want to do this - probably hit MacOs archive hidden files and fail?
            //if (archive.Entries.Count != (2 * results.Count) + 1) // +1 for dataset_names.csv
            //{
            //
            //}

            if (results.Count == 0)
            {
                return ValidationActionResult(DataBulkZipCannotHaveNoDatasets);
            }

            return results;
        }

        private async Task<bool> IsZipFile(IFormFile file)
        {
            if (!file.FileName.ToLower().EndsWith(".zip"))
            {
                return false;
            }

            return await _fileTypeService.HasMatchingMimeType(
                       file,
                       AllowedMimeTypesByFileType[DataZip]
                   )
                   && _fileTypeService.HasMatchingEncodingType(file, ZipEncodingTypes);
        }

        private static Either<ActionResult, Unit> ValidateFilenameLengths(
            int zipFilenameLength,
            int dataFilenameLength,
            int metaFilenameLength)
        {
            if (zipFilenameLength > MaxFilenameSize)
            {
                return ValidationActionResult(DataZipFilenameTooLong);
            }

            if (dataFilenameLength > MaxFilenameSize && metaFilenameLength > MaxFilenameSize)
            {
                return ValidationActionResult(DataZipContentFilenamesTooLong);
            }

            if (dataFilenameLength > MaxFilenameSize)
            {
                return ValidationActionResult(DataFilenameTooLong);
            }

            if (metaFilenameLength > MaxFilenameSize)
            {
                return ValidationActionResult(MetaFilenameTooLong);
            }

            return Unit.Instance;
        }
    }
}
