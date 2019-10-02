using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using GovUk.Education.ExploreEducationStatistics.Content.Model;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces
{
    public interface IMetaService
    {
        List<TimeIdentifierCategoryModel> GetTimeIdentifiersByCategory();
        List<ReleaseType> GetReleaseTypes();
    }
}