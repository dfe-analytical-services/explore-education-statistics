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

        var releaseOrder = release.Publication.ReleaseOrders.Find(ro => ro.ReleaseId == release.Id)
            ?? throw new KeyNotFoundException($"No matching ReleaseOrder found for {nameof(Release)} \"{release.Title}\"");

        Order = releaseOrder.Order;
    }

    public Guid Id { get; set; }

    public string Slug { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public int? Order { get; set; }
}
