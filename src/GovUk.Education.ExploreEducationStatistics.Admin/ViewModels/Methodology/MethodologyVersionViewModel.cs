#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.ViewModels;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.Methodology;

public record MethodologyVersionViewModel
{
    public Guid Id { get; set; }

    public Guid MethodologyId { get; set; }

    public string? InternalReleaseNote { get; set; }

    public PublicationSummaryViewModel OwningPublication { get; set; }

    public List<PublicationSummaryViewModel> OtherPublications { get; set; } = new();

    public DateTime? Published { get; set; }

    [JsonConverter(typeof(StringEnumConverter))]
    public MethodologyPublishingStrategy PublishingStrategy { get; set; }

    public IdTitleViewModel? ScheduledWithRelease { get; set; }

    public string Slug { get; set; } = string.Empty;

    [JsonConverter(typeof(StringEnumConverter))]
    public MethodologyApprovalStatus Status { get; set; }

    public string Title { get; set; } = string.Empty;

    public bool Amendment { get; set; }

    public Guid? PreviousVersionId { get; set; }
}
