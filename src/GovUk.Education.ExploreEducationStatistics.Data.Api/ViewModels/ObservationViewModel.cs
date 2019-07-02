using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Converters;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using Newtonsoft.Json;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.ViewModels
{
    public class ObservationViewModel
    {
        public IEnumerable<string> Filters { get; set; }
        
        public LocationViewModel Location { get; set; }

        public Dictionary<string, string> Measures { get; set; }

        [JsonConverter(typeof(EnumToEnumValueJsonConverter<TimeIdentifier>))]
        public TimeIdentifier TimeIdentifier { get; set; }

        public int Year { get; set; }
    }
}