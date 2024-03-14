using GovUk.Education.ExploreEducationStatistics.Content.Model;

namespace GovUk.Education.ExploreEducationStatistics.Content.ViewModels;

public record ReleaseVersionTitleViewModel // @MarkFix can be remove as we can use ReleaseSeries instead?
{
    public ReleaseVersionTitleViewModel()
    {
    }

    public ReleaseVersionTitleViewModel(ReleaseVersion releaseVersion)
    {
        Id = releaseVersion.Id;
        Title = releaseVersion.Title;
        Slug = releaseVersion.Slug;

        // @MarkFix check we can remove this
        //var releaseSeriesItem = release.Publication.ReleaseSeries.First(rsi => rsi.ReleaseParentId == release.ReleaseParentId);

        //Order = releaseSeriesItem.Order;
    }

    public Guid Id { get; set; }

    public string Slug { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    //public int? Order { get; set; } // @MarkFix check we can remove this
}
