using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces;

public interface IReleaseService
{
    Task<ReleaseVersion> Get(Guid releaseVersionId);

    Task<IEnumerable<ReleaseVersion>> GetAmendedReleases(IEnumerable<Guid> releaseVersionIds);

    Task<List<File>> GetFiles(Guid releaseVersionId, params FileType[] types);

    Task<ReleaseVersion> GetLatestPublishedReleaseVersion(
        Guid publicationId,
        IReadOnlyList<Guid>? includeUnpublishedVersionIds = null);

    Task CompletePublishing(Guid releaseVersionId, DateTime actualPublishedDate);
}
