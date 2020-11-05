using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Extensions;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.FileStoragePathUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.BlobContainerNames;
using static GovUk.Education.ExploreEducationStatistics.Common.Extensions.BlobInfoExtensions;
using static GovUk.Education.ExploreEducationStatistics.Common.Model.ReleaseFileTypes;
using FileInfo = GovUk.Education.ExploreEducationStatistics.Common.Model.FileInfo;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services
{
    public class ReleaseFileService : IReleaseFileService
    {
        private readonly ContentDbContext _contentDbContext;
        private readonly IPersistenceHelper<ContentDbContext> _persistenceHelper;
        private readonly IBlobStorageService _blobStorageService;
        private readonly IFileRepository _fileRepository;
        private readonly IFileUploadsValidatorService _fileUploadsValidatorService;
        private readonly IReleaseFileRepository _releaseFileRepository;
        private readonly IUserService _userService;

        public ReleaseFileService(ContentDbContext contentDbContext,
            IPersistenceHelper<ContentDbContext> persistenceHelper,
            IBlobStorageService blobStorageService,
            IFileRepository fileRepository,
            IFileUploadsValidatorService fileUploadsValidatorService,
            IReleaseFileRepository releaseFileRepository,
            IUserService userService)
        {
            _contentDbContext = contentDbContext;
            _persistenceHelper = persistenceHelper;
            _blobStorageService = blobStorageService;
            _fileRepository = fileRepository;
            _fileUploadsValidatorService = fileUploadsValidatorService;
            _releaseFileRepository = releaseFileRepository;
            _userService = userService;
        }

        public async Task<Either<ActionResult, Unit>> Delete(Guid releaseId,
            Guid id,
            bool forceDelete = false)
        {
            return await Delete(releaseId, new List<Guid>
            {
                id
            }, forceDelete: forceDelete);
        }

        public async Task<Either<ActionResult, Unit>> Delete(Guid releaseId,
            IEnumerable<Guid> ids,
            bool forceDelete = false)
        {
            return await _persistenceHelper
                .CheckEntityExists<Release>(releaseId)
                .OnSuccess(async release => await _userService.CheckCanUpdateRelease(release, ignoreCheck: forceDelete))
                .OnSuccess(async release =>
                    await ids.Select(id => _releaseFileRepository.CheckFileExists(releaseId, id, Ancillary, Chart))
                        .OnSuccessAll())
                .OnSuccessVoid(async files =>
                {
                    foreach (var file in files)
                    {
                        if (await _releaseFileRepository.FileIsLinkedToOtherReleases(releaseId, file.Id))
                        {
                            await _releaseFileRepository.Delete(releaseId, file.Id);
                        }
                        else
                        {
                            await _blobStorageService.DeleteBlob(
                                PrivateFilesContainerName,
                                file.Path());

                            await _fileRepository.Delete(file.Id);
                        }

                        await _contentDbContext.SaveChangesAsync();
                    }
                });
        }

        public async Task<Either<ActionResult, Unit>> DeleteAll(Guid releaseId, bool forceDelete = false)
        {
            var releaseFiles = await _releaseFileRepository.GetByFileType(releaseId,
                Ancillary,
                Chart);

            return await Delete(releaseId,
                releaseFiles.Select(releaseFile => releaseFile.ReleaseFileReference.Id),
                forceDelete: forceDelete);
        }

        public async Task<Either<ActionResult, IEnumerable<FileInfo>>> ListAll(Guid releaseId,
            params ReleaseFileTypes[] types)
        {
            return await _persistenceHelper
                .CheckEntityExists<Release>(releaseId)
                .OnSuccess(_userService.CheckCanViewRelease)
                .OnSuccess(async _ =>
                {
                    var files = await _releaseFileRepository.GetByFileType(releaseId, types);

                    var filesWithMetadata = files
                        .Select(async fileLink =>
                        {
                            var fileReference = fileLink.ReleaseFileReference;

                            // Files should exists in storage but if not then allow user to delete
                            var exists = await _blobStorageService.CheckBlobExists(
                                PrivateFilesContainerName,
                                fileReference.Path());

                            if (!exists)
                            {
                                return new FileInfo
                                {
                                    Id = fileReference.Id,
                                    Extension = fileReference.Extension,
                                    Name = "Unknown",
                                    Path = fileReference.Filename,
                                    Size = "0.00 B",
                                    Type = fileLink.ReleaseFileReference.ReleaseFileType
                                };
                            }

                            var blob = await _blobStorageService.GetBlob(PrivateFilesContainerName,
                                fileReference.Path());
                            return blob.ToFileInfo(fileReference.ReleaseFileType, fileReference.Id);
                        });

                    return (await Task.WhenAll(filesWithMetadata))
                        .OrderBy(file => file.Name)
                        .AsEnumerable();
                });
        }

        public async Task<Either<ActionResult, FileStreamResult>> Stream(Guid releaseId, Guid id)
        {
            return await _persistenceHelper
                .CheckEntityExists<Release>(releaseId)
                .OnSuccess(_userService.CheckCanViewRelease)
                .OnSuccess(release => _releaseFileRepository.CheckFileExists(releaseId, id))
                .OnSuccess(async file =>
                {
                    var path = file.Path();
                    var blob = await _blobStorageService.GetBlob(PrivateFilesContainerName, path);

                    var stream = new MemoryStream();
                    await _blobStorageService.DownloadToStream(PrivateFilesContainerName, path, stream);

                    return new FileStreamResult(stream, blob.ContentType)
                    {
                        FileDownloadName = file.Filename
                    };
                });
        }

        public Task<Either<ActionResult, FileInfo>> UploadAncillary(Guid releaseId, IFormFile file, string name)
        {
            return _persistenceHelper
                .CheckEntityExists<Release>(releaseId)
                .OnSuccess(_userService.CheckCanUpdateRelease)
                .OnSuccess(async () => await _fileUploadsValidatorService.ValidateUploadFileType(file, Ancillary))
                .OnSuccess(async () =>
                    await _fileUploadsValidatorService.ValidateFileForUpload(releaseId, file, Ancillary, false))
                .OnSuccess(async () => await _fileUploadsValidatorService.ValidateFileUploadName(name))
                .OnSuccess(async () =>
                {
                    var releaseFileReference = await _fileRepository.Create(
                        releaseId,
                        file.FileName.ToLower(),
                        Ancillary);

                    await _contentDbContext.SaveChangesAsync();

                    await _blobStorageService.UploadFile(
                        containerName: PrivateFilesContainerName,
                        path: releaseFileReference.Path(),
                        file: file,
                        options: new IBlobStorageService.UploadFileOptions
                        {
                            MetaValues = new Dictionary<string, string>
                            {
                                {
                                    NameKey, name
                                }
                            }
                        }
                    );

                    var blob = await _blobStorageService.GetBlob(
                        PrivateFilesContainerName,
                        releaseFileReference.Path());

                    return blob.ToFileInfo(Ancillary, releaseFileReference.Id);
                });
        }

        public Task<Either<ActionResult, FileInfo>> UploadChart(Guid releaseId,
            IFormFile file,
            Guid? replacingId = null)
        {
            return _persistenceHelper
                .CheckEntityExists<Release>(releaseId)
                .OnSuccess(_userService.CheckCanUpdateRelease)
                .OnSuccess(async () => await _fileUploadsValidatorService.ValidateUploadFileType(file, Chart))
                .OnSuccess(async () =>
                    await _fileUploadsValidatorService.ValidateFileForUpload(releaseId, file, Chart,
                        replacingId.HasValue))
                .OnSuccess(async () =>
                {
                    var releaseFileReference = replacingId.HasValue
                        ? await _fileRepository.UpdateFilename(
                            releaseId,
                            replacingId.Value,
                            file.FileName.ToLower())
                        : await _fileRepository.Create(
                            releaseId,
                            file.FileName.ToLower(),
                            Chart);

                    await _contentDbContext.SaveChangesAsync();

                    await _blobStorageService.UploadFile(
                        containerName: PrivateFilesContainerName,
                        path: releaseFileReference.Path(),
                        file: file,
                        options: new IBlobStorageService.UploadFileOptions
                        {
                            MetaValues = new Dictionary<string, string>
                            {
                                {
                                    NameKey, file.FileName.ToLower()
                                }
                            }
                        }
                    );

                    var blob = await _blobStorageService.GetBlob(
                        PrivateFilesContainerName,
                        releaseFileReference.Path());

                    return blob.ToFileInfo(Chart, releaseFileReference.Id);
                });
        }
    }
}