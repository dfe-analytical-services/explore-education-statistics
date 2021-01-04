﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces
{
    public interface IReleaseFileRepository
    {
        public Task<Either<ActionResult, File>> CheckFileExists(Guid releaseId, Guid id,
            params FileType[] allowedFileTypes);
        
        public Task Delete(Guid releaseId, Guid fileId);

        public Task<ReleaseFile> Get(Guid releaseId, Guid fileId);

        public Task<List<ReleaseFile>> GetByFileType(Guid releaseId, params FileType[] types);

        public Task<bool> FileIsLinkedToOtherReleases(Guid releaseId, Guid fileId);
    }
}