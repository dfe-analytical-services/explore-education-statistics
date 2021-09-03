using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Methodologies;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
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
using FileInfo = GovUk.Education.ExploreEducationStatistics.Common.Model.FileInfo;

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

        public async Task<Either<ActionResult, Unit>> Delete(Guid methodologyId, IEnumerable<Guid> fileIds)
        {
            return await _persistenceHelper
                .CheckEntityExists<MethodologyVersion>(methodologyId)
                .OnSuccess(async release => await _userService.CheckCanUpdateMethodology(release))
                .OnSuccess(async release =>
                    await fileIds.Select(fileId =>
                        _methodologyFileRepository.CheckFileExists(methodologyId, fileId, Image)).OnSuccessAll())
                .OnSuccessVoid(async files =>
                {
                    await files.ForEachAsync(async file =>
                    {
                        var methodologyLinks = await _methodologyFileRepository.GetByFile(file.Id);

                        await _methodologyFileRepository.Delete(methodologyId, file.Id);

                        // If this methodology version is the only version that is referencing this Blob and File, it
                        // can be deleted from Blob Storage and the File table. Otherwise preserve them for the other
                        // versions that are still using them. 
                        if (methodologyLinks.Count == 1 && methodologyLinks[0].MethodologyVersionId == methodologyId)
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
                .OnSuccess(async methodologyFile =>
                {
                    var path = methodologyFile.Path();
                    var blob = await _blobStorageService.GetBlob(PrivateMethodologyFiles, path);

                    var stream = new MemoryStream();
                    await _blobStorageService.DownloadToStream(PrivateMethodologyFiles, path, stream);

                    return new FileStreamResult(stream, blob.ContentType)
                    {
                        FileDownloadName = methodologyFile.File.Filename
                    };
                });
        }

        public Task<Either<ActionResult, ImageFileViewModel>> Upload(Guid methodologyVersionId, IFormFile formFile)
        {
            return _persistenceHelper
                .CheckEntityExists<MethodologyVersion>(methodologyVersionId)
                .OnSuccess(_userService.CheckCanUpdateMethodology)
                .OnSuccess(async () => await _fileUploadsValidatorService.ValidateFileForUpload(formFile, Image))
                .OnSuccess(async () => await Upload(
                    methodologyVersionId,
                    Image,
                    formFile))
                .OnSuccess(fileInfo => new ImageFileViewModel($"/api/methodologies/{methodologyVersionId}/images/{fileInfo.Id}")
                {
                    // TODO EES-1922 Add support for resizing the image
                });
        }

        private async Task<Either<ActionResult, FileInfo>> Upload(Guid methodologyVersionId,
            FileType type,
            IFormFile formFile,
            IDictionary<string, string> metadata = null)
        {
            var methodologyFile = await _methodologyFileRepository.Create(
                methodologyVersionId: methodologyVersionId,
                filename: formFile.FileName,
                type: type,
                createdById: _userService.GetUserId());

            await _contentDbContext.SaveChangesAsync();

            await _blobStorageService.UploadFile(
                containerName: PrivateMethodologyFiles,
                path: methodologyFile.Path(),
                file: formFile,
                metadata: metadata);

            var blob = await _blobStorageService.GetBlob(
                PrivateMethodologyFiles,
                methodologyFile.Path());

            return methodologyFile.ToFileInfo(blob);
        }
    }
}
