using Newtonsoft.Json;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Models
{
    public class Region
    {
        [JsonProperty(PropertyName = "region_code")]
        public string Code { get; set; }

        [JsonProperty(PropertyName = "region_name")]
        public string Name { get; set; }
    }
}