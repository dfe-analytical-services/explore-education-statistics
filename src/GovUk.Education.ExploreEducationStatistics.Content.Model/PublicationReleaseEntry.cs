namespace GovUk.Education.ExploreEducationStatistics.Content.Model;

public interface IPublicationReleaseEntry;

public class ReleaseEntry : IPublicationReleaseEntry
{
    public required Guid ReleaseId { get; init; }
}

public class LegacyReleaseEntry : IPublicationReleaseEntry
{
    /// <summary>
    /// Unique identifier for referencing a legacy link in the UI, used when selecting one to edit or delete.
    /// </summary>
    public required Guid Id { get; init; }

    public required string Title { get; init; }

    public required string Url { get; init; }
}
