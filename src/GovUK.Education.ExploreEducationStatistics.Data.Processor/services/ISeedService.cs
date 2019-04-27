namespace GovUk.Education.ExploreEducationStatistics.Data.Processor.Services
{
    using GovUk.Education.ExploreEducationStatistics.Data.Processor.Models;
    using Microsoft.WindowsAzure.Storage.Blob;

    public interface ISeedService
    {
        void Seed(Publication publication);
    }
}