using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Data.Importer.Models;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;

namespace GovUk.Education.ExploreEducationStatistics.Data.Importer.Services.Interfaces
{
    public interface IImporterMetaService
    {
        SubjectMeta Import(IEnumerable<string> lines, Subject subject, StatisticsDbContext context);

        SubjectMeta Get(IEnumerable<string> lines, Subject subject, StatisticsDbContext context);
    }
}