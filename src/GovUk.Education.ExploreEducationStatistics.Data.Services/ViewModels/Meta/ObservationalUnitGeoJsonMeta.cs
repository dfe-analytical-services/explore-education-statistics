using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels.Meta
{
    public class ObservationalUnitGeoJsonMeta : LabelValue
    {
        [JsonConverter(typeof(StringEnumConverter), true)]
        public GeographicLevel Level { get; set; }

        public dynamic GeoJson { get; set; }

        public bool GeoJsonAvailable => GeoJson != null;
    }
}