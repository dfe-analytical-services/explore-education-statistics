using System.Threading.Tasks;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces
{
    public interface IContentCacheGenerationService
    {
        Task<bool> CleanAndRebuildFullCache();
    }
}