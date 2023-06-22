using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Models;

namespace GovUk.Education.ExploreEducationStatistics.Data.Processor.Services.Interfaces
{
    public interface IFileImportService
    {
        Task ImportObservations(DataImport import, StatisticsDbContext context);

        Task ImportFiltersAndLocations(Guid importId, SubjectMeta subjectMeta, StatisticsDbContext context);
    }
}
