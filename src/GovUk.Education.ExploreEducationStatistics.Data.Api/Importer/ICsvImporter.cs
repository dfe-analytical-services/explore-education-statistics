using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Models;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Importer
{
    public interface ICsvImporter
    {
        IEnumerable<TidyData> Data(DataCsvFilename dataCsvFilename, Models.Release release);
    }
}