using System.Collections.Generic;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels.Meta
{
    public class
        FilterMetaViewModel : LegendOptionsMetaValueModel<Dictionary<string, FilterItemsMetaViewModel>>
    {
        public string TotalValue { get; set; }
    }
}