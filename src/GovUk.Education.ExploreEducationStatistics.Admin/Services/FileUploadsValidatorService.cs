using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationErrorMessages;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.FileStoragePathUtils;
using static System.StringComparison;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services
{
    public class FileUploadsValidatorService : IFileUploadsValidatorService
    {
        private readonly ISubjectService _subjectService;
        private readonly IFileTypeService _fileTypeService;
        private readonly ContentDbContext _context;

        public static readonly Regex[] AllowedCsvMimeTypes = {
            new Regex(@"^(application|text)/csv$"),
            new Regex(@"text/plain$")
        };
        
        public static readonly Regex[] AllowedZipMimeTypes = {
            new Regex(@"^(application)/zip$")
        };
        
        public static readonly string[] CsvEncodingTypes = {
            "us-ascii",
            "utf-8"
        };
        
        public static readonly Regex[] AllowedChartFileTypes = {
            new Regex(@"^image/.*") 
        };
        
        public static readonly Regex[] AllowedAncillaryFileTypes = {
            new Regex(@"^image/.*"),
            new Regex(@"^(application|text)/csv$"),
            new Regex(@"^text/plain$"),
            new Regex(@"^application/pdf$"),
            new Regex(@"^application/msword$"),
            new Regex(@"^application/vnd.ms-excel$"),
            new Regex(@"^application/vnd.openxmlformats(.*)$"),
            new Regex(@"^application/vnd.oasis.opendocument(.*)$"),
            new Regex(@"^application/CDFV2$"), 
        };

        public FileUploadsValidatorService(ISubjectService subjectService, IFileTypeService fileTypeService, ContentDbContext context)
        {
            _subjectService = subjectService;
            _fileTypeService = fileTypeService;
            _context = context;
        }

        // We cannot rely on the normal upload validation as we want this to be an atomic operation for both files.
        public async Task<Either<ActionResult, bool>> ValidateDataFilesForUpload(Guid releaseId, IFormFile dataFile, IFormFile metaFile, string name)
        {
            return await ValidateDataFileNames(releaseId, dataFile.FileName, metaFile.FileName)
                .OnSuccess(async _ => await ValidateDataFileSizes(dataFile.Length, metaFile.Length))
                .OnSuccess(async _ => await ValidateSubjectName(releaseId, name))
                .OnSuccess(async _ => await ValidateDataFileTypes(releaseId, dataFile, metaFile));
        }

        public async Task<Either<ActionResult, bool>> ValidateZippedDataFileForUpload(Guid releaseId, IFormFile zipFile, string name)
        {
            if (!IsZipFile(zipFile))
            {
                return ValidationActionResult(DataFileMustBeZipFile);
            }

            using var stream = zipFile.OpenReadStream();
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

            return await ValidateDataFileNames(releaseId, dataFile.Name, metaFile.Name)
                .OnSuccess(async _ => await ValidateDataFileSizes(dataFile.Length, metaFile.Length))
                .OnSuccess(async _ => await ValidateSubjectName(releaseId, name));
        }

        public async Task<Either<ActionResult, bool>> ValidateFileForUpload(Guid releaseId, IFormFile file, ReleaseFileTypes type, bool overwrite)
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

            return true;
        }

        // We cannot rely on the normal upload validation as we want this to be an atomic operation for both files.
        public async Task<Either<ActionResult, bool>> ValidateUploadFileType(
            IFormFile file, ReleaseFileTypes type)
        {
            var allowedMimeTypes = AllowedMimeTypesByFileType[type];
            
            if (!_fileTypeService.HasMatchingMimeType(file, allowedMimeTypes))
            {
                return ValidationActionResult(FileTypeInvalid);
            }
            
            if (type == ReleaseFileTypes.Data)
            {
                return ValidationActionResult(CannotUseGenericFunctionToAddDataFile);
            }

            return true;
        }

        private bool IsCsvFile(string filePath, IFormFile file)
        {
            if (!filePath.EndsWith(".csv"))
            {
                return false;
            }
            
            return _fileTypeService.HasMatchingMimeType(file, AllowedMimeTypesByFileType[ReleaseFileTypes.Data]) 
                   && _fileTypeService.HasMatchingEncodingType(file, CsvEncodingTypes);
        }
        
        private bool IsZipFile(IFormFile file)
        {
            if (!file.FileName.ToLower().EndsWith(".zip"))
            {
                return false;
            }
            
            return _fileTypeService.HasMatchingMimeType(file, AllowedMimeTypesByFileType[ReleaseFileTypes.DataZip]) 
                   && _fileTypeService.HasMatchingEncodingType(file, CsvEncodingTypes);
        }
        
        private bool FileContainsSpacesOrSpecialChars(string filename)
        {
            return filename.IndexOf(" ", Ordinal) > -1 || 
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

        private async Task<Either<ActionResult, bool>> ValidateDataFileNames(Guid releaseId, string dataFileName, string metaFileName)
        {
            if (string.Equals(dataFileName.ToLower(), metaFileName.ToLower(), OrdinalIgnoreCase))
            {
                return ValidationActionResult(DataAndMetadataFilesCannotHaveTheSameName);
            }
            
            if (dataFileName.IndexOf(" ", Ordinal) > -1 || 
                dataFileName.IndexOfAny(Path.GetInvalidFileNameChars()) > -1)
            {
                return ValidationActionResult(DataFilenameCannotContainSpacesOrSpecialCharacters);
            }
            
            if (FileContainsSpacesOrSpecialChars(dataFileName))
            {
                return ValidationActionResult(FilenameCannotContainSpacesOrSpecialCharacters);
            }
            
            if (metaFileName.IndexOf(" ", Ordinal) > -1 || 
                metaFileName.IndexOfAny(Path.GetInvalidFileNameChars()) > -1)
            {
                return ValidationActionResult(MetaFilenameCannotContainSpacesOrSpecialCharacters);
            }

            if (FileContainsSpacesOrSpecialChars(metaFileName))
            {
                return ValidationActionResult(FilenameCannotContainSpacesOrSpecialCharacters);
            }
            
            if (!metaFileName.ToLower().Contains(".meta."))
            {
                return ValidationActionResult(MetaFileIsIncorrectlyNamed);
            }

            if (IsFileExisting(releaseId, ReleaseFileTypes.Data, dataFileName))
            {
                return ValidationActionResult(CannotOverwriteDataFile);
            }

            if (IsFileExisting(releaseId, ReleaseFileTypes.Metadata, metaFileName))
            {
                return ValidationActionResult(CannotOverwriteMetadataFile);
            }

            return true;
        }

        private async Task<Either<ActionResult, bool>> ValidateDataFileSizes(long dataFileLength, long metaFileLength)
        {
            if (dataFileLength == 0)
            {
                return ValidationActionResult(DataFileCannotBeEmpty);
            }

            if (metaFileLength == 0)
            {
                return ValidationActionResult(MetadataFileCannotBeEmpty);
            }

            return true;
        }
        
        private async Task<Either<ActionResult, bool>> ValidateDataFileTypes(Guid releaseId, IFormFile dataFile, IFormFile metaFile)
        {
            var dataFilePath = AdminReleasePath(releaseId, ReleaseFileTypes.Data, dataFile.FileName.ToLower());
            var metadataFilePath = AdminReleasePath(releaseId, ReleaseFileTypes.Data, metaFile.FileName.ToLower());

            if (!IsCsvFile(dataFilePath, dataFile))
            {
                return ValidationActionResult(DataFileMustBeCsvFile);
            }

            if (!IsCsvFile(metadataFilePath, metaFile))
            {
                return ValidationActionResult(MetaFileMustBeCsvFile);
            }

            return true;
        }
        
        private async Task<Either<ActionResult, bool>> ValidateSubjectName(Guid releaseId, string name)
        {
            if (await _subjectService.GetAsync(releaseId, name) != null)
            {
                return ValidationActionResult(SubjectTitleMustBeUnique);
            }

            return true;
        }

        public static readonly Dictionary<ReleaseFileTypes, IEnumerable<Regex>> AllowedMimeTypesByFileType = 
            new Dictionary<ReleaseFileTypes, IEnumerable<Regex>>
            {
                { ReleaseFileTypes.Ancillary, AllowedAncillaryFileTypes },
                { ReleaseFileTypes.Chart, AllowedChartFileTypes },
                { ReleaseFileTypes.Data, AllowedCsvMimeTypes }
            };
    }
}