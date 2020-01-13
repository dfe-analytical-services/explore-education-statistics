using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Content.Model.ViewModels;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces
{
    public interface IReleaseService
    {
        ReleaseViewModel GetRelease(Guid id);

        ReleaseViewModel GetLatestRelease(Guid id);

        Task SetPublishedDateAsync(Guid id);
    }
}