#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Methodologies;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static GovUk.Education.ExploreEducationStatistics.Common.BlobContainers;
using static GovUk.Education.ExploreEducationStatistics.Common.Model.FileType;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Methodologies
{
    public class MethodologyImageService : IMethodologyImageService
    {
        private readonly ContentDbContext _contentDbContext;
        private readonly IPersistenceHelper<ContentDbContext> _persistenceHelper;
        private readonly IBlobStorageService _blobStorageService;
        private readonly IFileUploadsValidatorService _fileUploadsValidatorService;
        private readonly IFileRepository _fileRepository;
        private readonly IMethodologyFileRepository _methodologyFileRepository;
        private readonly IUserService _userService;

        public MethodologyImageService(ContentDbContext contentDbContext,
            IPersistenceHelper<ContentDbContext> persistenceHelper,
            IBlobStorageService blobStorageService,
            IFileUploadsValidatorService fileUploadsValidatorService,
            IFileRepository fileRepository,
            IMethodologyFileRepository methodologyFileRepository,
            IUserService userService)
        {
            _contentDbContext = contentDbContext;
            _persistenceHelper = persistenceHelper;
            _blobStorageService = blobStorageService;
            _fileUploadsValidatorService = fileUploadsValidatorService;
            _fileRepository = fileRepository;
            _methodologyFileRepository = methodologyFileRepository;
            _userService = userService;
        }

        public async Task<Either<ActionResult, Unit>> DeleteAll(Guid methodologyVersionId,
            bool forceDelete = false)
        {
            var methodologyFiles = await _methodologyFileRepository.GetByFileType(methodologyVersionId, Image);

            return await Delete(methodologyVersionId,
                methodologyFiles.Select(methodologyFile => methodologyFile.FileId),
                forceDelete);
        }

        public async Task<Either<ActionResult, Unit>> Delete(Guid methodologyVersionId,
            IEnumerable<Guid> fileIds,
            bool forceDelete = false)
        {
            return await _persistenceHelper
                .CheckEntityExists<MethodologyVersion>(methodologyVersionId)
                .OnSuccess(async release => await _userService.CheckCanUpdateMethodologyVersion(release, forceDelete))
                .OnSuccess(async release =>
                    await fileIds.Select(fileId =>
                        _methodologyFileRepository.CheckFileExists(methodologyVersionId, fileId, Image)).OnSuccessAll())
                .OnSuccessVoid(async files =>
                {
                    await files
                        .ToAsyncEnumerable()
                        .ForEachAwaitAsync(async file =>
                        {
                            var methodologyLinks = await _methodologyFileRepository.GetByFile(file.Id);

                            await _methodologyFileRepository.Delete(methodologyVersionId, file.Id);

                            // If this methodology version is the only version that is referencing this Blob and File, it
                            // can be deleted from Blob Storage and the File table. Otherwise preserve them for the other
                            // versions that are still using them.
                            if (methodologyLinks.Count == 1 &&
                                methodologyLinks[0].MethodologyVersionId == methodologyVersionId)
                            {
                                await _blobStorageService.DeleteBlob(PrivateMethodologyFiles, file.Path());
                                await _fileRepository.Delete(file.Id);
                            }
                        });
                });
        }

        public async Task<Either<ActionResult, FileStreamResult>> Stream(Guid methodologyVersionId, Guid fileId)
        {
            return await _persistenceHelper
                .CheckEntityExists<MethodologyFile>(q => q
                    .Include(mf => mf.File)
                    .Where(mf => mf.MethodologyVersionId == methodologyVersionId && mf.FileId == fileId))
                .OnSuccessCombineWith(mf =>
                    _blobStorageService.DownloadToStream(PrivateMethodologyFiles, mf.Path(), new MemoryStream()))
                .OnSuccess(methodologyFileAndStream =>
                {
                    var (methodologyFile, stream) = methodologyFileAndStream;
                    return new FileStreamResult(stream, methodologyFile.File.ContentType)
                    {
                        FileDownloadName = methodologyFile.File.Filename
                    };
                });
        }

        public Task<Either<ActionResult, ImageFileViewModel>> Upload(Guid methodologyVersionId, IFormFile formFile)
        {
            return _persistenceHelper
                .CheckEntityExists<MethodologyVersion>(methodologyVersionId)
                .OnSuccess(_userService.CheckCanUpdateMethodologyVersion)
                .OnSuccess(async () => await _fileUploadsValidatorService.ValidateFileForUpload(formFile, Image))
                .OnSuccess(async () => await Upload(
                    methodologyVersionId,
                    Image,
                    formFile))
                .OnSuccess(methodologyFile => new ImageFileViewModel($"/api/methodologies/{methodologyVersionId}/images/{methodologyFile.File.Id}")
                {
                    // TODO EES-1922 Add support for resizing the image
                });
        }

        private async Task<Either<ActionResult, MethodologyFile>> Upload(Guid methodologyVersionId,
            FileType type,
            IFormFile formFile)
        {
            var methodologyFile = await _methodologyFileRepository.Create(
                methodologyVersionId: methodologyVersionId,
                filename: formFile.FileName,
                contentLength: formFile.Length,
                contentType: formFile.ContentType,
                type: type,
                createdById: _userService.GetUserId());

            await _contentDbContext.SaveChangesAsync();

            await _blobStorageService.UploadFile(
                containerName: PrivateMethodologyFiles,
                path: methodologyFile.Path(),
                file: formFile);

            return methodologyFile;
        }
    }
}
