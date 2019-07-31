using GovUk.Education.ExploreEducationStatistics.Data.Processor.Model;

namespace GovUk.Education.ExploreEducationStatistics.Data.Processor.Services.Interfaces
{
    public interface IFileImportService
    {
        void ImportObservations(ImportMessage message);

        void ImportFiltersLocationsAndSchools(ImportMessage message);
    }
}