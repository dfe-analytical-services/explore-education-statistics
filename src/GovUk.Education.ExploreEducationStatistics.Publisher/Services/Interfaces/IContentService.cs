using System;
using System.Threading.Tasks;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces
{
    public interface IContentService
    {
        Task UpdateTrees();
        Task UpdateAllContent();
        Task UpdateContent(Guid releaseId);
    }
}