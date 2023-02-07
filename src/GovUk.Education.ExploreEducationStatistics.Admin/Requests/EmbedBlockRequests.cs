#nullable enable
using System;
using System.ComponentModel.DataAnnotations;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Requests;

public record EmbedBlockCreateRequest
{
    [Required] public string Title { get; init; } = string.Empty;

    [Required]
    [Url]
    public string Url { get; init; } = string.Empty;

    [Required] public Guid ContentSectionId { get; init; }
}

public record EmbedBlockUpdateRequest
{
    [Required] public string Title { get; init; } = string.Empty;

    [Required]
    [Url]
    public string Url { get; init; } = string.Empty;
}
