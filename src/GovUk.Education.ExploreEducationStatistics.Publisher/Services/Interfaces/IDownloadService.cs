using System;
using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Publisher.Model.ViewModels;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces
{
    public interface IDownloadService
    {
        IEnumerable<ThemeTree<PublicationDownloadTreeNode>> GetTree(IEnumerable<Guid> includedReleaseIds);
    }
}