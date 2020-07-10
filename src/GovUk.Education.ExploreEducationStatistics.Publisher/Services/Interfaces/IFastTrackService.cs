using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Publisher.Models;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces
{
    public interface IFastTrackService
    {
        Task CreateAllByRelease(Guid releaseId, PublishContext context);

        Task DeleteAllReleaseFastTracks();
    }
}