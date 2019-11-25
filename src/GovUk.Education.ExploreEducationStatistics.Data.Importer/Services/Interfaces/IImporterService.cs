using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Data.Importer.Models;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;

namespace GovUk.Education.ExploreEducationStatistics.Data.Importer.Services.Interfaces
{
    public interface IImporterService
    {
        SubjectMeta ImportMeta(List<string> metaLines, Subject subject, StatisticsDbContext context);

        SubjectMeta GetMeta(List<string> metaLines, Subject subject, StatisticsDbContext context);

        void ImportObservations(List<string> batch, Subject subject,
            SubjectMeta subjectMeta, int batchNo, int rowsPerBatch, StatisticsDbContext context);

        void ImportFiltersLocationsAndSchools(List<string> lines, SubjectMeta subjectMeta, Subject subject, StatisticsDbContext context);
    }
}