using Newtonsoft.Json;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model
{
    public class Region
    {
        [JsonProperty(PropertyName = "region_code")]
        public string Code { get; set; }

        [JsonProperty(PropertyName = "region_name")]
        public string Name { get; set; }
    }
}