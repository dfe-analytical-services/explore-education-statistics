#nullable enable
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Newtonsoft.Json;

namespace GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;

public class EinPageVersionSummaryViewModel
{
    public Guid Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? Slug { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public int Version { get; set; }

    public DateTimeOffset? Published { get; set; }

    [JsonIgnore]
    public int Order { get; set; }

    public static EinPageVersionSummaryViewModel FromModel(EinPageVersion pageVersion)
    {
        return new EinPageVersionSummaryViewModel
        {
            Id = pageVersion.Id,
            Title = pageVersion.EinPage.Title,
            Slug = pageVersion.EinPage.Slug,
            Description = pageVersion.EinPage.Description,
            Version = pageVersion.Version,
            Published = pageVersion.Published,
            Order = pageVersion.EinPage.Order,
        };
    }
}

public class EinPageVersionSummaryWithPrevVersionViewModel : EinPageVersionSummaryViewModel
{
    public Guid? PreviousVersionId { get; set; }

    public static EinPageVersionSummaryWithPrevVersionViewModel FromModel(
        EinPageVersion pageVersion,
        Guid? previousVersionId
    )
    {
        return new EinPageVersionSummaryWithPrevVersionViewModel
        {
            Id = pageVersion.Id,
            Title = pageVersion.EinPage.Title,
            Slug = pageVersion.EinPage.Slug,
            Description = pageVersion.EinPage.Description,
            Version = pageVersion.Version,
            Published = pageVersion.Published,
            Order = pageVersion.EinPage.Order,
            PreviousVersionId = previousVersionId,
        };
    }
}
