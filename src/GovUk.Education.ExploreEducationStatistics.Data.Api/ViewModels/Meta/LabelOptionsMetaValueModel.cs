using System.Collections.Generic;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.ViewModels.Meta
{
    public class LabelOptionsMetaValueModel<OptionsType>
    {
        public string Label { get; set; }
        public IEnumerable<OptionsType> Options { get; set; }
    }
}