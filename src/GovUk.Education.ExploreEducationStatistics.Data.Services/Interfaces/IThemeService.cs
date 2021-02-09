using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces
{
    public interface IThemeService
    {
        IEnumerable<ThemeViewModel> ListThemes();
    }
}