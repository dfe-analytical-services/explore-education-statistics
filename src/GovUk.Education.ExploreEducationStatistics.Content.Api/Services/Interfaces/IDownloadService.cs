using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Content.Api.ViewModels;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Services.Interfaces
{
    public interface IDownloadService
    {
        IEnumerable<ThemeTree> GetDownloadTree();
    }
}