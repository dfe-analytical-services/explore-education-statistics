using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Models;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Importer
{
    public interface IMongoCsvImporter
    {
        List<TidyData> Data(DataCsvFilename dataCsvFilename);
    }
}