using System.Collections.Generic;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.ViewModels.Meta.TableBuilder
{
    public class TableBuilderSubjectMetaViewModel
    {
        public Dictionary<string, TableBuilderFilterMetaViewModel> Filters { get; set; }

        public Dictionary<string, TableBuilderIndicatorsMetaViewModel> Indicators { get; set; }

        public Dictionary<string, TableBuilderObservationalUnitsMetaViewModel> Locations { get; set; }

        public TableBuilderTimePeriodsMetaViewModel TimePeriod { get; set; }
    }
}