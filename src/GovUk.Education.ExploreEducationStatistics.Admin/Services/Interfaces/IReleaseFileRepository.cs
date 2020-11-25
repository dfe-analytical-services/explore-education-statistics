using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces
{
    public interface IReleaseFileRepository
    {
        public Task<Either<ActionResult, ReleaseFileReference>> CheckFileExists(Guid releaseId, Guid id,
            params ReleaseFileTypes[] allowedFileTypes);
        
        public Task Delete(Guid releaseId, Guid releaseFileReferenceId);

        public Task<ReleaseFile> Get(Guid releaseId, Guid releaseFileReferenceId);

        public Task<List<ReleaseFile>> GetByFileType(Guid releaseId, params ReleaseFileTypes[] types);

        public Task<bool> FileIsLinkedToOtherReleases(Guid releaseId, Guid fileId);
    }
}