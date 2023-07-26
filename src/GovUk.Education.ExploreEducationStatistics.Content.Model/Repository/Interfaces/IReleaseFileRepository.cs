#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;

public interface IReleaseFileRepository
{
    Task<ReleaseFile> Create(
        Guid releaseId,
        string filename,
        long contentLength,
        string contentType,
        FileType type,
        Guid createdById,
        string? name = null,
        string? summary = null,
        Guid? newFileId = null);

    Task Delete(Guid releaseId, Guid fileId);

    Task<ReleaseFile?> Find(Guid releaseId, Guid fileId);

    Task<Either<ActionResult, ReleaseFile>> FindOrNotFound(Guid releaseId, Guid fileId);

    Task<List<ReleaseFile>> GetByFileType(Guid releaseId, params FileType[] types);

    Task<bool> FileIsLinkedToOtherReleases(Guid releaseId, Guid fileId);

    Task<ReleaseFile> Update(
        Guid releaseId,
        Guid fileId,
        Guid? newFileId = null,
        string? name = null,
        string? fileName = null,
        string? summary = null);
}
