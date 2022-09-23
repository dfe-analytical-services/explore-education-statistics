#nullable enable
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static GovUk.Education.ExploreEducationStatistics.Common.BlobContainers;
using static GovUk.Education.ExploreEducationStatistics.Common.Model.FileType;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services
{
    public class ReleaseImageService : IReleaseImageService
    {
        private readonly ContentDbContext _contentDbContext;
        private readonly IPersistenceHelper<ContentDbContext> _persistenceHelper;
        private readonly IBlobStorageService _blobStorageService;
        private readonly IFileUploadsValidatorService _fileUploadsValidatorService;
        private readonly IReleaseFileRepository _releaseFileRepository;
        private readonly IUserService _userService;

        public ReleaseImageService(ContentDbContext contentDbContext,
            IPersistenceHelper<ContentDbContext> persistenceHelper,
            IBlobStorageService blobStorageService,
            IFileUploadsValidatorService fileUploadsValidatorService,
            IReleaseFileRepository releaseFileRepository,
            IUserService userService)
        {
            _contentDbContext = contentDbContext;
            _persistenceHelper = persistenceHelper;
            _blobStorageService = blobStorageService;
            _fileUploadsValidatorService = fileUploadsValidatorService;
            _releaseFileRepository = releaseFileRepository;
            _userService = userService;
        }

        public async Task<Either<ActionResult, FileStreamResult>> Stream(Guid releaseId, Guid fileId)
        {
            return await _persistenceHelper
                .CheckEntityExists<ReleaseFile>(q => q
                    .Include(rf => rf.File)
                    .Where(rf => rf.ReleaseId == releaseId && rf.FileId == fileId))
                .OnSuccessCombineWith(rf =>
                    _blobStorageService.DownloadToStream(PrivateReleaseFiles, rf.Path(), new MemoryStream()))
                .OnSuccess(releaseFileAndStream =>
                {
                    var (releaseFile, stream) = releaseFileAndStream;
                    return new FileStreamResult(stream, releaseFile.File.ContentType)
                    {
                        FileDownloadName = releaseFile.File.Filename
                    };
                });
        }

        public Task<Either<ActionResult, ImageFileViewModel>> Upload(Guid releaseId, IFormFile formFile)
        {
            return _persistenceHelper
                .CheckEntityExists<Release>(releaseId)
                .OnSuccess(_userService.CheckCanUpdateRelease)
                .OnSuccess(async () => await _fileUploadsValidatorService.ValidateFileForUpload(formFile, Image))
                .OnSuccess(async () => await Upload(
                    releaseId,
                    Image,
                    formFile))
                .OnSuccess(releaseFile => new ImageFileViewModel($"/api/releases/{releaseId}/images/{releaseFile.File.Id}")
                {
                    // TODO EES-1922 Add support for resizing the image
                });
        }

        private async Task<Either<ActionResult, ReleaseFile>> Upload(Guid releaseId,
            FileType type,
            IFormFile formFile)
        {
            var releaseFile = await _releaseFileRepository.Create(
                releaseId: releaseId,
                filename: formFile.FileName,
                contentLength: formFile.Length,
                contentType: formFile.ContentType,
                type: type,
                createdById: _userService.GetUserId());

            await _contentDbContext.SaveChangesAsync();

            await _blobStorageService.UploadFile(
                containerName: PrivateReleaseFiles,
                path: releaseFile.Path(),
                file: formFile);

            return releaseFile;
        }
    }
}
