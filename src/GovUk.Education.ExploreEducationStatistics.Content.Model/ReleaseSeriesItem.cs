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
    /// Converts the ReleaseSeriesItem to either a LegacyReleaseEntry or a ReleaseEntry,
    /// to allow polymorphic handling of the types of release series items.
    /// Ideally ReleaseSeriesItem should be replaced with this polymorphic model and JSON polymorphic serialization be
    /// added.
    /// </summary>
    public IPublicationReleaseEntry ToPublicationReleaseEntry()
    {
        return IsLegacyLink
            ? new LegacyReleaseEntry
            {
                Id = Id,
                Title = LegacyLinkDescription ?? string.Empty,
                Url = LegacyLinkUrl ?? string.Empty
            }
            : new ReleaseEntry { ReleaseId = ReleaseId!.Value };
    }
}
