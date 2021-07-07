using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Models;

namespace GovUk.Education.ExploreEducationStatistics.Data.Processor.Services.Interfaces
{
    public interface IImporterService
    {
        void ImportMeta(DataTable table, Subject subject, StatisticsDbContext context);

        SubjectMeta GetMeta(DataTable table, Subject subject, StatisticsDbContext context);

        Task ImportObservations(DataColumnCollection cols,
            DataRowCollection rows,
            Subject subject,
            SubjectMeta subjectMeta,
            HashSet<GeographicLevel> subjectGeographicLevels,
            int batchNo,
            int rowsPerBatch,
            StatisticsDbContext context);

        Task ImportFiltersAndLocations(DataImport dataImport,
            DataColumnCollection cols,
            DataRowCollection rows,
            SubjectMeta subjectMeta,
            StatisticsDbContext context);

        GeographicLevel GetGeographicLevel(IReadOnlyList<string> line, List<string> headers);

        TimeIdentifier GetTimeIdentifier(IReadOnlyList<string> line, List<string> headers);

        int GetYear(IReadOnlyList<string> line, List<string> headers);
    }
}
