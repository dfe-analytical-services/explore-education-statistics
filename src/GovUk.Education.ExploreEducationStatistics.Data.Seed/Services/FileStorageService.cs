using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using static GovUk.Education.ExploreEducationStatistics.Data.Seed.ValidationErrorMessages;
using static GovUk.Education.ExploreEducationStatistics.Data.Seed.ValidationUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.FileStorageUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.FileStoragePathUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.BlobContainerNames;

namespace GovUk.Education.ExploreEducationStatistics.Data.Seed.Services
{
    public class FileStorageService : IFileStorageService
    {
        private readonly IBlobStorageService _blobStorageService;

        public FileStorageService(IBlobStorageService blobStorageService)
        {
            _blobStorageService = blobStorageService;
        }

        public async Task<Either<ValidationResult, Either<ValidationResult, Unit>>> UploadDataFilesAsync(Guid releaseId,
            IFormFile dataFile, IFormFile metadataFile, string name, bool overwrite)
        {
            var dataInfo = GetDataFileMetaValues(
                name: name,
                metaFileName: metadataFile.Name,
                userName: string.Empty,
                numberOfRows: CalculateNumberOfRows(dataFile.OpenReadStream())
            );
            var metaDataInfo = GetMetaDataFileMetaValues(
                dataFileName: dataFile.FileName,
                userName: string.Empty,
                numberOfRows: CalculateNumberOfRows(metadataFile.OpenReadStream())
            );

            return await ValidateDataFilesForUpload(releaseId, dataFile, metadataFile, overwrite)
                .OnSuccess(() =>
                    UploadFileAsync(releaseId, dataFile, ReleaseFileTypes.Data, dataInfo, overwrite))
                .OnSuccess(() => UploadFileAsync(releaseId, metadataFile, ReleaseFileTypes.Data,
                    metaDataInfo, overwrite));
        }

        // We cannot rely on the normal upload validation as we want this to be an atomic operation for both files.
        private async Task<Either<ValidationResult, Unit>> ValidateDataFilesForUpload(
            Guid releaseId,
            IFormFile dataFile,
            IFormFile metaFile,
            bool overwrite)
        {
            if (string.Equals(dataFile.FileName, metaFile.FileName, StringComparison.OrdinalIgnoreCase))
            {
                return ValidationResult(DataAndMetadataFilesCannotHaveTheSameName);
            }

            if (dataFile.Length == 0)
            {
                return ValidationResult(DataFileCannotBeEmpty);
            }

            if (metaFile.Length == 0)
            {
                return ValidationResult(MetadataFileCannotBeEmpty);
            }

            var dataFilePath = AdminReleasePath(releaseId, ReleaseFileTypes.Data, dataFile.FileName);
            var metadataFilePath = AdminReleasePath(releaseId, ReleaseFileTypes.Data, metaFile.FileName);
            if (!overwrite && CheckBlobExists(dataFilePath))
            {
                return ValidationResult(CannotOverwriteDataFile);
            }

            if (!overwrite && CheckBlobExists(metadataFilePath))
            {
                return ValidationResult(CannotOverwriteMetadataFile);
            }

            return Unit.Instance;
        }

        private async Task<Either<ValidationResult, Unit>> UploadFileAsync(
            Guid releaseId,
            IFormFile file,
            ReleaseFileTypes type,
            IDictionary<string, string> metaValues,
            bool overwrite)
        {
            var path = AdminReleasePath(releaseId, type, file.FileName);
            if (!overwrite && CheckBlobExists(path))
            {
                return ValidationResult(CannotOverwriteFile);
            }

            // Check that it is not an empty file because this causes issues downstream
            if (file.Length == 0)
            {
                return ValidationResult(FileCannotBeEmpty);
            }

            await _blobStorageService.UploadFile(PrivateFilesContainerName, path, file, new IBlobStorageService.UploadFileOptions
            {
                MetaValues = metaValues
            });

            return Unit.Instance;
        }

        private bool CheckBlobExists(string path)
        {
            return _blobStorageService.CheckBlobExists(PrivateFilesContainerName, path).Result;
        }
    }
}