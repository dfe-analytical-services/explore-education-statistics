using Newtonsoft.Json;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Models
{
    public class Characteristic
    {
        [JsonProperty(PropertyName = "characteristic_breakdown")]
        public string Breakdown { get; set; }

        [JsonProperty(PropertyName = "characteristic_label")]
        public string Label { get; set; }
    }
}