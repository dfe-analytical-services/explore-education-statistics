using Newtonsoft.Json;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Models
{
    public class Characteristic
    {
        [JsonProperty(PropertyName = "characteristic_1")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "characteristic_2")]
        public string Name2 { get; set; }

        [JsonProperty(PropertyName = "characteristic_desc")]
        public string Description { get; set; }
    }
}