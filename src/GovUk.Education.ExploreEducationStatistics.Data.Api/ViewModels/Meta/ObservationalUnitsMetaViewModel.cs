using System.Collections.Generic;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.ViewModels.Meta
{
    public class ObservationalUnitsMetaViewModel
    {
        public LegendOptionsMetaValueModel<LocationMetaViewModel> Location { get; set; }
        public LegendOptionsMetaValueModel<IEnumerable<TimePeriodMetaViewModel>> TimePeriod { get; set; }
    }
}