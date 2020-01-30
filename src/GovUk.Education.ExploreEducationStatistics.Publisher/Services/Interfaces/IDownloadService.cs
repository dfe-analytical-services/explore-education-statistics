using System;
using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Publisher.ViewModels;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces
{
    public interface IDownloadService
    {
        IEnumerable<ThemeTree> GetTree(IEnumerable<Guid> includedReleaseIds);
    }
}