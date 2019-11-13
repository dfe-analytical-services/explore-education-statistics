using GovUk.Education.ExploreEducationStatistics.Data.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels.Meta
{
    public class ObservationalUnitGeoJsonMeta : LabelValue
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public GeographicLevel Level { get; set; }

        public dynamic GeoJson { get; set; }
    }
}