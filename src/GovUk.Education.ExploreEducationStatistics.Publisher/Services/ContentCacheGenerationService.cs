using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Services
{
    public class ContentCacheGenerationService : IContentCacheGenerationService
    {
        public ContentCacheGenerationService(ApplicationDbContext context
        )
        {
        }

        public async Task<bool> CleanAndRebuildFullCache()
        {
            // download tree
            // content tree
            // methodology tree

            // methodologies
            // publications & releases

            return true;
        }
    }
}