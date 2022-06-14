#nullable enable
using System;
using GovUk.Education.ExploreEducationStatistics.Common;
using GovUk.Education.ExploreEducationStatistics.Common.Cache.Interfaces;
using static GovUk.Education.ExploreEducationStatistics.Common.BlobContainers;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.FileStoragePathUtils;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services.Cache;

public record PrivateSubjectMetaCacheKey : IBlobCacheKey
{
    private Guid ReleaseId { get; }
    private Guid SubjectId { get; }

    public PrivateSubjectMetaCacheKey(Guid releaseId, Guid subjectId)
    {
        ReleaseId = releaseId;
        SubjectId = subjectId;
    }

    public IBlobContainer Container => PrivateContent;

    public string Key => PrivateContentSubjectMetaPath(
        ReleaseId,
        SubjectId
    );
}
