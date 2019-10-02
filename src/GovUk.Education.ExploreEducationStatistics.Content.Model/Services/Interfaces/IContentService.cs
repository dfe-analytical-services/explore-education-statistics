using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Content.Model.ViewModels;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model.Services.Interfaces
{
    public interface IContentService
    {
        List<ThemeTree> GetContentTree();
    }
}