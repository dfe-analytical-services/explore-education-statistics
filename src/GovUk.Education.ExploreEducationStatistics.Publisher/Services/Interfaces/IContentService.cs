using System;
using System.Threading.Tasks;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces
{
    public interface IContentService
    {
        Task UpdateAllContentAsync();
        Task UpdateContentAsync(Guid releaseId);
    }
}