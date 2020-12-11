using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Publisher.Model.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Publisher.Models;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces
{
    public interface IReleaseService
    {
        Task<Release> GetAsync(Guid id);

        Task<IEnumerable<Release>> GetAsync(IEnumerable<Guid> ids);

        Task<IEnumerable<Release>> GetAmendedReleases(IEnumerable<Guid> releaseIds);

        Task<List<FileInfo>> GetDownloadFiles(Release release);

        Task<List<ReleaseFileReference>> GetFiles(Guid releaseId, params FileType[] types);

        Task<Release> GetLatestRelease(Guid publicationId, IEnumerable<Guid> includedReleaseIds);

        Task<CachedReleaseViewModel> GetLatestReleaseViewModel(Guid publicationId,
            IEnumerable<Guid> includedReleaseIds,
            PublishContext context);

        Task<CachedReleaseViewModel> GetReleaseViewModel(Guid id, PublishContext context);

        Task SetPublishedDatesAsync(Guid id, DateTime published);

        Task DeletePreviousVersionsStatisticalData(params Guid[] releaseIds);
    }
}