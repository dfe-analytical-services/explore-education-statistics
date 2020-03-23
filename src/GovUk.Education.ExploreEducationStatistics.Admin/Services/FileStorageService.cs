using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Storage.Blob;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using static System.StringComparison;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationErrorMessages;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.FileStorageUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.FileStoragePathUtils;
using FileInfo = GovUk.Education.ExploreEducationStatistics.Common.Model.FileInfo;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services
{
    public class FileStorageService : IFileStorageService
    {
        public static readonly Regex[] AllowedCsvMimeTypes = {
            new Regex(@"^(application|text)/csv$"),
            new Regex(@"text/plain$")
        };
        
        private static readonly string[] CsvEncodingTypes = {
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
        
        private static readonly Dictionary<ReleaseFileTypes, IEnumerable<Regex>> AllowedMimeTypesByFileType = 
            new Dictionary<ReleaseFileTypes, IEnumerable<Regex>>
        {
            { ReleaseFileTypes.Ancillary, AllowedAncillaryFileTypes },
            { ReleaseFileTypes.Chart, AllowedChartFileTypes },
            { ReleaseFileTypes.Data, AllowedCsvMimeTypes }
        };
        
        private readonly string _storageConnectionString;

        private readonly ISubjectService _subjectService;
        private readonly IPersistenceHelper<ContentDbContext> _persistenceHelper;
        private readonly IUserService _userService;
        private readonly IFileTypeService _fileTypeService;
        private readonly ContentDbContext _context;

        private const string ContainerName = "releases";

        private const string NameKey = "name";

        public FileStorageService(IConfiguration config, ISubjectService subjectService, IUserService userService, 
            IPersistenceHelper<ContentDbContext> persistenceHelper, IFileTypeService fileTypeService, ContentDbContext context)
        {
            _storageConnectionString = config.GetValue<string>("CoreStorage");
            _subjectService = subjectService;
            _userService = userService;
            _persistenceHelper = persistenceHelper;
            _fileTypeService = fileTypeService;
            _context = context;
        }

        public async Task<Either<ActionResult, IEnumerable<FileInfo>>> ListPublicFilesPreview(Guid releaseId)
        {
            return await _persistenceHelper
                .CheckEntityExists<Release>(releaseId)
                .OnSuccess(_userService.CheckCanViewRelease)
                .OnSuccess(release => 
                    FileStorageUtils.ListPublicFilesPreview(_storageConnectionString, ContainerName, releaseId));
        }

        public Task<Either<ActionResult, IEnumerable<Models.FileInfo>>> UploadDataFilesAsync(Guid releaseId,
            IFormFile dataFile, IFormFile metadataFile, string name, bool overwrite, string userName)
        {
            return _persistenceHelper
                .CheckEntityExists<Release>(releaseId)
                .OnSuccess(_userService.CheckCanUpdateRelease)
                .OnSuccess(async release =>
                {
                    var blobContainer = await GetCloudBlobContainer();
                    var dataInfo = new Dictionary<string, string>
                        {{NameKey, name}, {MetaFileKey, metadataFile.FileName}, {UserName, userName}};
                    var metaDataInfo = new Dictionary<string, string> {{DataFileKey, dataFile.FileName}, {UserName, userName}};
                    return await 
                        ValidateDataFilesForUpload(blobContainer, releaseId, dataFile, metadataFile, name, overwrite)
                        .OnSuccess(() => UploadFileAsync(blobContainer, releaseId, dataFile, ReleaseFileTypes.Data, dataInfo, overwrite))
                        .OnSuccess(() => CreateBasicFileLink(dataFile.FileName, releaseId, ReleaseFileTypes.Data))
                        .OnSuccess(() => UploadFileAsync(blobContainer, releaseId, metadataFile, ReleaseFileTypes.Metadata,
                            metaDataInfo, overwrite))
                        .OnSuccess(() => CreateBasicFileLink(metadataFile.FileName, releaseId, ReleaseFileTypes.Metadata))
                        .OnSuccess(() => ListFilesAsync(releaseId, ReleaseFileTypes.Data, ReleaseFileTypes.Metadata));
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
                .OnSuccess(() => DeleteFileLink(releaseId, fileName, ReleaseFileTypes.Data))
                .OnSuccess(() => ListFilesAsync(releaseId, ReleaseFileTypes.Data));
        }

        public Task<Either<ActionResult, IEnumerable<Models.FileInfo>>> UploadFilesAsync(Guid releaseId,
            IFormFile file, string name, ReleaseFileTypes type, bool overwrite)
        {
            return _persistenceHelper
                .CheckEntityExists<Release>(releaseId)
                .OnSuccess(_userService.CheckCanUpdateRelease)
                .OnSuccessDo(() => ValidateUploadFileType(file, AllowedMimeTypesByFileType[type]))
                .OnSuccess(async release =>
                {
                    if (type == ReleaseFileTypes.Data)
                    {
                        return ValidationActionResult(CannotUseGenericFunctionToAddDataFile);
                    }

                    var blobContainer = await GetCloudBlobContainer();
                    var info = new Dictionary<string, string> {{NameKey, name}};
                    return await 
                        UploadFileAsync(blobContainer, releaseId, file, type, info, overwrite)
                        .OnSuccess(() => CreateBasicFileLink(name, releaseId, type))
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
                        .OnSuccess(() => DeleteFileLink(releaseId, fileName, type))
                        .OnSuccess(() => ListFilesAsync(releaseId, type));
                });
        }

        public async Task<Either<ActionResult, IEnumerable<Models.FileInfo>>> ListFilesAsync(Guid releaseId, params ReleaseFileTypes[] types)
        {
            return await _persistenceHelper
                .CheckEntityExists<Release>(releaseId)
                .OnSuccess(_userService.CheckCanViewRelease)
                .OnSuccess(async _ =>
                {
                    var blobContainer = await GetCloudBlobContainer();

                    var files = _context
                        .ReleaseFiles
                        .Include(f => f.ReleaseFileReference)
                        .Where(f => f.ReleaseId == releaseId && types.Contains(f.ReleaseFileReference.ReleaseFileType))
                        .ToList();

                    var filesWithMetadata = files
                        .Select(async fileLink =>
                        {
                            var fileReference = fileLink.ReleaseFileReference;
                            var blobPath = AdminReleasePathWithFileReference(fileReference);
                            var file = blobContainer.GetBlockBlobReference(blobPath);
                            await file.FetchAttributesAsync();
                            return new Models.FileInfo
                            {
                                Extension = GetExtension(file),
                                Name = GetName(file),
                                Path = file.Name,
                                Size = GetSize(file),
                                MetaFileName = GetMetaFileName(file),
                                Rows = GetNumberOfRows(file),
                                UserName = GetUserName(file),
                                Created = file.Properties.Created
                            };
                        });

                    return (await Task.WhenAll(filesWithMetadata))
                        .OrderBy(file => file.Name)
                        .AsEnumerable();
                });
        }

        // TODO BAU-405 - temporary helper method to list the content of folders directly from Blob storage
        public async Task<IEnumerable<Models.FileInfo>> ListFilesFromBlobStorage(Guid releaseId, ReleaseFileTypes type)
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
        }

        private string AdminReleasePathWithFileReference(ReleaseFileReference fileReference)
        {
            return AdminReleasePath(fileReference.ReleaseId, fileReference.ReleaseFileType, fileReference.Filename);
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
                    var blob = blobContainer.GetBlockBlobReference(AdminReleasePath(releaseId, type == ReleaseFileTypes.Metadata ? ReleaseFileTypes.Data : type, fileName));

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

        // We cannot rely on the normal upload validation as we want this to be an atomic operation for both files.
        private async Task<Either<ActionResult, bool>> ValidateDataFilesForUpload(CloudBlobContainer blobContainer,
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
        
        // We cannot rely on the normal upload validation as we want this to be an atomic operation for both files.
        private async Task<Either<ActionResult, bool>> ValidateUploadFileType(
            IFormFile file, IEnumerable<Regex> allowedMimeTypes)
        {
            if (!_fileTypeService.HasMatchingMimeType(file, allowedMimeTypes))
            {
                return ValidationActionResult(FileTypeInvalid);
            }

            return true;
        }

        private async Task<Either<ActionResult, bool>> CreateBasicFileLink(string filename, Guid releaseId, ReleaseFileTypes type)
        {
            var fileLink = new ReleaseFile
            {
                Id = Guid.NewGuid(),
                ReleaseId = releaseId,
                ReleaseFileReference = new ReleaseFileReference
                {
                    Id = Guid.NewGuid(),
                    ReleaseId = releaseId,
                    Filename = filename,
                    ReleaseFileType = type
                }
            };

            _context.ReleaseFiles.Add(fileLink);
            await _context.SaveChangesAsync();
            return true;
        }

        private async Task<Either<ActionResult, bool>> DeleteFileLink(Guid releaseId, string filename, ReleaseFileTypes type)
        {
            var fileLink = await _context
                .ReleaseFiles
                .Include(f => f.ReleaseFileReference)
                .Where(f => 
                    f.ReleaseId == releaseId 
                    && f.ReleaseFileReference.ReleaseFileType == type 
                    && f.ReleaseFileReference.Filename == filename)
                .FirstOrDefaultAsync();

            _context.ReleaseFiles.Remove(fileLink);
            await _context.SaveChangesAsync();
            return true;
        }

        private static async Task<Either<ActionResult, bool>> UploadFileAsync(CloudBlobContainer blobContainer,
            Guid releaseId, IFormFile file, ReleaseFileTypes type, IDictionary<string, string> metaValues,
            bool overwrite)
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

        private bool IsCsvFile(string filePath, IFormFile file)
        {
            if (!filePath.EndsWith(".csv"))
            {
                return false;
            }
            
            return _fileTypeService.HasMatchingMimeType(file, AllowedMimeTypesByFileType[ReleaseFileTypes.Data]) 
                   && _fileTypeService.HasMatchingEncodingType(file, CsvEncodingTypes);
        }
    }
}