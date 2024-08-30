#nullable enable
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Model;

public interface IPublisherClient
{
    Task PublishMethodologyFiles(Guid methodologyId, CancellationToken cancellationToken = default);

    Task PublishReleaseContent(
        ReleasePublishingKeyOld releasePublishingKeyOld, CancellationToken cancellationToken = default);

    Task PublishReleaseFiles(
        IReadOnlyList<ReleasePublishingKeyOld> releasePublishingKeys, CancellationToken cancellationToken = default);

    Task PublishTaxonomy(CancellationToken cancellationToken = default);

    Task HandleReleaseChanged(
        ReleasePublishingKeyOld releasePublishingKeyOld, bool immediate, CancellationToken cancellationToken = default);

    Task RetryReleasePublishing(Guid releaseVersionId, CancellationToken cancellationToken = default);

    Task StageReleaseContent(
        IReadOnlyList<ReleasePublishingKeyOld> releasePublishingKeys, CancellationToken cancellationToken = default);
}
