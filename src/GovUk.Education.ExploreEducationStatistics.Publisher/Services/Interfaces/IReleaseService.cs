using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.ViewModels;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces
{
    public interface IReleaseService
    {
        Task<Release> GetAsync(Guid id);
        
        Task<IEnumerable<Release>> GetAsync(IEnumerable<Guid> ids);

        ReleaseViewModel GetReleaseViewModel(Guid id);

        ReleaseViewModel GetLatestRelease(Guid id, IEnumerable<Guid> includedReleaseIds);

        Task SetPublishedDateAsync(Guid id);
    }
}