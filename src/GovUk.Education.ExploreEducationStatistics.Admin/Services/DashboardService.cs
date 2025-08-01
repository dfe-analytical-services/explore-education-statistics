using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services;

public class DashboardService(
    ContentDbContext contentDbContext) : IDashboardService
{
    public async Task<Either<ActionResult, DashboardViewModel>> GetDashboard(string slug)
    {
        var dashboard =  await contentDbContext.Dashboards
            .SingleOrNotFoundAsync(dash => dash.Slug == slug)
            .OnSuccess(dashboard =>
        {
            return dashboard.ToViewModel();
        });
    }
}
