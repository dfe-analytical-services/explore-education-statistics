#nullable enable
using System;
using System.ComponentModel.DataAnnotations;

namespace GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;

public record EmbedBlockCreateRequest
{
    [Required] public string Title { get; init; } = string.Empty;

    [Required] public string Url { get; init; } = string.Empty;

    [Required] public Guid ContentSectionId { get; init; }
}

public record EmbedBlockUpdateRequest
{
    [Required] public Guid ContentBlockId { get; init; }

    [Required] public string Title { get; init; } = string.Empty;

    [Required] public string Url { get; init; } = string.Empty;
}
