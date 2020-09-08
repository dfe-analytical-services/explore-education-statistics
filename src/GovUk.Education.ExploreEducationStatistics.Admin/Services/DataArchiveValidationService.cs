using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Storage.Blob;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationErrorMessages;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Validators.FileTypeValidationUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.FileStoragePathUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services
{
    public class DataArchiveValidationService : IDataArchiveValidationService
    {
        private readonly IFileTypeService _fileTypeService;
        
        private static readonly Dictionary<ReleaseFileTypes, IEnumerable<Regex>> AllowedMimeTypesByFileType = 
            new Dictionary<ReleaseFileTypes, IEnumerable<Regex>>
            {
                { ReleaseFileTypes.DataZip, AllowedArchiveMimeTypes }
            };
        
        public DataArchiveValidationService(IFileTypeService fileTypeService)
        {
            _fileTypeService = fileTypeService;
        }

        public async Task<Either<ActionResult, Tuple<ZipArchiveEntry, ZipArchiveEntry>>> ValidateDataArchiveFile(
            CloudBlobContainer blobContainer, Guid releaseId, IFormFile zipFile)
        {
            if (!await IsZipFile(zipFile))
            {
                return ValidationActionResult(DataFileMustBeZipFile);
            }
            
            var path = AdminReleasePath(releaseId, ReleaseFileTypes.DataZip, zipFile.FileName);

            if (blobContainer.GetBlockBlobReference(path).Exists())
            {
                return ValidationActionResult(DataZipFileAlreadyExists);
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

            return new Tuple<ZipArchiveEntry, ZipArchiveEntry>(dataFile, metaFile);
        }

        private async Task<bool> IsZipFile(IFormFile file)
        {
            if (!file.FileName.ToLower().EndsWith(".zip"))
            {
                return false;
            }
            
            return await _fileTypeService.HasMatchingMimeType(file, AllowedMimeTypesByFileType[ReleaseFileTypes.DataZip]) 
                   && _fileTypeService.HasMatchingEncodingType(file, ZipEncodingTypes);
        }
    }
}