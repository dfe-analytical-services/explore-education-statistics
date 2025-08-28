#nullable enable
namespace GovUk.Education.ExploreEducationStatistics.Publisher.Model;

/// <summary>
/// Represents a key used for identifying a release during the publishing process.
/// </summary>
/// <param name="ReleaseVersionId">The unique identifier for the release version.</param>
/// <param name="ReleaseStatusId">The unique identifier for the release status.</param>
public record ReleasePublishingKey(Guid ReleaseVersionId, Guid ReleaseStatusId);
