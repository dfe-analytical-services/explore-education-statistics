using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces
{
    public interface IThemeService
    {
        Task<Either<ActionResult, ThemeViewModel>> CreateTheme(CreateThemeRequest request);

        Task<Either<ActionResult, ThemeViewModel>> Get(Guid id);

        Task<Either<ActionResult, List<Theme>>> GetMyThemes();

        Task<Either<ActionResult, TitleAndIdViewModel>> GetSummary(Guid id);
    }
}