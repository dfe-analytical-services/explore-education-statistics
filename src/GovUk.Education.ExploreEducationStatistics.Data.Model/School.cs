using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model
{
    public class School
    {
        [JsonProperty(PropertyName = "acad_opendate")]
        public string AcademyOpenDate { get; set; }

        [JsonProperty(PropertyName = "acad_type")]
        public string AcademyType { get; set; }

        [JsonProperty(PropertyName = "school_estab")]
        public string Estab { get; set; }

        [Key]
        [JsonProperty(PropertyName = "school_laestab")]
        public string LaEstab { get; set; }

        [JsonProperty(PropertyName = "school_name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "school_postcode")]
        public string Postcode { get; set; }

        [JsonProperty(PropertyName = "school_urn")]
        public string Urn { get; set; }

        public IEnumerable<Observation> Observations { get; set; }
    }
}