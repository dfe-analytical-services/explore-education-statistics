using System.Collections.Generic;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.ViewModels.Meta.TableBuilder
{
    public class TableBuilderFilterMetaViewModel : LegendOptionsMetaValueModel<Dictionary<string, TableBuilderFilterItemMetaViewModel>>
    {
        public string TotalValue { get; set; }
    }
}