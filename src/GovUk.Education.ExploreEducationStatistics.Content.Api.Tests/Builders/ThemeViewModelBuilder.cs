using GovUk.Education.ExploreEducationStatistics.Content.ViewModels;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Tests.Builders;

public class ThemeViewModelBuilder
{
    private Guid? _id;
    private string? _title;

    public ThemeViewModel Build() => new()
    {
        Id = _id ?? Guid.NewGuid(),
        Slug = "Theme slug",
        Title = _title ?? "Theme title",
        Summary = "Theme summary"
    };

    public ThemeViewModelBuilder WithId(Guid id)
    {
        _id = id;
        return this;
    }

    public ThemeViewModelBuilder WithTitle(string title)
    {
        _title = title;
        return this;
    }

    public static implicit operator ThemeViewModel(ThemeViewModelBuilder builder) => builder.Build();
}
