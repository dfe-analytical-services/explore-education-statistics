using System.Collections.Generic;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels.Meta
{
    public class SubjectMetaViewModel
    {
        public Dictionary<string, FilterMetaViewModel> Filters { get; set; }

        public Dictionary<string, IndicatorsMetaViewModel> Indicators { get; set; }

        public Dictionary<string, ObservationalUnitsMetaViewModel> Locations { get; set; }

        public TimePeriodsMetaViewModel TimePeriod { get; set; }
    }
}