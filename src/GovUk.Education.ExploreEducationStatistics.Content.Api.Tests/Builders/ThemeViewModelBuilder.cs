#nullable enable
using System;
using GovUk.Education.ExploreEducationStatistics.Content.ViewModels;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Tests.Builders;

public class ThemeViewModelBuilder
{
    private string? _title;
    public ThemeViewModel Build() => new(Guid.NewGuid(), "theme slug", _title ?? "theme title", "theme summary");

    public ThemeViewModelBuilder WithTitle(string title)
    {
        _title = title;
        return this;
    }
    
    public static implicit operator ThemeViewModel(ThemeViewModelBuilder builder) => builder.Build();
}
