using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Storage.Blob;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationErrorMessages;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.FileStorageUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.FileStoragePathUtils;
using FileInfo = GovUk.Education.ExploreEducationStatistics.Common.Model.FileInfo;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services
{
    public class FileStorageService : IFileStorageService
    {
        private readonly string _storageConnectionString;

        private readonly IPersistenceHelper<ContentDbContext> _persistenceHelper;
        private readonly IUserService _userService;
        private readonly ContentDbContext _context;
        private readonly IImportService _importService;
        private readonly IFileUploadsValidatorService _fileUploadsValidatorService;
        
        private const string ContainerName = "releases";
        private const string NameKey = "name";

        public FileStorageService(IConfiguration config, IUserService userService, 
            IPersistenceHelper<ContentDbContext> persistenceHelper, ContentDbContext context,
            IImportService importService, IFileUploadsValidatorService fileUploadsValidatorService)
        {
            _storageConnectionString = config.GetValue<string>("CoreStorage");
            _userService = userService;
            _persistenceHelper = persistenceHelper;
            _context = context;
            _importService = importService;
            _fileUploadsValidatorService = fileUploadsValidatorService;
        }

        public async Task<Either<ActionResult, IEnumerable<FileInfo>>> ListPublicFilesPreview(Guid releaseId)
        {
            return await _persistenceHelper
                .CheckEntityExists<Release>(releaseId)
                .OnSuccess(_userService.CheckCanViewRelease)
                .OnSuccess(release => 
                    FileStorageUtils.ListPublicFilesPreview(_storageConnectionString, ContainerName, releaseId));
        }

        public async Task<Either<ActionResult, IEnumerable<Models.FileInfo>>> UploadDataFilesAsync(Guid releaseId,
            IFormFile dataFile, IFormFile metadataFile, string name, bool overwrite, string userName)
        {
            return await _persistenceHelper
                .CheckEntityExists<Release>(releaseId)
                .OnSuccess(_userService.CheckCanUpdateRelease)
                .OnSuccess(async release =>
                {
                    var blobContainer = await GetCloudBlobContainer();
                    var dataInfo = new Dictionary<string, string>
                        {{NameKey, name}, {MetaFileKey, metadataFile.FileName}, {UserName, userName}};
                    var metaDataInfo = new Dictionary<string, string> {{DataFileKey, dataFile.FileName}, {UserName, userName}};
                    return await _fileUploadsValidatorService.ValidateDataFilesForUpload(blobContainer, releaseId, dataFile, metadataFile, name, overwrite)
                        .OnSuccess(() => _importService.CreateImportTableRow(releaseId, dataFile.FileName))
                        .OnSuccess(() => _fileUploadsValidatorService.ValidateFileForUpload(blobContainer, releaseId,
                            dataFile,ReleaseFileTypes.Data, overwrite))
                        .OnSuccess(() => 
                            UploadFileAsync(blobContainer, releaseId, dataFile, ReleaseFileTypes.Data, dataInfo))
                        .OnSuccess(() => _fileUploadsValidatorService.ValidateFileForUpload(blobContainer, releaseId,
                            metadataFile, ReleaseFileTypes.Data, overwrite))
                        .OnSuccess(() => UploadFileAsync(blobContainer, releaseId, metadataFile, ReleaseFileTypes.Data,
                            metaDataInfo))
                        // add message to queue to process these files
                        .OnSuccessDo(() => _importService.Import(dataFile.FileName, releaseId, dataFile))
                        .OnSuccess(() => CreateBasicFileLink(dataFile.FileName, releaseId))
                        .OnSuccess(() => ListFilesAsync(releaseId, ReleaseFileTypes.Data));
                });
        }

        public Task<Either<ActionResult, IEnumerable<Models.FileInfo>>> DeleteDataFileAsync(Guid releaseId,
            string fileName)
        {
            return _persistenceHelper
                .CheckEntityExists<Release>(releaseId)
                .OnSuccess(_userService.CheckCanUpdateRelease)
                .OnSuccess(GetCloudBlobContainer)
                .OnSuccess(blobContainer => 
                    DataPathsForDeletion(blobContainer, releaseId, fileName)
                    .OnSuccess(paths => DeleteDataFilesAsync(blobContainer, paths)))
                .OnSuccess(() => DeleteFileLink(fileName, releaseId))
                .OnSuccess(() => ListFilesAsync(releaseId, ReleaseFileTypes.Data));
        }

        public Task<Either<ActionResult, IEnumerable<Models.FileInfo>>> UploadFilesAsync(Guid releaseId,
            IFormFile file, string name, ReleaseFileTypes type, bool overwrite)
        {
            return _persistenceHelper
                .CheckEntityExists<Release>(releaseId)
                .OnSuccess(_userService.CheckCanUpdateRelease)
                .OnSuccessDo(() => _fileUploadsValidatorService.ValidateUploadFileType(file, type))
                .OnSuccess(async release =>
                {
                    var blobContainer = await GetCloudBlobContainer();
                    var info = new Dictionary<string, string> {{NameKey, name}};
                    return await _fileUploadsValidatorService.ValidateFileForUpload(blobContainer, releaseId, file, type, overwrite)
                        .OnSuccess(() => UploadFileAsync(blobContainer, releaseId, file, type, info))
                        .OnSuccess(() => ListFilesAsync(releaseId, type));
                });
        }

        public Task<Either<ActionResult, IEnumerable<Models.FileInfo>>> DeleteFileAsync(Guid releaseId,
            ReleaseFileTypes type, string fileName)
        {
            return _persistenceHelper
                .CheckEntityExists<Release>(releaseId)
                .OnSuccess(_userService.CheckCanUpdateRelease)
                .OnSuccess(async release =>
                {
                    if (type == ReleaseFileTypes.Data)
                    {
                        return ValidationActionResult(CannotUseGenericFunctionToDeleteDataFile);
                    }

                    return await DeleteFileAsync(await GetCloudBlobContainer(),
                            AdminReleasePath(releaseId, type, fileName))
                        .OnSuccess(() => ListFilesAsync(releaseId, type));
                });
        }

        public async Task<Either<ActionResult, IEnumerable<Models.FileInfo>>> ListFilesAsync(Guid releaseId, ReleaseFileTypes type)
        {
            return await _persistenceHelper
                .CheckEntityExists<Release>(releaseId)
                .OnSuccess(_userService.CheckCanViewRelease)
                .OnSuccess(async _ =>
                {
                    var blobContainer = await GetCloudBlobContainer();

                    IEnumerable<Models.FileInfo> files = blobContainer
                        .ListBlobs(AdminReleaseDirectoryPath(releaseId, type), true, BlobListingDetails.Metadata)
                        .Where(blob => !IsBatchedDataFile(blob, releaseId))
                        .OfType<CloudBlockBlob>()
                        .Select(file => new Models.FileInfo
                        {
                            Extension = GetExtension(file),
                            Name = GetName(file),
                            Path = file.Name,
                            Size = GetSize(file),
                            MetaFileName = GetMetaFileName(file),
                            Rows = GetNumberOfRows(file),
                            UserName = GetUserName(file),
                            Created = file.Properties.Created
                        })
                        .OrderBy(info => info.Name);

                    return files;
                });
        }

        public Task<Either<ActionResult, Release>> CopyReleaseFilesAsync(Guid originalReleaseId, Guid newReleaseId, ReleaseFileTypes type)
        {
            return _persistenceHelper
                .CheckEntityExists<Release>(originalReleaseId)
                .OnSuccess(_userService.CheckCanViewRelease)
                .OnSuccess(_ => _persistenceHelper.CheckEntityExists<Release>(newReleaseId))
                .OnSuccess(_userService.CheckCanViewRelease)
                .OnSuccess(async newRelease =>
                {
                    var blobContainer = await GetCloudBlobContainer();

                    var originalBlobs = blobContainer
                        .ListBlobs(AdminReleaseDirectoryPath(originalReleaseId, type), true, BlobListingDetails.Metadata)
                        .Where(blob => !IsBatchedDataFile(blob, originalReleaseId))
                        .OfType<CloudBlockBlob>()
                        .ToList();

                    var copyJobs = originalBlobs
                        .Select(blob =>
                        {
                            var originalFilename = blob.Name.Substring(blob.Name.LastIndexOf('/') + 1);
                            var blobCopy = blobContainer
                                .GetBlockBlobReference(AdminReleasePath(newReleaseId, type, originalFilename));
                            return blobCopy.StartCopyAsync(blob);
                        });

                    await Task.WhenAll(copyJobs);
                    
                    return newRelease;
                });
        }

        public async Task<Either<ActionResult, FileStreamResult>> StreamFile(Guid releaseId,
            ReleaseFileTypes type, string fileName)
        {
            return await _persistenceHelper
                .CheckEntityExists<Release>(releaseId)
                .OnSuccess(_userService.CheckCanViewRelease)
                .OnSuccess(async _ =>
                {
                    var blobContainer = await GetCloudBlobContainer();
                    var blob = blobContainer.GetBlockBlobReference(AdminReleasePath(releaseId, type, fileName));

                    if (!blob.Exists())
                    {
                        return ValidationActionResult<FileStreamResult>(FileNotFound);
                    }

                    var stream = new MemoryStream();

                    await blob.DownloadToStreamAsync(stream);
                    stream.Seek(0, SeekOrigin.Begin);
                    return new FileStreamResult(stream, blob.Properties.ContentType)
                    {
                        FileDownloadName = fileName
                    };
                });
        }

        private async static Task<Either<ActionResult, (string dataFilePath, string metadataFilePath)>>
            DataPathsForDeletion(CloudBlobContainer blobContainer, Guid releaseId, string fileName)
        {
            var dataFilePath = AdminReleasePath(releaseId, ReleaseFileTypes.Data, fileName);
            var dataBlob = blobContainer.GetBlockBlobReference(dataFilePath);
            if (!dataBlob.Exists())
            {
                return ValidationActionResult(FileNotFound);
            }

            dataBlob.FetchAttributes();
            if (!dataBlob.Metadata.ContainsKey(MetaFileKey))
            {
                return ValidationActionResult(UnableToFindMetadataFileToDelete);
            }

            var metaFileName = dataBlob.Metadata[MetaFileKey];
            var metadataFilePath = AdminReleasePath(releaseId, ReleaseFileTypes.Data, metaFileName);
            var metaBlob = blobContainer.GetBlockBlobReference(metadataFilePath);
            if (!metaBlob.Exists())
            {
                return ValidationActionResult(UnableToFindMetadataFileToDelete);
            }

            return (dataFilePath, metadataFilePath);
        }

        private async Task<Either<ActionResult, bool>> CreateBasicFileLink(string filename, Guid releaseId)
        {
            var fileLink = new ReleaseFile
            {
                Id = Guid.NewGuid(),
                ReleaseId = releaseId,
                ReleaseFileReference = new ReleaseFileReference
                {
                    Id = Guid.NewGuid(),
                    ReleaseId = releaseId,
                    Filename = filename
                }
            };

            _context.ReleaseFiles.Add(fileLink);
            await _context.SaveChangesAsync();
            return true;
        }

        private async Task<Either<ActionResult, bool>> DeleteFileLink(string filename, Guid releaseId)
        {
            var fileLink = await _context
                .ReleaseFiles
                .Include(f => f.ReleaseFileReference)
                .Where(f => f.ReleaseId == releaseId && f.ReleaseFileReference.Filename == filename)
                .FirstOrDefaultAsync();

            _context.ReleaseFiles.Remove(fileLink);
            await _context.SaveChangesAsync();
            return true;
        }

        private static async Task<Either<ActionResult, bool>> UploadFileAsync(CloudBlobContainer blobContainer,
            Guid releaseId, IFormFile file, ReleaseFileTypes type, IDictionary<string, string> metaValues)
        {
            var blob = blobContainer.GetBlockBlobReference(AdminReleasePath(releaseId, type, file.FileName));
            blob.Properties.ContentType = file.ContentType;
            var path = await UploadToTemporaryFile(file);
            await blob.UploadFromFileAsync(path);

            metaValues["NumberOfRows"] = CalculateNumberOfRows(file.OpenReadStream()).ToString();

            await AddMetaValuesAsync(blob, metaValues);
            return true;
        }

        private static async Task<Either<ActionResult, bool>> DeleteFileAsync(CloudBlobContainer blobContainer,
            string path)
        {
            var blob = blobContainer.GetBlockBlobReference(path);
            if (!blob.Exists())
            {
                return ValidationActionResult(FileNotFound);
            }

            await blob.DeleteAsync();
            return true;
        }

        private static async Task<Either<ActionResult, bool>> DeleteDataFilesAsync(CloudBlobContainer blobContainer,
            (string, string) paths)
        {
            await DeleteFileAsync(blobContainer, paths.Item1);
            await DeleteFileAsync(blobContainer, paths.Item2);
            return true;
        }
        
        private async Task<CloudBlobContainer> GetCloudBlobContainer()
        {
            return await GetCloudBlobContainerAsync(_storageConnectionString, ContainerName);
        }

        private static async Task<string> UploadToTemporaryFile(IFormFile file)
        {
            var path = Path.GetTempFileName();
            if (file.Length > 0)
            {
                using (var stream = new FileStream(path, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }
            }

            return path;
        }

        private static string GetMetaFileName(CloudBlob blob)
        {
            return blob.Metadata.TryGetValue(MetaFileKey, out var name) ? name : "";
        }

        private static int GetNumberOfRows(CloudBlob blob)
        {
            return
                blob.Metadata.TryGetValue(NumberOfRows, out var numberOfRows) &&
                int.TryParse(numberOfRows, out var numberOfRowsValue)
                    ? numberOfRowsValue
                    : 0;
        }

        private static string GetUserName(CloudBlob blob)
        {
            return blob.Metadata.TryGetValue(UserName, out var name) ? name : "";
        }

        private static async Task AddMetaValuesAsync(CloudBlob blob, IDictionary<string, string> values)
        {
            foreach (var (key, value) in values)
            {
                if (blob.Metadata.ContainsKey(key))
                {
                    blob.Metadata.Remove(key);
                }

                blob.Metadata.Add(key, value);
            }

            await blob.SetMetadataAsync();
        }
    }
}