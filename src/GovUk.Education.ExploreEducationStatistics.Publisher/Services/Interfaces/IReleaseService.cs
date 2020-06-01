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

        CachedReleaseViewModel GetReleaseViewModel(Guid id, PublishContext context);

        Release GetLatestRelease(Guid publicationId, IEnumerable<Guid> includedReleaseIds);

        CachedReleaseViewModel GetLatestReleaseViewModel(Guid publicationId, IEnumerable<Guid> includedReleaseIds,
            PublishContext context);

        Task SetPublishedDatesAsync(Guid id, DateTime published);

        List<ReleaseFileReference> GetReleaseFileReferences(Guid releaseId, params ReleaseFileTypes[] types);
    }
}