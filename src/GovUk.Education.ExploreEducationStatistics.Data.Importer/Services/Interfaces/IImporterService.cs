using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Data.Importer.Models;
using GovUk.Education.ExploreEducationStatistics.Data.Model;

namespace GovUk.Education.ExploreEducationStatistics.Data.Importer.Services.Interfaces
{
    public interface IImporterService
    {
        SubjectMeta ImportMeta(List<string> metaLines, Subject subject);

        SubjectMeta GetMeta(List<string> metaLines, Subject subject);

        void ImportObservations(List<string> batch, Subject subject,
            SubjectMeta subjectMeta, int batchNo, int rowsPerBatch);

        void ImportFiltersLocationsAndSchools(List<string> lines, SubjectMeta subjectMeta, Subject subject);
    }
}