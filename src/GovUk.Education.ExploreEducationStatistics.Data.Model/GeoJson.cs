using Newtonsoft.Json;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model
{
    /// <summary>
    /// Read only query type which maps to the geojson database view.  
    /// </summary>
    public class GeoJson
    {
        public long BoundaryLevelId { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }

        public dynamic Deserialized => JsonConvert.DeserializeObject(Value);
    }
}
