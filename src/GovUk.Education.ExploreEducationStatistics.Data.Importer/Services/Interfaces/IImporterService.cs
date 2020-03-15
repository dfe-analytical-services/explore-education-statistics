using System.Collections.Generic;
using System.Data;
using GovUk.Education.ExploreEducationStatistics.Data.Importer.Models;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;

namespace GovUk.Education.ExploreEducationStatistics.Data.Importer.Services.Interfaces
{
    public interface IImporterService
    {
        void ImportMeta(DataTable table, Subject subject, StatisticsDbContext context);

        SubjectMeta GetMeta(DataTable table, Subject subject, StatisticsDbContext context);

        void ImportObservations(DataColumnCollection cols, DataRowCollection rows, Subject subject,
            SubjectMeta subjectMeta, int batchNo, int rowsPerBatch, StatisticsDbContext context);

        void ImportFiltersLocationsAndSchools(DataColumnCollection cols, DataRowCollection rows, SubjectMeta subjectMeta, Subject subject, StatisticsDbContext context);
    }
}