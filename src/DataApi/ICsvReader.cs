using System.Collections.Generic;
using DataApi.Models;

namespace DataApi
{
    public interface ICsvReader
    {
        IEnumerable<GeographicModel> GeoLevels(string publication, List<string> attributes);

        IEnumerable<LaCharacteristicModel> LaCharacteristics(string publication, List<string> attributes);

        IEnumerable<NationalCharacteristicModel> NationalCharacteristics(string publication, List<string> attributes);
    }
}