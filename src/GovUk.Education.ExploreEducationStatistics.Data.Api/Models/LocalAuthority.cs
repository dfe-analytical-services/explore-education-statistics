using Newtonsoft.Json;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Models
{
    public class LocalAuthority
    {
        [JsonProperty(PropertyName = "new_la_code")]
        public string Code { get; set; }

        [JsonProperty(PropertyName = "old_la_code")]
        public string Old_Code { get; set; }

        [JsonProperty(PropertyName = "la_name")]
        public string Name { get; set; }
    }
}