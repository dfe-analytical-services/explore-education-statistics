using System.Collections.Generic;
using Newtonsoft.Json;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model
{
    public class School
    {
        public long Id { get; set; }

        [JsonProperty(PropertyName = "acad_opendate")]
        public string AcademyOpenDate { get; set; }

        [JsonProperty(PropertyName = "acad_type")]
        public string AcademyType { get; set; }

        [JsonProperty(PropertyName = "estab")] public string Estab { get; set; }

        [JsonProperty(PropertyName = "laestab")]
        public string LaEstab { get; set; }

        [JsonProperty(PropertyName = "urn")] public string Urn { get; set; }

        public IEnumerable<Observation> Observations { get; set; }
    }
}