using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Storage.Blob;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationErrorMessages;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationUtils;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.FileTypeValidationUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.BlobContainerNames;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.FileStoragePathUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services
{
    public class DataZipArchiveService : IDataZipArchiveService
    {
        private readonly IFileTypeService _fileTypeService;

        private static readonly Dictionary<ReleaseFileTypes, IEnumerable<Regex>> AllowedMimeTypesByFileType = 
            new Dictionary<ReleaseFileTypes, IEnumerable<Regex>>
            {
                { ReleaseFileTypes.DataZip, AllowedZipMimeTypes }
            };
        
        public DataZipArchiveService(IFileTypeService fileTypeService)
        {
            _fileTypeService = fileTypeService;
        }

        public async Task<Either<ActionResult, Tuple<ZipArchiveEntry, ZipArchiveEntry>>> GetArchiveEntries(
            CloudBlobContainer blobContainer, Guid releaseId, IFormFile zipFile)
        {
            if (!IsZipFile(zipFile))
            {
                return ValidationActionResult(DataFileMustBeZipFile);
            }
            
            var path = AdminReleasePath(releaseId, ReleaseFileTypes.Chart, zipFile.FileName);

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

        private bool IsZipFile(IFormFile file)
        {
            if (!file.FileName.ToLower().EndsWith(".zip"))
            {
                return false;
            }
            
            return _fileTypeService.HasMatchingMimeType(file, AllowedMimeTypesByFileType[ReleaseFileTypes.DataZip]) 
                   && _fileTypeService.HasMatchingEncodingType(file, ZipEncodingTypes);
        }
    }
}