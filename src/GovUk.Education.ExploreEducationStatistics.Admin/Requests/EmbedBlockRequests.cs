#nullable enable
using System;
using System.ComponentModel.DataAnnotations;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Requests;

public record EmbedBlockCreateRequest
{
    public const string UrlPattern = @"^https://department-for-education\.shinyapps\.io/.+$";

    [Required] public string Title { get; init; } = string.Empty;

    [Required]
    [Url]
    [RegularExpression(UrlPattern, ErrorMessage = "Url not permitted")]
    public string Url { get; init; } = string.Empty;

    [Required] public Guid ContentSectionId { get; init; }
}

public record EmbedBlockUpdateRequest
{
    [Required] public string Title { get; init; } = string.Empty;

    [Required]
    [Url]
    [RegularExpression(EmbedBlockCreateRequest.UrlPattern, ErrorMessage="URL not permitted")]
    public string Url { get; init; } = string.Empty;
}
