#nullable enable
using System;
using GovUk.Education.ExploreEducationStatistics.Common;
using GovUk.Education.ExploreEducationStatistics.Common.Cache.Interfaces;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.FileStoragePathUtils;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Controllers.Cache
{
    public record SubjectMetaCacheKey : IBlobCacheKey
    {
        private Guid SubjectId { get; }

        public SubjectMetaCacheKey(Guid subjectId)
        {
            SubjectId = subjectId;
        }
        
        public IBlobContainer Container => BlobContainers.PublicContent;

        public string Key => PublicContentSubjectMetaPath(SubjectId);
    }
}
