using System;
using System.Collections.Generic;
using System.IO;
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
            if (string.Equals(dataFile.FileName.ToLower(), metaFile.FileName.ToLower(), OrdinalIgnoreCase))
            {
                return ValidationActionResult(DataAndMetadataFilesCannotHaveTheSameName);
            }

            if (dataFile.Length == 0)
            {
                return ValidationActionResult(DataFileCannotBeEmpty);
            }

            if (metaFile.Length == 0)
            {
                return ValidationActionResult(MetadataFileCannotBeEmpty);
            }

            if (dataFile.FileName.IndexOf(" ", Ordinal) > -1 || 
                dataFile.FileName.IndexOfAny(Path.GetInvalidFileNameChars()) > -1)
            {
                return ValidationActionResult(DataFilenameCannotContainSpacesOrSpecialCharacters);
            }
            
            if (FileContainsSpacesOrSpecialChars(dataFile))
            {
                return ValidationActionResult(FilenameCannotContainSpacesOrSpecialCharacters);
            }
            
            if (metaFile.FileName.IndexOf(" ", Ordinal) > -1 || 
                metaFile.FileName.IndexOfAny(Path.GetInvalidFileNameChars()) > -1)
            {
                return ValidationActionResult(MetaFilenameCannotContainSpacesOrSpecialCharacters);
            }

            if (FileContainsSpacesOrSpecialChars(metaFile))
            {
                return ValidationActionResult(FilenameCannotContainSpacesOrSpecialCharacters);
            }

            if (IsFileExisting(releaseId, ReleaseFileTypes.Data, dataFile.FileName))
            {
                return ValidationActionResult(CannotOverwriteDataFile);
            }

            if (IsFileExisting(releaseId, ReleaseFileTypes.Metadata, metaFile.FileName))
            {
                return ValidationActionResult(CannotOverwriteMetadataFile);
            }

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

            if (await _subjectService.GetAsync(releaseId, name) != null)
            {
                return ValidationActionResult(SubjectTitleMustBeUnique);
            }

            return true;
        }

        public async Task<Either<ActionResult, bool>> ValidateFileForUpload(Guid releaseId, IFormFile file, ReleaseFileTypes type, bool overwrite)
        {
            // Check that it is not an empty file because this causes issues downstream
            if (file.Length == 0)
            {
                return ValidationActionResult(FileCannotBeEmpty);
            }
            
            if (FileContainsSpacesOrSpecialChars(file))
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
        
        private bool FileContainsSpacesOrSpecialChars(IFormFile file)
        {
            return file.FileName.IndexOf(" ", Ordinal) > -1 || 
                   file.FileName.IndexOfAny(Path.GetInvalidFileNameChars()) > -1;
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
        
        public static readonly Dictionary<ReleaseFileTypes, IEnumerable<Regex>> AllowedMimeTypesByFileType = 
            new Dictionary<ReleaseFileTypes, IEnumerable<Regex>>
            {
                { ReleaseFileTypes.Ancillary, AllowedAncillaryFileTypes },
                { ReleaseFileTypes.Chart, AllowedChartFileTypes },
                { ReleaseFileTypes.Data, AllowedCsvMimeTypes }
            };
    }
}