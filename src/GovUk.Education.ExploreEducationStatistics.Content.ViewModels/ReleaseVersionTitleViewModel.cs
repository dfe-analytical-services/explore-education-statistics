using GovUk.Education.ExploreEducationStatistics.Content.Model;

namespace GovUk.Education.ExploreEducationStatistics.Content.ViewModels;

public record ReleaseVersionTitleViewModel
{
    public ReleaseVersionTitleViewModel()
    {
    }

    public ReleaseVersionTitleViewModel(ReleaseVersion releaseVersion)
    {
        Id = releaseVersion.Id;
        Title = releaseVersion.Title;
        Slug = releaseVersion.Slug;
    }

    public Guid Id { get; set; }

    public string Slug { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;
}
