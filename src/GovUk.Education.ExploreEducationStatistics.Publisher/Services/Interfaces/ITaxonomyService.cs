using System.Threading.Tasks;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces
{
    public interface ITaxonomyService
    {
        Task SyncTaxonomy();
    }
}