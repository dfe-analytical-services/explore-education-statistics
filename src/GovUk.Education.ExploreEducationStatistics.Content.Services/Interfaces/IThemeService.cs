
using GovUk.Education.ExploreEducationStatistics.Content.ViewModels;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces;

public interface IThemeService
{
    Task<IList<ThemeViewModel>> ListThemes();
}
