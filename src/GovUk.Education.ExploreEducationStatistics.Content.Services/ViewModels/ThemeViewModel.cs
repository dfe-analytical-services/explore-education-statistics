#nullable enable

using System;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.ViewModels;

public record ThemeViewModel
{
    public Guid Id { get; init; }
    public string Slug { get; init; }
    public string Title { get; init; }
}
