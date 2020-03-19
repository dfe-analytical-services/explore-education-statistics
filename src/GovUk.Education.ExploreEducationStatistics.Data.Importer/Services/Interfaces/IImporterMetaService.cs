using System.Collections.Generic;
using System.Data;
using GovUk.Education.ExploreEducationStatistics.Data.Importer.Models;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;

namespace GovUk.Education.ExploreEducationStatistics.Data.Importer.Services.Interfaces
{
    public interface IImporterMetaService
    {
        SubjectMeta Import(DataColumnCollection cols, DataRowCollection rows, Subject subject, StatisticsDbContext context);

        SubjectMeta Get(DataColumnCollection cols, DataRowCollection rows, Subject subject, StatisticsDbContext context);
    }
}