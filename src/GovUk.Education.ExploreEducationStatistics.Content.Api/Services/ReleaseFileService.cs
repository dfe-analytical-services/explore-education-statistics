using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static GovUk.Education.ExploreEducationStatistics.Common.BlobContainers;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Services
{
    public class ReleaseFileService : IReleaseFileService
    {
        private readonly IPersistenceHelper<ContentDbContext> _persistenceHelper;
        private readonly IBlobStorageService _blobStorageService;

        public ReleaseFileService(IPersistenceHelper<ContentDbContext> persistenceHelper,
            IBlobStorageService blobStorageService)
        {
            _persistenceHelper = persistenceHelper;
            _blobStorageService = blobStorageService;
        }

        public async Task<Either<ActionResult, FileStreamResult>> Stream(Guid releaseId, Guid fileId)
        {
            return await _persistenceHelper
                .CheckEntityExists<ReleaseFile>(q => q
                    .Include(rf => rf.File)
                    .Include(rf => rf.Release)
                    .ThenInclude(release => release.Publication)
                    .Where(rf => rf.ReleaseId == releaseId && rf.FileId == fileId)
                )
                .OnSuccess(async rf =>
                {
                    return await GetBlob(rf.PublicPath())
                        .OnSuccess(blob => DownloadToStream(blob, rf.File.Filename));
                });
        }

        public async Task<Either<ActionResult, FileStreamResult>> StreamByPath(string path)
        {
            return await GetBlob(path)
                .OnSuccess(DownloadToStream);
        }

        private async Task<FileStreamResult> DownloadToStream(BlobInfo blob, string filename)
        {
            var stream = new MemoryStream();
            await _blobStorageService.DownloadToStream(PublicReleaseFiles, blob.Path, stream);

            return new FileStreamResult(stream, blob.ContentType)
            {
                FileDownloadName = filename
            };
        }

        private async Task<FileStreamResult> DownloadToStream(BlobInfo blob)
        {
            var stream = new MemoryStream();
            await _blobStorageService.DownloadToStream(PublicReleaseFiles, blob.Path, stream);

            return new FileStreamResult(stream, blob.ContentType)
            {
                FileDownloadName = blob.FileName
            };
        }

        private async Task<Either<ActionResult, BlobInfo>> GetBlob(string path)
        {
            if (!await _blobStorageService.CheckBlobExists(PublicReleaseFiles, path))
            {
                return new NotFoundResult();
            }

            var blob = await _blobStorageService.GetBlob(PublicReleaseFiles, path);

            if (!blob.IsReleased())
            {
                return new NotFoundResult();
            }

            return blob;
        }
    }
}
