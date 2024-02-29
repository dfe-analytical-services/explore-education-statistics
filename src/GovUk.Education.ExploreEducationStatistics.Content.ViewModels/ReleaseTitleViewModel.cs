using GovUk.Education.ExploreEducationStatistics.Content.Model;

namespace GovUk.Education.ExploreEducationStatistics.Content.ViewModels;

public record ReleaseTitleViewModel
{
    public ReleaseTitleViewModel()
    {
    }

    public ReleaseTitleViewModel(Release release)
    {
        Id = release.Id;
        Title = release.Title;
        Slug = release.Slug;

        // @MarkFix check we can remove this
        //var releaseSeriesItem = release.Publication.ReleaseSeriesView.First(rsi => rsi.ReleaseParentId == release.ReleaseParentId);

        //Order = releaseSeriesItem.Order;
    }

    public Guid Id { get; set; }

    public string Slug { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    //public int? Order { get; set; } // @MarkFix check we can remove this
}
