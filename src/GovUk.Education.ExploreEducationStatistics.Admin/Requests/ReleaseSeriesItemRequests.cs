#nullable enable
using System;
using System.ComponentModel.DataAnnotations;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Requests;

public record ReleaseSeriesLegacyLinkAddRequest
{
    [Required]
    public required string Description { get; init; }

    [Required]
    public required string Url { get; init; }
}

public record ReleaseSeriesItemUpdateRequest
{
    public Guid? ReleaseId { get; init; }

    public string? LegacyLinkDescription { get; init; }
    public string? LegacyLinkUrl { get; init; }
}
