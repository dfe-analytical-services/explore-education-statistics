namespace GovUk.Education.ExploreEducationStatistics.Publisher.Model;

public record NotifyChangeMessage(bool Immediate, ReleasePublishingKey ReleasePublishingKey)
{
    public Guid ReleaseVersionId => ReleasePublishingKey.ReleaseVersionId;
}

public record PublishMethodologyFilesMessage(Guid MethodologyId);

public record PublishReleaseFilesMessage(IReadOnlyList<ReleasePublishingKey> ReleasePublishingKeys);

public record PublishTaxonomyMessage;
