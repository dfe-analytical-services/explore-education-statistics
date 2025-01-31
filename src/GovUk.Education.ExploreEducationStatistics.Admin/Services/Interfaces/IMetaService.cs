#nullable enable
using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Admin.Models;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces
{
    public interface IMetaService
    {
        List<TimeIdentifierCategoryModel> GetTimeIdentifiersByCategory();
    }
}
