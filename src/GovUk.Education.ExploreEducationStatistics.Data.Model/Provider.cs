using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model
{
    public class Provider
    {
        [Key]
        [JsonProperty(PropertyName = "provider_urn")]
        public string Urn { get; set; }

        [JsonProperty(PropertyName = "provider_ukprn")]
        public string Ukprn { get; set; }

        [JsonProperty(PropertyName = "provider_upin")]
        public string Upin { get; set; }

        [JsonProperty(PropertyName = "provider_name")]
        public string Name { get; set; }

        public IEnumerable<Observation> Observations { get; set; }

        private Provider()
        {
        }

        public Provider(string urn, string ukprn, string upin, string name)
        {
            Urn = urn;
            Ukprn = ukprn;
            Upin = upin;
            Name = name;
        }
    }
}