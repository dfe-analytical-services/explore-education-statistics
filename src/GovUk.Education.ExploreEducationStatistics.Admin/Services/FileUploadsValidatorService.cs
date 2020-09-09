using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationErrorMessages;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Validators.FileTypeValidationUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.FileStoragePathUtils;
using static System.StringComparison;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services
{
    public class FileUploadsValidatorService : IFileUploadsValidatorService
    {
        private readonly ISubjectService _subjectService;
        private readonly IFileTypeService _fileTypeService;
        private readonly ContentDbContext _context;

        public FileUploadsValidatorService(ISubjectService subjectService, IFileTypeService fileTypeService, ContentDbContext context)
        {
            _subjectService = subjectService;
            _fileTypeService = fileTypeService;
            _context = context;
        }

        // We cannot rely on the normal upload validation as we want this to be an atomic operation for both files.
        public async Task<Either<ActionResult, Unit>> ValidateDataFilesForUpload(Guid releaseId, IFormFile dataFile, IFormFile metaFile, string name)
        {
            return await ValidateDataFileNames(releaseId, dataFile.FileName, metaFile.FileName)
                .OnSuccess(async _ => await ValidateDataFileSizes(dataFile.Length, metaFile.Length))
                .OnSuccess(async _ => await ValidateSubjectName(releaseId, name))
                .OnSuccess(async _ => await ValidateDataFileTypes(releaseId, dataFile, metaFile));
        }

        public async Task<Either<ActionResult, Unit>> ValidateDataArchiveEntriesForUpload(Guid releaseId, ZipArchiveEntry dataFile, ZipArchiveEntry metaFile, string name)
        {
            return await ValidateDataFileNames(releaseId, dataFile.Name, metaFile.Name)
                .OnSuccess(async _ => await ValidateDataFileSizes(dataFile.Length, metaFile.Length))
                .OnSuccess(async _ => await ValidateSubjectName(releaseId, name));
        }

        public async Task<Either<ActionResult, Unit>> ValidateFileForUpload(Guid releaseId, IFormFile file,
            ReleaseFileTypes type, bool overwrite)
        {
            // Check that it is not an empty file because this causes issues downstream
            if (file.Length == 0)
            {
                return ValidationActionResult(FileCannotBeEmpty);
            }
            
            if (FileContainsSpacesOrSpecialChars(file.FileName))
            {
                return ValidationActionResult(FilenameCannotContainSpacesOrSpecialCharacters);
            }
            
            if (!overwrite && IsFileExisting(releaseId, type, file.FileName))
            {
                return ValidationActionResult(CannotOverwriteFile);
            }

            return Unit.Instance;
        }
        
        public async Task<Either<ActionResult, Unit>> ValidateFileUploadName(string name)
        {
            if (FileContainsSpecialChars(name))
            {
                return ValidationActionResult(FileUploadNameCannotContainSpecialCharacters);
            }

            return Unit.Instance;
        }

        // We cannot rely on the normal upload validation as we want this to be an atomic operation for both files.
        public async Task<Either<ActionResult, Unit>> ValidateUploadFileType(
            IFormFile file, ReleaseFileTypes type)
        {
            var allowedMimeTypes = AllowedMimeTypesByFileType[type];
            
            if (!await _fileTypeService.HasMatchingMimeType(file, allowedMimeTypes))
            {
                return ValidationActionResult(FileTypeInvalid);
            }
            
            if (type == ReleaseFileTypes.Data)
            {
                return ValidationActionResult(CannotUseGenericFunctionToAddDataFile);
            }

            return Unit.Instance;
        }

        private async Task<bool> IsCsvFile(string filePath, IFormFile file)
        {
            return await _fileTypeService.HasMatchingMimeType(file, AllowedMimeTypesByFileType[ReleaseFileTypes.Data]) 
                   && _fileTypeService.HasMatchingEncodingType(file, CsvEncodingTypes);
        }

        private bool FileContainsSpacesOrSpecialChars(string filename)
        {
            return filename.IndexOf(" ", Ordinal) > -1 ||
                   FileContainsSpecialChars(filename);
        }
        
        private bool FileContainsSpecialChars(string filename)
        {
            return filename.IndexOf("&", Ordinal) > -1 ||
                   filename.IndexOfAny(Path.GetInvalidFileNameChars()) > -1;
        }

        private bool IsFileExisting(Guid releaseId, ReleaseFileTypes type, string name)
        {
            return _context
                .ReleaseFiles
                .Include(rf => rf.ReleaseFileReference)
                .Where(rf => rf.ReleaseId == releaseId)
                .ToList()
                .Any(rf => String.Equals(rf.ReleaseFileReference.Filename, name, CurrentCultureIgnoreCase)
                && rf.ReleaseFileReference.ReleaseFileType == type);
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
            
            if (IsFileExisting(releaseId, ReleaseFileTypes.Data, dataFileName))
            {
                return ValidationActionResult(CannotOverwriteDataFile);
            }

            if (IsFileExisting(releaseId, ReleaseFileTypes.Metadata, metaFileName))
            {
                return ValidationActionResult(CannotOverwriteMetadataFile);
            }

            return Unit.Instance;
        }

        private async Task<Either<ActionResult, Unit>> ValidateDataFileSizes(long dataFileLength, long metaFileLength)
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
        
        private async Task<Either<ActionResult, Unit>> ValidateDataFileTypes(Guid releaseId, IFormFile dataFile, IFormFile metaFile)
        {
            var dataFilePath = AdminReleasePath(releaseId, ReleaseFileTypes.Data, dataFile.FileName.ToLower());
            var metadataFilePath = AdminReleasePath(releaseId, ReleaseFileTypes.Data, metaFile.FileName.ToLower());

            if (!await IsCsvFile(dataFilePath, dataFile))
            {
                return ValidationActionResult(DataFileMustBeCsvFile);
            }

            if (!await IsCsvFile(metadataFilePath, metaFile))
            {
                return ValidationActionResult(MetaFileMustBeCsvFile);
            }

            return Unit.Instance;
        }
        
        private bool ValidateFileExtension(string fileName, string requiredExtension)
        {
            return fileName.EndsWith(requiredExtension);
        }
        
        private async Task<Either<ActionResult, Unit>> ValidateSubjectName(Guid releaseId, string name)
        {
            if (FileContainsSpecialChars(name))
            {
                return ValidationActionResult(SubjectTitleCannotContainSpecialCharacters);
            }
            
            if (await _subjectService.GetAsync(releaseId, name) != null)
            {
                return ValidationActionResult(SubjectTitleMustBeUnique);
            }

            return Unit.Instance;
        }
    }
}