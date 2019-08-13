using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Data.Api.ViewModels.Meta;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Models.Query
{
    public class TableBuilderTableHeadersConfig
    {
        public IEnumerable<IEnumerable<LabelValueViewModel>> ColumnGroups { get; set; }
        public IEnumerable<LabelValueViewModel> Columns { get; set; }
        public IEnumerable<IEnumerable<LabelValueViewModel>> RowGroups { get; set; }
        public IEnumerable<LabelValueViewModel> Rows { get; set; }
    }
}