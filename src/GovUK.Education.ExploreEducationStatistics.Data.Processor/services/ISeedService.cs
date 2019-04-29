namespace GovUk.Education.ExploreEducationStatistics.Data.Processor.Services
{
    using GovUk.Education.ExploreEducationStatistics.Data.Processor.Models;

    public interface ISeedService
    {
        void SeedRelease(Release release);
    }
}