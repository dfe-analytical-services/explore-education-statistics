#nullable enable

using GovUk.Education.ExploreEducationStatistics.Content.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces;

public interface IThemeService
{
    Task<IList<ThemeViewModel>> ListThemes();
}
