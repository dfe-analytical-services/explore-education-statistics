using System.Collections.Generic;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.ViewModels.Meta
{
    public class FilterItemMetaViewModel : LabelOptionsMetaValueModel<IEnumerable<LabelValueViewModel>>
    {
        public string TotalValue { get; set; }
    }
}