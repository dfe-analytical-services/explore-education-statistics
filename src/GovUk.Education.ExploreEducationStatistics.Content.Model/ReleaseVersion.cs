#nullable enable
using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations.Schema;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Newtonsoft.Json;
using static System.DateTime;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model;

public class ReleaseVersion : ICreatedTimestamp<DateTime>
{
    public Guid Id { get; set; }

    /**
     * The last date the release was published - this should be set when the PublishScheduled date is reached and
     * the release is published.
     */
    public DateTime? Published { get; set; }

    // The date that the release is scheduled to be published - when this time is reached then the release should
    // be published and the Published date set.
    public DateTime? PublishScheduled { get; set; }

    [NotMapped]
    public bool Live => Published.HasValue && UtcNow >= Published.Value;

    [NotMapped]
    public bool Amendment => Version > 0 && !Live;

    [Obsolete("Use ReleaseVersion.Release.PublicationId. This will be removed in EES-5818")]
    public Guid PublicationId { get; set; }

    [Obsolete("Use ReleaseVersion.Release.Publication. This will be removed in EES-5818")]
    public Publication Publication { get; set; } = null!;

    public List<Update> Updates { get; set; } = new();

    public List<ReleaseStatus> ReleaseStatuses { get; set; } = new();

    public string? LatestInternalReleaseNote
    {
        get
        {
            return ReleaseStatuses.Count > 0
                ? ReleaseStatuses.OrderBy(rs => rs.Created).Last().InternalReleaseNote
                : null;
        }
    }

    [JsonIgnore]
    public List<ContentSection> Content { get; set; } = new();

    public List<KeyStatistic> KeyStatistics { get; set; } = new();

    public List<FeaturedTable> FeaturedTables { get; set; } = new();

    public string? PreReleaseAccessList { get; set; }

    public string? DataGuidance { get; set; }

    public bool NotifySubscribers { get; set; }

    public DateTime? NotifiedOn { get; set; }

    public List<Organisation> PublishingOrganisations { get; set; } = [];

    public bool UpdatePublishedDate { get; set; }

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
    [JsonProperty("Content")]
    public IEnumerable<ContentSection> GenericContent
    {
        get => Content.Where(section => section.Type == ContentSectionType.Generic).ToImmutableList();
        set => ReplaceContentSectionsOfType(ContentSectionType.Generic, value);
    }

    [NotMapped]
    public ContentSection? KeyStatisticsSecondarySection
    {
        get => FindSingleSectionByType(ContentSectionType.KeyStatisticsSecondary);
        set => ReplaceContentSectionsOfType(ContentSectionType.KeyStatisticsSecondary, value);
    }

    [NotMapped]
    public ContentSection? HeadlinesSection
    {
        get => FindSingleSectionByType(ContentSectionType.Headlines);
        set => ReplaceContentSectionsOfType(ContentSectionType.Headlines, value);
    }

    [NotMapped]
    public ContentSection? SummarySection
    {
        get => FindSingleSectionByType(ContentSectionType.ReleaseSummary);
        set => ReplaceContentSectionsOfType(ContentSectionType.ReleaseSummary, value);
    }

    [NotMapped]
    public ContentSection? RelatedDashboardsSection
    {
        get => FindSingleSectionByType(ContentSectionType.RelatedDashboards);
        set => ReplaceContentSectionsOfType(ContentSectionType.RelatedDashboards, value);
    }

    public List<DataBlockVersion> DataBlockVersions { get; set; } = new();

    private ContentSection? FindSingleSectionByType(ContentSectionType type)
    {
        return Content.SingleOrDefault(section => section.Type == type);
    }

    private void ReplaceContentSectionsOfType(ContentSectionType type, IEnumerable<ContentSection> replacementSections)
    {
        Content.RemoveAll(section => section.Type == type);
        Content.AddRange(replacementSections);
    }

    private void ReplaceContentSectionsOfType(ContentSectionType type, ContentSection? replacementSection)
    {
        Content.RemoveAll(section => section.Type == type);
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

    public List<Link> RelatedInformation { get; set; } = new();
}
