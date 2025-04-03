#nullable enable
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;

public interface IReleaseFileRepository
{
    Task<ReleaseFile> Create(
        Guid releaseVersionId,
        string filename,
        long contentLength,
        string contentType,
        FileType type,
        Guid createdById,
        string? name = null,
        string? summary = null,
        Guid? newFileId = null);

    Task Delete(Guid releaseVersionId,
        Guid fileId);

    Task<ReleaseFile?> Find(Guid releaseVersionId,
        Guid fileId);

    Task<Either<ActionResult, ReleaseFile>> FindOrNotFound(Guid releaseVersionId,
        Guid fileId);

    Task<List<ReleaseFile>> GetByFileType(Guid releaseVersionId,
        CancellationToken cancellationToken = default,
        params FileType[] types);

    Task<bool> FileIsLinkedToOtherReleases(Guid releaseVersionId,
        Guid fileId);

    Task<ReleaseFile> Update(
        Guid releaseVersionId,
        Guid fileId,
        Guid? newFileId = null,
        string? name = null,
        string? fileName = null,
        string? summary = null);

    Task<ReleaseFile?> GetByIdOrDefaultAsync(Guid releaseFileId);
}
