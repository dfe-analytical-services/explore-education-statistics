#nullable enable
namespace GovUk.Education.ExploreEducationStatistics.Common.ViewModels;

public record IdTitleViewModel
{
    public Guid Id { get; set; }

    public string Title { get; init; } = string.Empty;

    public IdTitleViewModel() { }

    public IdTitleViewModel(Guid id, string title)
    {
        Id = id;
        Title = title;
    }
}
