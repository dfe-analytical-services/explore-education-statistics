#nullable enable
using System;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.ViewModels;

public record HtmlBlockViewModel : IContentBlockViewModel
{
    public Guid Id { get; set; }

    public int Order { get; set; }

    public string Body { get; set; } = string.Empty;

    public string Type => "HtmlBlock";
}
