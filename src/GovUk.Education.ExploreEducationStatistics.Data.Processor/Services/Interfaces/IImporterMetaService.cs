using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Models;

namespace GovUk.Education.ExploreEducationStatistics.Data.Processor.Services.Interfaces
{
    public interface IImporterMetaService
    {
        Task<SubjectMeta> Import(
            List<string> metaFileCsvHeaders,
            List<List<string>> metaFileRows, 
            Subject subject, 
            StatisticsDbContext context);

        SubjectMeta Get(
            List<string> metaFileCsvHeaders,
            List<List<string>> metaFileRows, 
            Subject subject,
            StatisticsDbContext context);
    }
}
