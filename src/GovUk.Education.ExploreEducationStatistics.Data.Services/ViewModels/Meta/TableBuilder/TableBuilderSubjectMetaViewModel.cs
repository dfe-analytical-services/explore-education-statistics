using System.Collections.Generic;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels.Meta.TableBuilder
{
    public class TableBuilderSubjectMetaViewModel
    {
        public Dictionary<string, FilterMetaViewModel> Filters { get; set; }

        public Dictionary<string, TableBuilderIndicatorsMetaViewModel> Indicators { get; set; }

        public Dictionary<string, TableBuilderObservationalUnitsMetaViewModel> Locations { get; set; }

        public TableBuilderTimePeriodsMetaViewModel TimePeriod { get; set; }
    }
}