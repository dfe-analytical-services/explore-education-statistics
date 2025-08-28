namespace GovUk.Education.ExploreEducationStatistics.Publisher.Model;

public record NotifyChangeMessage(bool Immediate, ReleasePublishingKey ReleasePublishingKey)
{
    public Guid ReleaseVersionId => ReleasePublishingKey.ReleaseVersionId;
}

public record PublishMethodologyFilesMessage(Guid MethodologyId);

public record PublishReleaseContentMessage(ReleasePublishingKey ReleasePublishingKey);

public record PublishReleaseFilesMessage(IReadOnlyList<ReleasePublishingKey> ReleasePublishingKeys);

public record PublishTaxonomyMessage;

public record RetryReleasePublishingMessage(Guid ReleaseVersionId);

public record StageReleaseContentMessage(IReadOnlyList<ReleasePublishingKey> ReleasePublishingKeys);

