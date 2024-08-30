using System;
using System.Collections.Generic;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Model;

public record NotifyChangeMessage(bool Immediate, ReleasePublishingKeyOld ReleasePublishingKeyOld)
{
    public Guid ReleaseVersionId => ReleasePublishingKeyOld.ReleaseVersionId;
}

public record PublishMethodologyFilesMessage(Guid MethodologyId);

public record PublishReleaseContentMessage(ReleasePublishingKeyOld ReleasePublishingKeyOld);

public record PublishReleaseFilesMessage(IReadOnlyList<ReleasePublishingKeyOld> ReleasePublishingKeys);

public record PublishTaxonomyMessage;

public record RetryReleasePublishingMessage(Guid ReleaseVersionId);

public record StageReleaseContentMessage(IReadOnlyList<ReleasePublishingKeyOld> ReleasePublishingKeys);

