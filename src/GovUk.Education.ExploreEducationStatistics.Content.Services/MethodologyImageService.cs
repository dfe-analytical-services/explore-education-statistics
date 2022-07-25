using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static GovUk.Education.ExploreEducationStatistics.Common.BlobContainers;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services
{
    public class MethodologyImageService : IMethodologyImageService
    {
        private readonly IPersistenceHelper<ContentDbContext> _persistenceHelper;
        private readonly IBlobStorageService _blobStorageService;

        public MethodologyImageService(IPersistenceHelper<ContentDbContext> persistenceHelper,
            IBlobStorageService blobStorageService)
        {
            _persistenceHelper = persistenceHelper;
            _blobStorageService = blobStorageService;
        }

        public async Task<Either<ActionResult, FileStreamResult>> Stream(Guid methodologyVersionId, Guid fileId)
        {
            return await _persistenceHelper
                .CheckEntityExists<MethodologyFile>(q => q
                    .Include(mf => mf.File)
                    .Where(mf => mf.MethodologyVersionId == methodologyVersionId && mf.FileId == fileId))
                .OnSuccess(async mf =>
                {
                    return await GetBlob(mf.Path())
                        .OnSuccess(blob => DownloadToStream(blob, mf.File.Filename));
                });
        }

        private async Task<FileStreamResult> DownloadToStream(BlobInfo blob, string filename)
        {
            var stream = new MemoryStream();
            await _blobStorageService.DownloadToStream(PublicMethodologyFiles, blob.Path, stream);

            return new FileStreamResult(stream, blob.ContentType)
            {
                FileDownloadName = filename
            };
        }

        private async Task<Either<ActionResult, BlobInfo>> GetBlob(string path)
        {
            if (!await _blobStorageService.CheckBlobExists(PublicMethodologyFiles, path))
            {
                return new NotFoundResult();
            }

            var blob = await _blobStorageService.GetBlob(PublicMethodologyFiles, path);

            if (!blob.IsReleased())
            {
                return new NotFoundResult();
            }

            return blob;
        }
    }
}
