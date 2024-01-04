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

        // We cannot rely on the normal upload validation as we want this to be an atomic operation for both files.
        public async Task<Either<ActionResult, Unit>> ValidateDataFilesForUpload(
            Guid releaseId,
            IFormFile dataFile,
            IFormFile metaFile,
            File? replacingFile = null)
        {
            return await ValidateDataFileNames(releaseId,
                    dataFileName: dataFile.FileName,
                    metaFileName: metaFile.FileName,
                    replacingFile)
                .OnSuccess(async _ => await ValidateDataFileSizes(dataFile.Length, metaFile.Length))
                .OnSuccess(async _ => await ValidateDataFileTypes(dataFile, metaFile));
        }

        public async Task<Either<ActionResult, Unit>> ValidateDataArchiveEntriesForUpload(
            Guid releaseId,
            IDataArchiveFile archiveFile,
            File? replacingFile = null)
        {
            return await ValidateDataFileSizes(archiveFile.DataFileSize, archiveFile.MetaFileSize)
                .OnSuccess(async _ => await ValidateDataFileNames(releaseId,
                    dataFileName: archiveFile.DataFileName,
                    metaFileName: archiveFile.MetaFileName,
                    replacingFile));
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

        public async Task<Either<ActionResult, Unit>> ValidateSubjectName(Guid releaseId, string name)
        {
            if (!name.Any())
            {
                return ValidationActionResult(SubjectTitleCannotBeEmpty);
            }

            if (FileContainsSpecialChars(name))
            {
                return ValidationActionResult(SubjectTitleCannotContainSpecialCharacters);
            }

            var subjectNameExists = _context.ReleaseFiles
                .Include(rf => rf.File)
                .Any(rf =>
                    rf.ReleaseId == releaseId
                    && rf.File.Type == FileType.Data
                    && rf.Name == name);
            if (subjectNameExists)
            {
                return ValidationActionResult(SubjectTitleMustBeUnique);
            }

            return await Task.FromResult(Unit.Instance);
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

        private bool IsFileExisting(Guid releaseId, FileType type, string name)
        {
            return _context
                .ReleaseFiles
                .Include(rf => rf.File)
                .Where(rf => rf.ReleaseId == releaseId)
                .ToList()
                .Any(rf => String.Equals(rf.File.Filename, name, CurrentCultureIgnoreCase)
                           && rf.File.Type == type);
        }

        private async Task<Either<ActionResult, Unit>> ValidateDataFileNames(
            Guid releaseId,
            string dataFileName,
            string metaFileName,
            File? replacingFile)
        {
            if (string.Equals(dataFileName.ToLower(), metaFileName.ToLower(), OrdinalIgnoreCase))
            {
                return ValidationActionResult(DataAndMetadataFilesCannotHaveTheSameName);
            }

            if (FileContainsSpacesOrSpecialChars(dataFileName))
            {
                return ValidationActionResult(DataFilenameCannotContainSpacesOrSpecialCharacters);
            }

            if (FileContainsSpacesOrSpecialChars(metaFileName))
            {
                return ValidationActionResult(MetaFilenameCannotContainSpacesOrSpecialCharacters);
            }

            if (!metaFileName.ToLower().Contains(".meta."))
            {
                return ValidationActionResult(MetaFileIsIncorrectlyNamed);
            }

            if (!ValidateFileExtension(dataFileName, ".csv"))
            {
                return ValidationActionResult(DataFileMustBeCsvFile);
            }

            if (!ValidateFileExtension(metaFileName, ".csv"))
            {
                return ValidationActionResult(MetaFileMustBeCsvFile);
            }

            var filenamesValid = ValidateFilenameLengths(
                dataFileName.Length,
                metaFileName.Length);

            if (filenamesValid.IsLeft)
            {
                return filenamesValid.Left;
            }

            if (IsFileExisting(releaseId, FileType.Data, dataFileName) &&
                (replacingFile == null || replacingFile.Filename != dataFileName))
            {
                return ValidationActionResult(DataFilenameNotUnique);
            }

            return Unit.Instance;
        }

        private static async Task<Either<ActionResult, Unit>> ValidateDataFileSizes(long dataFileLength,
            long metaFileLength)
        {
            if (dataFileLength == 0)
            {
                return ValidationActionResult(DataFileCannotBeEmpty);
            }

            if (metaFileLength == 0)
            {
                return ValidationActionResult(MetadataFileCannotBeEmpty);
            }

            return Unit.Instance;
        }

        private async Task<Either<ActionResult, Unit>> ValidateDataFileTypes(IFormFile dataFile, IFormFile metaFile)
        {
            if (!await _fileTypeService.IsValidCsvFile(dataFile))
            {
                return ValidationActionResult(DataFileMustBeCsvFile);
            }

            if (!await _fileTypeService.IsValidCsvFile(metaFile))
            {
                return ValidationActionResult(MetaFileMustBeCsvFile);
            }

            return Unit.Instance;
        }

        private static bool ValidateFileExtension(string fileName, string requiredExtension)
        {
            return fileName.EndsWith(requiredExtension);
        }

        private static Either<ActionResult, Unit> ValidateFilenameLengths(
            int dataFilenameLength,
            int metaFilenameLength)
        {
            var errorMessages = new List<ValidationErrorMessages>();

            if (dataFilenameLength > MaxFilenameSize)
            {
                errorMessages.Add(DataFilenameTooLong);
            }

            if (metaFilenameLength > MaxFilenameSize)
            {
                errorMessages.Add(MetaFilenameTooLong);
            }

            if (errorMessages.Any()) {
                return ValidationActionResult(errorMessages);
            }

            return Unit.Instance;
        }
    }
}
