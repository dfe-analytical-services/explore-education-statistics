using Newtonsoft.Json;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model
{
    public class Country
    {
        [JsonProperty(PropertyName = "country_code")] public string Code { get; set; }
        [JsonProperty(PropertyName = "country_name")] public string Name { get; set; }
    }
}