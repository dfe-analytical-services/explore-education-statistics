using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Repository.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationErrorMessages;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Validators.FileTypeValidationUtils;
using static System.StringComparison;
using static GovUk.Education.ExploreEducationStatistics.Common.Model.FileType;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services
{
    public class FileUploadsValidatorService : IFileUploadsValidatorService
    {
        private readonly ISubjectRepository _subjectRepository;
        private readonly IFileTypeService _fileTypeService;
        private readonly ContentDbContext _context;

        public FileUploadsValidatorService(ISubjectRepository subjectRepository, IFileTypeService fileTypeService, ContentDbContext context)
        {
            _subjectRepository = subjectRepository;
            _fileTypeService = fileTypeService;
            _context = context;
        }

        // We cannot rely on the normal upload validation as we want this to be an atomic operation for both files.
        public async Task<Either<ActionResult, Unit>> ValidateDataFilesForUpload(Guid releaseId,
            IFormFile dataFile,
            IFormFile metaFile)
        {
            return await ValidateDataFileNames(releaseId, dataFile.FileName, metaFile.FileName)
                .OnSuccess(async _ => await ValidateDataFileSizes(dataFile.Length, metaFile.Length))
                .OnSuccess(async _ => await ValidateDataFileTypes(dataFile, metaFile));
        }

        public async Task<Either<ActionResult, Unit>> ValidateDataArchiveEntriesForUpload(
            Guid releaseId,
            IDataArchiveFile archiveFile)
        {
            return await ValidateDataFileNames(releaseId, archiveFile.DataFileName, archiveFile.MetaFileName)
                .OnSuccess(async _ => await ValidateDataFileSizes(archiveFile.DataFileSize, archiveFile.MetaFileSize));
        }

        public async Task<Either<ActionResult, Unit>> ValidateFileForUpload(IFormFile file, FileType type)
        {
            if (type != Ancillary && type != Chart && type != Image)
            {
                throw new ArgumentException("Cannot use generic function to validate data file", nameof(type));
            }

            // Check that it is not an empty file because this causes issues downstream
            if (file.Length == 0)
            {
                return ValidationActionResult(FileCannotBeEmpty);
            }
            
            if (!await _fileTypeService.HasMatchingMimeType(file, AllowedMimeTypesByFileType[type]))
            {
                return ValidationActionResult(FileTypeInvalid);
            }

            return Unit.Instance;
        }

        public async Task<Either<ActionResult, Unit>> ValidateSubjectName(Guid releaseId, string name)
        {
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

            return Unit.Instance;
        }

        private async Task<bool> IsCsvFile(IFormFile file)
        {
            return await _fileTypeService.HasMatchingMimeType(file, AllowedMimeTypesByFileType[FileType.Data])
                   && _fileTypeService.HasMatchingEncodingType(file, CsvEncodingTypes);
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

        private async Task<Either<ActionResult, Unit>> ValidateDataFileNames(Guid releaseId, string dataFileName, string metaFileName)
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

            if (IsFileExisting(releaseId, FileType.Data, dataFileName))
            {
                return ValidationActionResult(CannotOverwriteDataFile);
            }

            if (IsFileExisting(releaseId, Metadata, metaFileName))
            {
                return ValidationActionResult(CannotOverwriteMetadataFile);
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
            if (!await IsCsvFile(dataFile))
            {
                return ValidationActionResult(DataFileMustBeCsvFile);
            }

            if (!await IsCsvFile(metaFile))
            {
                return ValidationActionResult(MetaFileMustBeCsvFile);
            }

            return Unit.Instance;
        }

        private static bool ValidateFileExtension(string fileName, string requiredExtension)
        {
            return fileName.EndsWith(requiredExtension);
        }
    }
}
