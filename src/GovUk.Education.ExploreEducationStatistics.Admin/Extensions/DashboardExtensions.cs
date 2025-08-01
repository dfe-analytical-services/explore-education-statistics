using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Content.Model;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Extensions;

public static class DashboardExtensions
{
    public static DashboardViewModel ToViewModel(this Dashboard dashboard)
    {
        return new DashboardViewModel // @MarkFix yeah?
        {
            Id = dashboard.Id,
            Title = dashboard.Title,
            Slug = dashboard.Slug,
            Created = dashboard.Created,
            Description = dashboard.Description,
            Order = dashboard.Order,
            Published = dashboard.Published,
            Updated = dashboard.Updated,
            Version = dashboard.Version,
            CreatedBy = dashboard.CreatedBy,
            ParentDashboardId = dashboard.ParentDashboardId,
        };
    }
}
