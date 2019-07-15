using System;
using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Admin.Models;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces
{
    public interface IThemeService
    {
        List<Theme> GetThemes(Guid guid);
    }
}