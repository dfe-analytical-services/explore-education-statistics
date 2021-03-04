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
using FileInfo = GovUk.Education.ExploreEducationStatistics.Common.Model.FileInfo;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Methodologies
{
    public class MethodologyImageService : IMethodologyImageService
    {
        private readonly ContentDbContext _contentDbContext;
        private readonly IPersistenceHelper<ContentDbContext> _persistenceHelper;
        private readonly IBlobStorageService _blobStorageService;
        private readonly IFileUploadsValidatorService _fileUploadsValidatorService;
        private readonly IMethodologyFileRepository _methodologyFileRepository;
        private readonly IUserService _userService;

        public MethodologyImageService(ContentDbContext contentDbContext,
            IPersistenceHelper<ContentDbContext> persistenceHelper,
            IBlobStorageService blobStorageService,
            IFileUploadsValidatorService fileUploadsValidatorService,
            IMethodologyFileRepository methodologyFileRepository,
            IUserService userService)
        {
            _contentDbContext = contentDbContext;
            _persistenceHelper = persistenceHelper;
            _blobStorageService = blobStorageService;
            _fileUploadsValidatorService = fileUploadsValidatorService;
            _methodologyFileRepository = methodologyFileRepository;
            _userService = userService;
        }

        public async Task<Either<ActionResult, FileStreamResult>> Stream(Guid methodologyId, Guid fileId)
        {
            return await _persistenceHelper
                .CheckEntityExists<MethodologyFile>(q => q
                    .Include(mf => mf.File)
                    .Where(mf => mf.MethodologyId == methodologyId && mf.FileId == fileId))
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

        public Task<Either<ActionResult, ImageFileViewModel>> Upload(Guid methodologyId, IFormFile formFile)
        {
            return _persistenceHelper
                .CheckEntityExists<Methodology>(methodologyId)
                .OnSuccess(_userService.CheckCanUpdateMethodology)
                .OnSuccess(async () => await _fileUploadsValidatorService.ValidateFileForUpload(formFile, Image))
                .OnSuccess(async () => await Upload(
                    methodologyId,
                    Image,
                    formFile))
                .OnSuccess(fileInfo => new ImageFileViewModel($"/api/methodologies/{methodologyId}/images/{fileInfo.Id}")
                {
                    // TODO EES-1922 Add support for resizing the image
                });
        }

        private async Task<Either<ActionResult, FileInfo>> Upload(Guid methodologyId,
            FileType type,
            IFormFile formFile,
            IDictionary<string, string> metadata = null)
        {
            var file = await _methodologyFileRepository.Create(
                methodologyId,
                formFile.FileName,
                type);

            await _contentDbContext.SaveChangesAsync();

            await _blobStorageService.UploadFile(
                containerName: PrivateMethodologyFiles,
                path: file.Path(),
                file: formFile,
                metadata: metadata);

            var blob = await _blobStorageService.GetBlob(
                PrivateMethodologyFiles,
                file.Path());

            return file.ToFileInfo(blob);
        }
    }
}
