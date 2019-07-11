using System.Collections.Generic;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.ViewModels.Meta
{
    public class SubjectMetaViewModel
    {
        public Dictionary<string, LabelValueViewModel> Filters { get; set; }

        public Dictionary<string, IndicatorMetaViewModel> Indicators { get; set; }

        public Dictionary<string, ObservationalUnitMetaViewModel> Locations { get; set; }
        
        public Dictionary<string, TimePeriodMetaViewModel> TimePeriods { get; set; }
    }
}