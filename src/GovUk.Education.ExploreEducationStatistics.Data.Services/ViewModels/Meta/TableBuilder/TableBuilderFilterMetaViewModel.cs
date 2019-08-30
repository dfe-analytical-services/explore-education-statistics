using System.Collections.Generic;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels.Meta.TableBuilder
{
    public class
        TableBuilderFilterMetaViewModel : LegendOptionsMetaValueModel<Dictionary<string, TableBuilderFilterItemsMetaViewModel>>
    {
        public string TotalValue { get; set; }
    }
}