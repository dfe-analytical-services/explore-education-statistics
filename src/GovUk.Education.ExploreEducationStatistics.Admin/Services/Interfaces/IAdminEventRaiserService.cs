using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Content.Model;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;

public interface IAdminEventRaiserService
{
    Task OnThemeUpdated(Theme theme);
}
