using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Storage.Blob;
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

        public FileUploadsValidatorService(ISubjectService subjectService, IFileTypeService fileTypeService)
        {
            _subjectService = subjectService;
            _fileTypeService = fileTypeService;
        }

        // We cannot rely on the normal upload validation as we want this to be an atomic operation for both files.
        public async Task<Either<ActionResult, bool>> ValidateDataFilesForUpload(CloudBlobContainer blobContainer,
            Guid releaseId, IFormFile dataFile, IFormFile metaFile, string name, bool overwrite)
        {
            if (string.Equals(dataFile.FileName, metaFile.FileName, OrdinalIgnoreCase))
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
            
            if (metaFile.FileName.IndexOf(" ", Ordinal) > -1 || 
                metaFile.FileName.IndexOfAny(Path.GetInvalidFileNameChars()) > -1)
            {
                return ValidationActionResult(MetaFilenameCannotContainSpacesOrSpecialCharacters);
            }

            var dataFilePath = AdminReleasePath(releaseId, ReleaseFileTypes.Data, dataFile.FileName);
            var metadataFilePath = AdminReleasePath(releaseId, ReleaseFileTypes.Data, metaFile.FileName);

            if (!IsCsvFile(dataFilePath, dataFile))
            {
                return ValidationActionResult(DataFileMustBeCsvFile);
            }

            if (!IsCsvFile(metadataFilePath, metaFile))
            {
                return ValidationActionResult(MetaFileMustBeCsvFile);
            }

            if (!overwrite && blobContainer.GetBlockBlobReference(dataFilePath).Exists())
            {
                return ValidationActionResult(CannotOverwriteDataFile);
            }

            if (!overwrite && blobContainer.GetBlockBlobReference(metadataFilePath).Exists())
            {
                return ValidationActionResult(CannotOverwriteMetadataFile);
            }

            if (_subjectService.Exists(releaseId, name))
            {
                return ValidationActionResult(SubjectTitleMustBeUnique);
            }

            return true;
        }

        public async Task<Either<ActionResult, bool>> ValidateFileForUpload(CloudBlobContainer blobContainer,
            Guid releaseId, IFormFile file, ReleaseFileTypes type, bool overwrite)
        {
            var blob = blobContainer.GetBlockBlobReference(AdminReleasePath(releaseId, type, file.FileName));
            
            if (!overwrite && blob.Exists())
            {
                return ValidationActionResult(CannotOverwriteFile);
            }

            // Check that it is not an empty file because this causes issues downstream
            if (file.Length == 0)
            {
                return ValidationActionResult(FileCannotBeEmpty);
            }
            
            if (file.FileName.IndexOf(" ", Ordinal) > -1 || 
                file.FileName.IndexOfAny(Path.GetInvalidFileNameChars()) > -1)
            {
                return ValidationActionResult(FilenameCannotContainSpacesOrSpecialCharacters);
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
        
        public static readonly Dictionary<ReleaseFileTypes, IEnumerable<Regex>> AllowedMimeTypesByFileType = 
            new Dictionary<ReleaseFileTypes, IEnumerable<Regex>>
            {
                { ReleaseFileTypes.Ancillary, AllowedAncillaryFileTypes },
                { ReleaseFileTypes.Chart, AllowedChartFileTypes },
                { ReleaseFileTypes.Data, AllowedCsvMimeTypes }
            };
    }
}