#nullable enable
using System;
using JsonKnownTypes;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.ViewModels;

[JsonKnownThisType("HtmlBlock")]
public record HtmlBlockViewModel : IContentBlockViewModel
{
    public Guid Id { get; set; }

    public int Order { get; set; }

    public string Body { get; set; } = string.Empty;
}
