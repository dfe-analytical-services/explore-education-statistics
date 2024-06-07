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
using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services
{
    public class FileUploadsValidatorService : IFileUploadsValidatorService
    {
        private readonly IFileTypeService _fileTypeService;
        private readonly ContentDbContext _context;

        private const int MaxFilenameSize = 150;
        private const int MaxFileSize = int.MaxValue; // 2GB

        public FileUploadsValidatorService(IFileTypeService fileTypeService,
            ContentDbContext context)
        {
            _fileTypeService = fileTypeService;
            _context = context;
        }

        public async Task<List<ErrorViewModel>> ValidateDataFilesForUpload(
            Guid releaseVersionId,
            string dataSetName,
            string dataFileName,
            long dataFileSize,
            Func<Task<Stream>> dataFileStreamProvider,
            string metaFileName,
            long metaFileSize,
            Func<Task<Stream>> metaFileStreamProvider,
            File? replacingFile = null)
        {
            List<ErrorViewModel> errors = [];

            errors.AddRange(ValidateReleaseVersionDataSetFileName(
                releaseVersionId, dataSetName));

            errors.AddRange(ValidateDataFileNames(
                releaseVersionId,
                dataFileName: dataFileName,
                metaFileName: metaFileName,
                replacingFile));

            errors.AddRange(ValidateDataFileSizes(
                dataFileSize,
                dataFileName,
                metaFileSize,
                metaFileName));

            errors.AddRange(await ValidateDataFileTypes(
                dataFileName,
                dataFileStreamProvider,
                metaFileName,
                metaFileStreamProvider));

            return errors;
        }

        public async Task<List<ErrorViewModel>> ValidateDataFilesForUpload(
            Guid releaseVersionId,
            string dataSetFileName,
            IFormFile dataFile,
            IFormFile metaFile,
            File? replacingFile = null)
        {
            return await ValidateDataFilesForUpload(
                releaseVersionId,
                dataSetFileName,
                dataFile.FileName,
                dataFile.Length,
                () => Task.FromResult(dataFile.OpenReadStream()),
                metaFile.FileName,
                metaFile.Length,
                () => Task.FromResult(metaFile.OpenReadStream()),
                replacingFile);
        }

        public async Task<List<ErrorViewModel>> ValidateDataFilesForUpload(
            Guid releaseVersionId,
            ArchiveDataSetFile archiveDataSet,
            Func<Task<Stream>> dataFileStreamProvider,
            Func<Task<Stream>> metaFileStreamProvider,
            File? replacingFile = null)
        {
            return await ValidateDataFilesForUpload(
                releaseVersionId,
                archiveDataSet.DataSetName,
                archiveDataSet.DataFileName,
                archiveDataSet.DataFileSize,
                dataFileStreamProvider,
                archiveDataSet.MetaFileName,
                archiveDataSet.MetaFileSize,
                metaFileStreamProvider,
                replacingFile);
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

        public List<ErrorViewModel> ValidateReleaseVersionDataSetFileName(Guid releaseVersionId,
            string name)
        {
            List<ErrorViewModel> errors = [];

            if (!name.Any())
            {
                errors.Add(new ErrorViewModel
                {
                    Code = ValidationMessages.DataSetFileNameCannotBeEmpty.Code,
                    Message = ValidationMessages.DataSetFileNameCannotBeEmpty.Message,
                });
            }

            if (FileContainsSpecialChars(name))
            {
                errors.Add(ValidationMessages.GenerateErrorDataSetFileNameShouldNotContainSpecialCharacters(name));
            }

            var subjectNameExists = _context.ReleaseFiles
                .Include(rf => rf.File)
                .Any(rf =>
                    rf.ReleaseVersionId == releaseVersionId
                    && rf.File.Type == FileType.Data
                    && rf.Name == name);
            if (subjectNameExists)
            {
                errors.Add(ValidationMessages.GenerateErrorDataSetFileNamesShouldBeUnique(name));
            }

            return errors;
        }

        private static bool FileContainsSpacesOrSpecialChars(string filename)
        {
            return filename.IndexOf(" ", Ordinal) > -1 ||
                   FileContainsSpecialChars(filename);
        }

        private static bool FileContainsSpecialChars(string filename)
        {
            return filename.IndexOf("&", Ordinal) > -1 ||
                   filename.IndexOfAny(Path.GetInvalidFileNameChars()) > -1;
        }

        private bool IsFileExisting(Guid releaseVersionId,
            FileType type,
            string name)
        {
            return _context
                .ReleaseFiles
                .Include(rf => rf.File)
                .Where(rf => rf.ReleaseVersionId == releaseVersionId)
                .ToList()
                .Any(rf => String.Equals(rf.File.Filename, name, CurrentCultureIgnoreCase)
                           && rf.File.Type == type);
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

            if (FileContainsSpacesOrSpecialChars(dataFileName))
            {
                errors.Add(ValidationMessages.GenerateErrorFilenameCannotContainSpacesOrSpecialCharacters(
                    dataFileName));
            }

            if (FileContainsSpacesOrSpecialChars(metaFileName))
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
                errors.Add(ValidationMessages.GenerateErrorFileNameTooLong(
                    dataFileName, MaxFilenameSize));
            }

            if (metaFileName.Length > MaxFilenameSize)
            {
                errors.Add(ValidationMessages.GenerateErrorFileNameTooLong(
                    metaFileName, MaxFilenameSize));
            }

            if (IsFileExisting(releaseVersionId, FileType.Data, dataFileName) &&
                (replacingFile == null || replacingFile.Filename != dataFileName))
            {
                errors.Add(ValidationMessages.GenerateErrorDataFilenameNotUnique(dataFileName));
            }

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
            Func<Task<Stream>> dataFileStreamProvider,
            string metaFileName,
            Func<Task<Stream>> metaFileStreamProvider)
        {
            List<ErrorViewModel> errors = [];

            if (!await _fileTypeService.IsValidCsvFile(dataFileStreamProvider, dataFileName))
            {
                errors.Add(ValidationMessages.GenerateErrorMustBeCsvFile(dataFileName));
            }

            if (!await _fileTypeService.IsValidCsvFile(metaFileStreamProvider, metaFileName))
            {
                errors.Add(ValidationMessages.GenerateErrorMustBeCsvFile(metaFileName));
            }

            return errors;
        }
    }
}
