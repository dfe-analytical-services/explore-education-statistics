using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces
{
    public interface IThemeService
    {
        Task<Either<ActionResult, ThemeViewModel>> CreateTheme(ThemeSaveViewModel created);

        Task<Either<ActionResult, ThemeViewModel>> UpdateTheme(Guid id, ThemeSaveViewModel updated);

        Task<Either<ActionResult, ThemeViewModel>> GetTheme(Guid id);

        Task<Either<ActionResult, List<ThemeViewModel>>> GetThemes();

        Task<Either<ActionResult, Unit>> DeleteTheme(Guid themeId);
    }
}
