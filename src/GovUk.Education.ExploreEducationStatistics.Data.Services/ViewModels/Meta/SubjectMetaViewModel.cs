using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Data.Model;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels.Meta
{
    public class SubjectMetaViewModel
    {
        public Dictionary<string, LabelValue> Filters { get; set; }

        public Dictionary<string, IndicatorMetaViewModel> Indicators { get; set; }

        public Dictionary<string, ObservationalUnitGeoJsonMeta> Locations { get; set; }
        
        public IEnumerable<IdLabel> BoundaryLevels { get; set; }
        
        public Dictionary<string, TimePeriodMetaViewModel> TimePeriods { get; set; }
    }
}