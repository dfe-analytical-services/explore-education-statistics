﻿#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using FileInfo = GovUk.Education.ExploreEducationStatistics.Common.Model.FileInfo;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces
{
    public interface IReleaseFileService
    {
        Task<Either<ActionResult, Unit>> Delete(
            Guid releaseId,
            Guid id,
            bool forceDelete = false);

        Task<Either<ActionResult, Unit>> Delete(
            Guid releaseId,
            IEnumerable<Guid> ids,
            bool forceDelete = false);

        Task<Either<ActionResult, Unit>> DeleteAll(Guid releaseId, bool forceDelete = false);

        Task<Either<ActionResult, IEnumerable<FileInfo>>> ListAll(Guid releaseId, params FileType[] types);

        Task<Either<ActionResult, FileInfo>> GetFile(Guid releaseId, Guid fileId);

        Task<Either<ActionResult, FileStreamResult>> Stream(Guid releaseId, Guid id);

        Task<Either<ActionResult, Unit>> ZipFilesToStream(
            Guid releaseId,
            Stream outputStream,
            IEnumerable<Guid>? fileIds = null,
            CancellationToken? cancellationToken = null);

        Task<Either<ActionResult, Unit>> Update(Guid releaseId, Guid fileId, ReleaseFileUpdateViewModel update);

        Task<Either<ActionResult, IEnumerable<FileInfo>>> GetAncillaryFiles(Guid releaseId);

        Task<Either<ActionResult, FileInfo>> UploadAncillary(
            Guid releaseId,
            ReleaseAncillaryFileUploadViewModel upload);

        Task<Either<ActionResult, FileInfo>> UploadChart(Guid releaseId,
            IFormFile formFile,
            Guid? replacingId = null);
    }
}
