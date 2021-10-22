#nullable enable
using System;
using GovUk.Education.ExploreEducationStatistics.Common;
using GovUk.Education.ExploreEducationStatistics.Common.Cache.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.FileStoragePathUtils;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Controllers.Cache
{
    public record SubjectMetaCacheKey : IBlobCacheKey
    {
        private Guid ReleaseId { get; }
        private Guid SubjectId { get; }

        public SubjectMetaCacheKey(ReleaseSubject releaseSubject)
        {
            ReleaseId = releaseSubject.ReleaseId;
            SubjectId = releaseSubject.SubjectId;
        }
        
        public IBlobContainer Container => BlobContainers.PublicContent;

        public string Key => PublicContentSubjectMetaPath(ReleaseId, SubjectId);
    }
}
