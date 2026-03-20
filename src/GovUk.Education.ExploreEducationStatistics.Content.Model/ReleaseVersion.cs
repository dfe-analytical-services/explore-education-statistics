#nullable enable
using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations.Schema;
using GovUk.Education.ExploreEducationStatistics.Common.Model;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model;

public class ReleaseVersion : ICreatedTimestamp<DateTime>
{
    public Guid Id { get; set; }

    /// <summary>
    /// The date when the release version was published.
    /// For the latest published version, this denotes the effective public facing 'last updated' date of the release.
    /// </summary>
    public DateTimeOffset? Published { get; set; }

    /// <summary>
    /// The date displayed as the published date for the release version.
    /// For the initial version, this always equals <c>Published</c>.
    /// For subsequent versions, when publishing completes it inherits the previous version's <c>PublishedDisplayDate</c>,
    /// unless <c>UpdatePublishedDisplayDate</c> is set to true, in which case it is set to <c>Published</c>.
    /// </summary>
    public DateTimeOffset? PublishedDisplayDate { get; set; }

    /// <summary>
    /// The date that the release version is scheduled to be published.
    /// </summary>
    public DateTimeOffset? PublishScheduled { get; set; }

    [NotMapped]
    public bool Live => Published.HasValue && DateTimeOffset.UtcNow >= Published.Value;

    [NotMapped]
    public bool Amendment => Version > 0 && !Live;

    [Obsolete("Use ReleaseVersion.Release.PublicationId. This will be removed in EES-5818")]
    public Guid PublicationId { get; set; }

    [Obsolete("Use ReleaseVersion.Release.Publication. This will be removed in EES-5818")]
    public Publication Publication { get; set; } = null!;

    public List<Update> Updates { get; set; } = [];

    public List<ReleaseStatus> ReleaseStatuses { get; set; } = [];

    public string? LatestInternalReleaseNote
    {
        get
        {
            return ReleaseStatuses.Count > 0
                ? ReleaseStatuses.OrderBy(rs => rs.Created).Last().InternalReleaseNote
                : null;
        }
    }

    public List<ContentSection> Content { get; set; } = [];

    public List<KeyStatistic> KeyStatistics { get; set; } = [];

    public List<FeaturedTable> FeaturedTables { get; set; } = [];

    public string? PreReleaseAccessList { get; set; }

    public string? DataGuidance { get; set; }

    public bool NotifySubscribers { get; set; }

    public DateTime? NotifiedOn { get; set; }

    public List<Organisation> PublishingOrganisations { get; set; } = [];

    /// <summary>
    /// When publishing the release version, if this property is true, <c>PublishedDisplayDate</c> is set to the current date,
    /// otherwise it inherits the previous version's <c>PublishedDisplayDate</c>.
    /// </summary>
    public bool UpdatePublishedDisplayDate { get; set; }

    public ReleaseVersion? PreviousVersion { get; set; }

    public Guid? PreviousVersionId { get; set; }

    public Release Release { get; set; } = null!;

    public Guid ReleaseId { get; set; }

    public DateTime Created { get; set; }

    public User CreatedBy { get; set; } = null!;

    public Guid CreatedById { get; set; }

    public int Version { get; set; }

    public bool SoftDeleted { get; set; }

    [NotMapped]
    public IEnumerable<ContentSection> GenericContent
    {
        get => Content.Where(section => section.Type == ContentSectionType.Generic).ToImmutableList();
        set
        {
            if (value.Any(section => section.Type != ContentSectionType.Generic))
            {
                throw new InvalidOperationException(
                    $"All content sections must be of type {ContentSectionType.Generic}."
                );
            }
            Content.RemoveAll(section => section.Type == ContentSectionType.Generic);
            Content.AddRange(value);
        }
    }

    [NotMapped]
    public ContentSection? KeyStatisticsSecondarySection
    {
        get => FindSingleContentSectionByType(ContentSectionType.KeyStatisticsSecondary);
        set => ReplaceSingleContentSectionOfType(ContentSectionType.KeyStatisticsSecondary, value);
    }

    [NotMapped]
    public ContentSection? HeadlinesSection
    {
        get => FindSingleContentSectionByType(ContentSectionType.Headlines);
        set => ReplaceSingleContentSectionOfType(ContentSectionType.Headlines, value);
    }

    [NotMapped]
    public ContentSection? SummarySection
    {
        get => FindSingleContentSectionByType(ContentSectionType.ReleaseSummary);
        set => ReplaceSingleContentSectionOfType(ContentSectionType.ReleaseSummary, value);
    }

    [NotMapped]
    public ContentSection? RelatedDashboardsSection
    {
        get => FindSingleContentSectionByType(ContentSectionType.RelatedDashboards);
        set => ReplaceSingleContentSectionOfType(ContentSectionType.RelatedDashboards, value);
    }

    [NotMapped]
    public ContentSection? WarningSection
    {
        get => FindSingleContentSectionByType(ContentSectionType.Warning);
        set => ReplaceSingleContentSectionOfType(ContentSectionType.Warning, value);
    }

    public List<DataBlockVersion> DataBlockVersions { get; set; } = [];

    private ContentSection? FindSingleContentSectionByType(ContentSectionType type)
    {
        return Content.SingleOrDefault(section => section.Type == type);
    }

    private void ReplaceSingleContentSectionOfType(ContentSectionType sectionType, ContentSection? replacementSection)
    {
        // This method is only intended to be used for the non-generic section types which are unique per release version
        if (sectionType == ContentSectionType.Generic)
        {
            throw new InvalidOperationException(
                $"This method cannot be used to replace a content section of type {ContentSectionType.Generic}."
            );
        }

        // The replacement section type must match the expected type for the property this method is being called for
        if (replacementSection != null && replacementSection.Type != sectionType)
        {
            throw new InvalidOperationException($"The replacement content section must be of type {sectionType}.");
        }

        var sectionToRemove = Content.SingleOrDefault(section => section.Type == sectionType);
        if (sectionToRemove != null)
        {
            Content.Remove(sectionToRemove);
        }

        if (replacementSection != null)
        {
            Content.Add(replacementSection);
        }
    }

    public ReleaseType Type { get; set; }

    public ReleaseApprovalStatus ApprovalStatus { get; set; }

    private PartialDate? _nextReleaseDate;

    public PartialDate? NextReleaseDate
    {
        get => _nextReleaseDate;
        set
        {
            if (value == null || value.IsValid())
            {
                _nextReleaseDate = value;
            }
            else if (value.IsEmpty())
            {
                _nextReleaseDate = null;
            }
            else
            {
                throw new FormatException("The next release date is invalid");
            }
        }
    }

    public List<Link> RelatedInformation { get; set; } = [];
}
