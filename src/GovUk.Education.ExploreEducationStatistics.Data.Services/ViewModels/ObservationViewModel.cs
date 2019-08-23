using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels
{
    public class ObservationViewModel
    {
        public IEnumerable<string> Filters { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public GeographicLevel GeographicLevel { get; set; }

        public LocationViewModel Location { get; set; }

        public Dictionary<string, string> Measures { get; set; }
        
        public string TimePeriod { get; set; }
    }
}