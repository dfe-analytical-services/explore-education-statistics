#nullable enable

namespace GovUk.Education.ExploreEducationStatistics.Content.Model;

public record ReleaseSeriesItem
{
    /// <summary>
    /// Unique identifier for referencing a legacy link in the UI, used when selecting one to edit or delete.
    /// </summary>
    public Guid Id { get; set; }

    public Guid? ReleaseId { get; set; }

    public string? LegacyLinkUrl { get; set; }
    public string? LegacyLinkDescription { get; set; }

    public bool IsLegacyLink => ReleaseId == null;

    /// <summary>
    /// Converts the instance to either a <see cref="LegacyPublicationReleaseEntry"/> or
    /// a <see cref="PublicationReleaseEntry"/>, allowing consumers to interact with these types, rather than directly
    /// with <see cref="ReleaseSeriesItem"/>. Ideally <see cref="ReleaseSeriesItem"/> should be replaced with this
    /// polymorphic model and JSON polymorphic serialization be added to the database configuration.
    /// </summary>
    public IPublicationReleaseEntry ToPublicationReleaseEntry()
    {
        return IsLegacyLink
            ? new LegacyPublicationReleaseEntry
            {
                Id = Id,
                Title = LegacyLinkDescription ?? string.Empty,
                Url = LegacyLinkUrl ?? string.Empty
            }
            : new PublicationReleaseEntry { ReleaseId = ReleaseId!.Value };
    }
}
