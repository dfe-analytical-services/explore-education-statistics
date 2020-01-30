using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Publisher.ViewModels;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces
{
    public interface IReleaseService
    {
        Task<Release> GetAsync(Guid id);
        
        Task<IEnumerable<Release>> GetAsync(IEnumerable<Guid> ids);

        ReleaseViewModel GetReleaseViewModel(Guid id);

        Release GetLatestRelease(Guid publicationId, IEnumerable<Guid> includedReleaseIds);
        
        ReleaseViewModel GetLatestReleaseViewModel(Guid publicationId, IEnumerable<Guid> includedReleaseIds);

        Task SetPublishedDateAsync(Guid id);
    }
}