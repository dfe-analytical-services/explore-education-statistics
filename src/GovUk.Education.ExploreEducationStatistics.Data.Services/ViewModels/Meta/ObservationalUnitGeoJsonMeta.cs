using GovUk.Education.ExploreEducationStatistics.Common.Converters;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using Newtonsoft.Json;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels.Meta
{
    public class ObservationalUnitGeoJsonMeta : LabelValue
    {
        [JsonConverter(typeof(EnumToEnumValueJsonConverter<GeographicLevel>))]
        public GeographicLevel Level { get; set; }

        public dynamic GeoJson { get; set; }
    }
}