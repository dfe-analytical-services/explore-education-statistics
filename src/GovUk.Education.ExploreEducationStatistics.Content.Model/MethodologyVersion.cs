#nullable enable
using System.ComponentModel.DataAnnotations;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.MethodologyPublishingStrategy;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model;

public enum MethodologyApprovalStatus
{
    Draft,
    HigherLevelReview,
    Approved,
}

public enum MethodologyPublishingStrategy
{
    Immediately,
    WithRelease,
}

public class MethodologyVersion : ICreatedTimestamp<DateTime?>
{
    [Key]
    [Required]
    public Guid Id { get; set; }

    public string Title => AlternativeTitle ?? Methodology.OwningPublicationTitle;

    public string? AlternativeTitle { get; set; }

    public string Slug => AlternativeSlug ?? Methodology.OwningPublicationSlug;

    public string? AlternativeSlug { get; set; }

    public MethodologyApprovalStatus Status { get; set; }

    public DateTime? Published { get; set; }

    public DateTime? Updated { get; set; }

    public MethodologyVersionContent MethodologyContent { get; set; } = new();

    public List<MethodologyNote> Notes { get; set; } = new();

    public List<MethodologyRedirect> MethodologyRedirects { get; set; } = [];

    public Methodology Methodology { get; set; } = null!;

    public Guid MethodologyId { get; set; }

    // TODO - can this be non-nullable?
    public DateTime? Created { get; set; }

    // TODO - can this be non-nullable?
    public User? CreatedBy { get; set; }

    // TODO - can this be non-nullable?
    public Guid? CreatedById { get; set; }

    public MethodologyVersion? PreviousVersion { get; set; }

    public Guid? PreviousVersionId { get; set; }

    public int Version { get; set; }

    public MethodologyPublishingStrategy PublishingStrategy { get; set; }

    public ReleaseVersion? ScheduledWithReleaseVersion { get; set; }

    public Guid? ScheduledWithReleaseVersionId { get; set; }

    public bool Approved => Status == MethodologyApprovalStatus.Approved;

    public bool ScheduledForPublishingWithRelease => PublishingStrategy == WithRelease;

    public bool ScheduledForPublishingImmediately => PublishingStrategy == Immediately;

    public bool ScheduledForPublishingWithPublishedRelease
    {
        get
        {
            if (PublishingStrategy != WithRelease)
            {
                return false;
            }

            if (ScheduledWithReleaseVersionId == null || ScheduledWithReleaseVersion == null)
            {
                throw new InvalidOperationException(
                    "ScheduledWithRelease field not included in MethodologyVersion"
                );
            }

            return ScheduledWithReleaseVersion.Live;
        }
    }

    public bool Amendment => PreviousVersionId != null && Published == null;
}

public class MethodologyVersionContent
{
    public Guid MethodologyVersionId { get; set; }

    public List<ContentSection> Content { get; set; } = new();

    public List<ContentSection> Annexes { get; set; } = new();
}
