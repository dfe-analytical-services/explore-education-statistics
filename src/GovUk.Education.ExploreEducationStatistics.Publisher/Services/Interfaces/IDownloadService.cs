using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Publisher.Model.ViewModels;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces
{
    public interface IDownloadService
    {
        Task<IEnumerable<ThemeTree<PublicationDownloadTreeNode>>> GetTree(IEnumerable<Guid> includedReleaseIds);
    }
}