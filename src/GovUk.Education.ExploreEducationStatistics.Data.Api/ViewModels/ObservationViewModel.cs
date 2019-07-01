using System.Collections.Generic;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.ViewModels
{
    public class ObservationViewModel
    {
        public IEnumerable<string> Filters { get; set; }
        
        public LocationViewModel Location { get; set; }

        public Dictionary<string, string> Measures { get; set; }
        
        public string TimePeriod { get; set; }
    }
}