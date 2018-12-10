using System.Collections.Generic;
using DataApi.Models;

namespace DataApi
{
    public interface ICsvReader
    {
        IEnumerable<GeographicModel> GeoLevels(string publication);

        IEnumerable<LaCharacteristicModel> LaCharacteristics(string publication);

        IEnumerable<NationalCharacteristicModel> NationalCharacteristics(string publication);
    }
}