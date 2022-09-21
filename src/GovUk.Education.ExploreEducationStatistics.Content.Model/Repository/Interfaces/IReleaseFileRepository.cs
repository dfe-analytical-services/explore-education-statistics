#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces
{
    public interface IReleaseFileRepository
    {
        public Task<ReleaseFile> Create(
            Guid releaseId,
            string filename,
            long contentLength,
            string contentType,
            FileType type,
            Guid createdById,
            string? name = null,
            string? summary = null);

        public Task Delete(Guid releaseId, Guid fileId);

        public Task<ReleaseFile?> Find(Guid releaseId, Guid fileId);

        public Task<Either<ActionResult, ReleaseFile>> FindOrNotFound(Guid releaseId, Guid fileId);

        public Task<List<ReleaseFile>> GetByFileType(Guid releaseId, params FileType[] types);

        public Task<bool> FileIsLinkedToOtherReleases(Guid releaseId, Guid fileId);

        public Task<ReleaseFile> Update(
            Guid releaseId,
            Guid fileId,
            string? name = null,
            string? fileName = null,
            string? summary = null);
    }
}
