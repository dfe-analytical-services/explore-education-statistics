using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Models;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.ViewModels.Meta
{
    public class SubjectMetaViewModel
    {
        public Dictionary<string, LabelValue> Filters { get; set; }

        public Dictionary<string, IndicatorMetaViewModel> Indicators { get; set; }

        public Dictionary<string, ObservationalUnitGeoJsonMeta> Locations { get; set; }
        
        public Dictionary<string, TimePeriodMetaViewModel> TimePeriods { get; set; }
    }
}