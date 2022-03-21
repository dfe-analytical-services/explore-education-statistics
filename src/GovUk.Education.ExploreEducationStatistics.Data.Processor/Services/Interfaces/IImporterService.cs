#nullable enable
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
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

        Task ImportObservations(
            DataImport dataImport,
            DataColumnCollection cols,
            DataRowCollection rows,
            Subject subject,
            SubjectMeta subjectMeta,
            int batchNo,
            StatisticsDbContext context);

        Task ImportFiltersAndLocations(
            DataImport dataImport,
            DataColumnCollection cols,
            DataRowCollection rows,
            SubjectMeta subjectMeta,
            StatisticsDbContext context);

        TimeIdentifier GetTimeIdentifier(IReadOnlyList<string> rowValues, List<string> colValues);

        int GetYear(IReadOnlyList<string> rowValues, List<string> colValues);
    }
}
