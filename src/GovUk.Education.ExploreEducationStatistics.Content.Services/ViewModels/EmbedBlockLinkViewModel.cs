#nullable enable
using System;
using JsonKnownTypes;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.ViewModels;

[JsonKnownThisType("EmbedBlockLink")]
public record EmbedBlockLinkViewModel : IContentBlockViewModel
{
    public Guid Id { get; set; }

    public int Order { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Url { get; set; } = string.Empty;
}
