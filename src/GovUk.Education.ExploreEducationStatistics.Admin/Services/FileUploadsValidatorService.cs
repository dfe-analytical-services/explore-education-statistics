#nullable enable
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationErrorMessages;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Validators.FileTypeValidationUtils;
using static System.StringComparison;
using static GovUk.Education.ExploreEducationStatistics.Common.Model.FileType;
using File = GovUk.Education.ExploreEducationStatistics.Content.Model.File;
using GovUk.Education.ExploreEducationStatistics.Admin.Validators;
using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services
{
    public class FileUploadsValidatorService : IFileUploadsValidatorService
    {
        private readonly IFileTypeService _fileTypeService;
        private readonly ContentDbContext _context;

        public const int MaxFilenameSize = 150;
        private const int MaxFileSize = int.MaxValue; // 2GB

        public FileUploadsValidatorService(IFileTypeService fileTypeService,
            ContentDbContext context)
        {
            _fileTypeService = fileTypeService;
            _context = context;
        }

        public async Task<List<ErrorViewModel>> ValidateDataSetFilesForUpload(
            Guid releaseVersionId,
            string dataSetTitle,
            string dataFileName,
            long dataFileLength,
            Stream dataFileStream,
            string metaFileName,
            long metaFileLength,
            Stream metaFileStream,
            File? replacingFile = null)
        {
            List<ErrorViewModel> errors = [];
            var isReplacement = replacingFile != null;

            errors.AddRange(ValidateDataSetTitle(
                releaseVersionId,
                dataSetTitle,
                isReplacement));

            errors.AddRange(ValidateDataFileNames(
                releaseVersionId,
                dataFileName: dataFileName,
                metaFileName: metaFileName,
                replacingFile));

            errors.AddRange(ValidateDataFileSizes(
                dataFileLength,
                dataFileName,
                metaFileLength,
                metaFileName));

            errors.AddRange(await ValidateDataFileTypes(
                dataFileName,
                dataFileStream,
                metaFileName,
                metaFileStream));

            return errors;
        }

        public async Task<List<ErrorViewModel>> ValidateDataSetFilesForUpload(
            Guid releaseVersionId,
            string dataSetTitle,
            IFormFile dataFile,
            IFormFile metaFile,
            File? replacingFile = null)
        {
            await using var dataFileStream = dataFile.OpenReadStream();
            await using var metaFileStream = metaFile.OpenReadStream();

            return await ValidateDataSetFilesForUpload(
                releaseVersionId: releaseVersionId,
                dataSetTitle: dataSetTitle,
                dataFileName:dataFile.FileName,
                dataFileLength:dataFile.Length,
                dataFileStream: dataFileStream,
                metaFileName:metaFile.FileName,
                metaFileLength:metaFile.Length,
                metaFileStream: metaFileStream,
                replacingFile: replacingFile);
        }

        public async Task<List<ErrorViewModel>> ValidateDataSetFilesForUpload(
            Guid releaseVersionId,
            ArchiveDataSetFile archiveDataSet,
            Stream dataFileStream,
            Stream metaFileStream)
        {
            return await ValidateDataSetFilesForUpload(
                releaseVersionId: releaseVersionId,
                dataSetTitle: archiveDataSet.Title,
                dataFileName: archiveDataSet.DataFilename,
                dataFileLength: archiveDataSet.DataFileSize,
                dataFileStream: dataFileStream,
                metaFileName: archiveDataSet.MetaFilename,
                metaFileLength: archiveDataSet.MetaFileSize,
                metaFileStream: metaFileStream,
                replacingFile: archiveDataSet.ReplacingFile);
        }

        public async Task<Either<ActionResult, Unit>> ValidateFileForUpload(IFormFile file, FileType type)
        {
            if (type is FileType.Data or Metadata)
            {
                throw new ArgumentException("Cannot use generic function to validate data file", nameof(type));
            }

            // Check that it is not an empty file because this causes issues downstream
            if (file.Length == 0)
            {
                return ValidationActionResult(FileCannotBeEmpty);
            }

            if (file.Length > MaxFileSize)
            {
                return ValidationActionResult(FileSizeLimitExceeded);
            }

            if (!await _fileTypeService.HasMatchingMimeType(file, AllowedMimeTypesByFileType[type]))
            {
                return ValidationActionResult(FileTypeInvalid);
            }

            return Unit.Instance;
        }

        private List<ErrorViewModel> ValidateDataSetTitle(
            Guid releaseVersionId,
            string title,
            bool isReplacement)
        {
            List<ErrorViewModel> errors = [];

            if (!title.Any())
            {
                errors.Add(new ErrorViewModel
                {
                    Code = ValidationMessages.DataSetTitleCannotBeEmpty.Code,
                    Message = ValidationMessages.DataSetTitleCannotBeEmpty.Message,
                });
            }

            if (ContainsSpecialChars(title))
            {
                errors.Add(ValidationMessages.GenerateErrorDataSetTitleShouldNotContainSpecialCharacters(title));
            }

            if (!isReplacement) // if a replacement, we get the title from the replacement which is already validated as unique
            {
                var dataSetNameExists = _context.ReleaseFiles
                    .Include(rf => rf.File)
                    .Any(rf =>
                        rf.ReleaseVersionId == releaseVersionId
                        && rf.File.Type == FileType.Data
                        && rf.Name == title);

                if (dataSetNameExists)
                {
                    errors.Add(ValidationMessages.GenerateErrorDataSetTitleShouldBeUnique(title));
                }
            }

            return errors;
        }

        private static bool ContainsSpacesOrSpecialChars(string filename)
        {
            return filename.IndexOf(" ", Ordinal) > -1 ||
                   ContainsSpecialChars(filename);
        }

        private static bool ContainsSpecialChars(string filename)
        {
            return filename.IndexOf("&", Ordinal) > -1 ||
                   filename.IndexOfAny(Path.GetInvalidFileNameChars()) > -1;
        }

        private bool IsFileExisting(Guid releaseVersionId,
            FileType type,
            string filename)
        {
            return _context
                .ReleaseFiles
                .Include(rf => rf.File)
                .Where(rf => rf.ReleaseVersionId == releaseVersionId
                             && rf.File.Type == type)
                .ToList()
                .Any(rf => string.Equals(rf.File.Filename, filename, CurrentCultureIgnoreCase));
        }

        private List<ErrorViewModel> ValidateDataFileNames(
            Guid releaseVersionId,
            string dataFileName,
            string metaFileName,
            File? replacingFile = null)
        {
            List<ErrorViewModel> errors = [];

            if (string.Equals(dataFileName.ToLower(), metaFileName.ToLower(), OrdinalIgnoreCase))
            {
                errors.Add(new ErrorViewModel
                {
                    Code = ValidationMessages.DataAndMetaFilesCannotHaveSameName.Code,
                    Message = ValidationMessages.DataAndMetaFilesCannotHaveSameName.Message,
                });
            }

            if (ContainsSpacesOrSpecialChars(dataFileName))
            {
                errors.Add(ValidationMessages.GenerateErrorFilenameCannotContainSpacesOrSpecialCharacters(
                    dataFileName));
            }

            if (ContainsSpacesOrSpecialChars(metaFileName))
            {
                errors.Add(ValidationMessages.GenerateErrorFilenameCannotContainSpacesOrSpecialCharacters(
                    metaFileName));
            }

            if (!dataFileName.EndsWith(".csv"))
            {
                errors.Add(ValidationMessages.GenerateErrorFilenameMustEndDotCsv(dataFileName));
            }

            if (!metaFileName.EndsWith(".meta.csv"))
            {
                errors.Add(ValidationMessages.GenerateErrorMetaFilenameMustEndDotMetaDotCsv(metaFileName));
            }

            if (dataFileName.Length > MaxFilenameSize)
            {
                errors.Add(ValidationMessages.GenerateErrorFilenameTooLong(
                    dataFileName, MaxFilenameSize));
            }

            if (metaFileName.Length > MaxFilenameSize)
            {
                errors.Add(ValidationMessages.GenerateErrorFilenameTooLong(
                    metaFileName, MaxFilenameSize));
            }

            // - Original uploads' data filename is not unique if a ReleaseFile exists with the same filename.
            // - With replacement uploads, we can ignore a preexisting ReleaseFile if it is the file being replaced -
            // we only care if the preexisting duplicate ReleaseFile name isn't the file being replaced.
            if (IsFileExisting(releaseVersionId, FileType.Data, dataFileName) &&
                (replacingFile == null || replacingFile.Filename != dataFileName))
            {
                errors.Add(ValidationMessages.GenerateErrorFilenameNotUnique(dataFileName, FileType.Data));
            }

            // NOTE: We allow duplicate meta file names - meta files aren't included in publicly downloadable
            // zips, so meta files won't be included in the same directory by filename and thereby cannot clash

            return errors;
        }

        private static List<ErrorViewModel> ValidateDataFileSizes(
            long dataFileSize,
            string dataFileName,
            long metaFileSize,
            string metaFileName)

        {
            List<ErrorViewModel> errors = [];

            if (dataFileSize == 0)
            {
                errors.Add(ValidationMessages.GenerateErrorFileSizeMustNotBeZero(dataFileName));
            }

            if (metaFileSize == 0)
            {
                errors.Add(ValidationMessages.GenerateErrorFileSizeMustNotBeZero(metaFileName));
            }

            return errors;
        }

        private async Task<List<ErrorViewModel>> ValidateDataFileTypes(
            string dataFileName,
            Stream dataFileStream,
            string metaFileName,
            Stream metaFileStream)
        {
            List<ErrorViewModel> errors = [];

            if (!await _fileTypeService.IsValidCsvFile(dataFileStream))
            {
                errors.Add(ValidationMessages.GenerateErrorMustBeCsvFile(dataFileName));
            }

            if (!await _fileTypeService.IsValidCsvFile(metaFileStream))
            {
                errors.Add(ValidationMessages.GenerateErrorMustBeCsvFile(metaFileName));
            }

            return errors;
        }
    }
}
