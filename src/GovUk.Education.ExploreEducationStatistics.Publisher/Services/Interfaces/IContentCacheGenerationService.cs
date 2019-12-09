using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Publisher.Model;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces
{
    public interface IContentCacheGenerationService
    {
        Task<bool> GenerateReleaseContent(GenerateReleaseContentMessage message);
    }
}