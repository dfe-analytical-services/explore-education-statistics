using System;
using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Data.Api.ViewModels;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Importer.Old
{
    [Obsolete]
    public interface ICsvReader
    {
        IEnumerable<GeographicModel> GeoLevels(string publication, List<string> attributes);

        IEnumerable<LaCharacteristicModel> LaCharacteristics(string publication, List<string> attributes);

        IEnumerable<NationalCharacteristicModel> NationalCharacteristics(string publication, List<string> attributes);
    }
}