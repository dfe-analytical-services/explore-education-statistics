#nullable enable
using System.ComponentModel.DataAnnotations;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Requests;

public record CreateEducationInNumbersPageRequest
{
    [MaxLength(255)] public string Title { get; set; } = string.Empty;

    [MaxLength(255)] public string Slug { get; set; } = string.Empty;

    [MaxLength(2047)] public string Description { get; set; } = string.Empty;
}

public record UpdateEducationInNumbersPageRequest
{
    [MaxLength(255)] public string? Title { get; set; }

    [MaxLength(255)] public string? Slug { get; set; }

    [MaxLength(2047)] public string? Description { get; set; }

    public bool? Publish { get; set; }
}
