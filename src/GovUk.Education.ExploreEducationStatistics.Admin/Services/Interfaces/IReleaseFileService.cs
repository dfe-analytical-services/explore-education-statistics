#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Requests;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using FileInfo = GovUk.Education.ExploreEducationStatistics.Common.Model.FileInfo;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;

public interface IReleaseFileService
{
    public Task<Either<ActionResult, Content.Model.File>> CheckFileExists(Guid releaseVersionId,
        Guid fileId,
        params FileType[] allowedFileTypes);

    Task<Either<ActionResult, Unit>> Delete(Guid releaseVersionId,
        Guid fileId,
        bool forceDelete = false);

    Task<Either<ActionResult, Unit>> Delete(Guid releaseVersionId,
        IEnumerable<Guid> fileIds,
        bool forceDelete = false);

    Task<Either<ActionResult, Unit>> DeleteAll(Guid releaseVersionId,
        bool forceDelete = false);

    Task<Either<ActionResult, IEnumerable<FileInfo>>> ListAll(Guid releaseVersionId,
        params FileType[] types);

    Task<Either<ActionResult, FileInfo>> GetFile(Guid releaseVersionId,
        Guid fileId);

    Task<Either<ActionResult, FileStreamResult>> Stream(Guid releaseVersionId,
        Guid fileId);

    Task<Either<ActionResult, Unit>> ZipFilesToStream(Guid releaseVersionId,
        Stream outputStream,
        IEnumerable<Guid>? fileIds = null,
        CancellationToken cancellationToken = default);

    Task<Either<ActionResult, Unit>> UpdateDataFileDetails(Guid releaseVersionId,
        Guid fileId,
        ReleaseDataFileUpdateRequest update);

    Task<Either<ActionResult, IEnumerable<FileInfo>>> GetAncillaryFiles(Guid releaseVersionId);

    Task<Either<ActionResult, FileInfo>> UploadAncillary(Guid releaseVersionId,
        ReleaseAncillaryFileUploadRequest upload);

    Task<Either<ActionResult, FileInfo>> UpdateAncillary(Guid releaseVersionId,
        Guid fileId,
        ReleaseAncillaryFileUpdateRequest request);

    Task<Either<ActionResult, FileInfo>> UploadChart(Guid releaseVersionId,
        IFormFile formFile,
        Guid? replacingId = null);
}
