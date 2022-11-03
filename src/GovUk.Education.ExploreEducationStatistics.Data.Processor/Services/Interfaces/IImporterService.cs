#nullable enable
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
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
        Task<SubjectMeta> ImportMeta(
            List<string> metaFileCsvHeaders,
            List<List<string>> metaFileRows, 
            Subject subject, 
            StatisticsDbContext context);

        SubjectMeta GetMeta(
            List<string> metaFileCsvHeaders,
            List<List<string>> metaFileRows, 
            Subject subject, 
            StatisticsDbContext context);

        Task ImportObservations(
            DataImport dataImport,
            Func<Task<Stream>> dataFileStreamProvider,
            Subject subject,
            SubjectMeta subjectMeta,
            int batchNo,
            StatisticsDbContext context);

        Task ImportFiltersAndLocations(
            DataImport dataImport,
            Func<Task<Stream>> dataFileStreamProvider,
            SubjectMeta subjectMeta,
            StatisticsDbContext context);

        TimeIdentifier GetTimeIdentifier(IReadOnlyList<string> rowValues, List<string> colValues);

        int GetYear(IReadOnlyList<string> rowValues, List<string> colValues);
    }
}
