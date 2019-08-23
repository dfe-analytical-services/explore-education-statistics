using GovUk.Education.ExploreEducationStatistics.Data.Model;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels.Meta
{
    public class ObservationalUnitGeoJsonMeta : LabelValue
    {
        public GeographicLevel Level { get; set; }
        public dynamic GeoJson { get; set; }
    }
}