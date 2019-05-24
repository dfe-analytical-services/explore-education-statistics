using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.ViewModels
{
    public class ObservationViewModel
    {
        public IEnumerable<string> Filters { get; set; }
        
        public LocationViewModel Location { get; set; }

        public Dictionary<string, string> Measures { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public TimeIdentifier TimeIdentifier { get; set; }

        public int Year { get; set; }
    }
}