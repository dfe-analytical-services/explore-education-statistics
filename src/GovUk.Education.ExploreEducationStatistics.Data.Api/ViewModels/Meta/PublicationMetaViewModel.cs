using System.Collections.Generic;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.ViewModels.Meta
{
    public class SubjectMetaViewModel
    {
        public Dictionary<string,
            LegendOptionsMetaValueModel<Dictionary<string,
                LabelOptionsMetaValueModel<IEnumerable<LabelValueViewModel>>>>> Filters { get; set; }

        public Dictionary<string,
            LabelOptionsMetaValueModel<IEnumerable<IndicatorMetaViewModel>>> Indicators { get; set; }

        public Dictionary<string, LegendOptionsMetaValueModel<IEnumerable<LabelValueViewModel>>> Locations
        {
            get;
            set;
        }

        public LegendOptionsMetaValueModel<IEnumerable<TimePeriodMetaViewModel>> TimePeriod { get; set; }
    }
}