using System.Collections.Generic;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.ViewModels.Meta
{
    public class SubjectMetaViewModel
    {
        public Dictionary<string,
            LegendOptionsMetaValueModel<Dictionary<string,
                LabelOptionsMetaValueModel<IEnumerable<LabelValueViewModel>>>>> CategoricalFilters { get; set; }

        public Dictionary<string,
            LabelOptionsMetaValueModel<IEnumerable<IndicatorMetaViewModel>>> IndicatorsPrototype { get; set; }

        public Dictionary<string, IEnumerable<IndicatorMetaViewModel>> Indicators { get; set; }
        public Dictionary<string, IEnumerable<CharacteristicMetaViewModel>> Characteristics { get; set; }
        public ObservationalUnitsMetaViewModel ObservationalUnits { get; set; }
    }
}