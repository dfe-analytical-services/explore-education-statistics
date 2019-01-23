using System;
using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Models;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Importer
{
    public interface ICsvImporter
    {
        List<TidyData> Data(DataCsvFilename dataCsvFilename,
            Guid publicationId,
            Guid releaseId,
            DateTime releaseDate);
    }
}