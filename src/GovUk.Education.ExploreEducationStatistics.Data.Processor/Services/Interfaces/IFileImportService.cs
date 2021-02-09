using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Model;

namespace GovUk.Education.ExploreEducationStatistics.Data.Processor.Services.Interfaces
{
    public interface IFileImportService
    {
        Task ImportObservations(ImportObservationsMessage message, StatisticsDbContext context);

        Task ImportFiltersAndLocations(Guid importId, StatisticsDbContext context);
    }
}