namespace GovUk.Education.ExploreEducationStatistics.Publisher.Model;

public record NotifyChangeMessage(bool Immediate, ReleasePublishingKey ReleasePublishingKey)
{
    public Guid ReleaseVersionId => ReleasePublishingKey.ReleaseVersionId;
}

public record PublishMethodologyFilesMessage(Guid MethodologyId);

public record PublishReleaseFilesMessage
{
    /// <summary>
    /// Creates a message for immediate publishing of a single release version.
    /// </summary>
    public static PublishReleaseFilesMessage ForImmediate(ReleasePublishingKey releasePublishingKey) =>
        new() { Immediate = true, ReleasePublishingKeys = [releasePublishingKey] };

    /// <summary>
    /// Creates a message for scheduled publishing of one or more release versions.
    /// </summary>
    public static PublishReleaseFilesMessage ForScheduled(IReadOnlyList<ReleasePublishingKey> releasePublishingKeys) =>
        new() { Immediate = false, ReleasePublishingKeys = releasePublishingKeys };

    public required bool Immediate { get; init; }
    public required IReadOnlyList<ReleasePublishingKey> ReleasePublishingKeys { get; init; }
}

public record PublishTaxonomyMessage;
