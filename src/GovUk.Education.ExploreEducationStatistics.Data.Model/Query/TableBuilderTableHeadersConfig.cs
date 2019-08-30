using System.Collections.Generic;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Query
{
    public class TableBuilderTableHeadersConfig
    {
        public IEnumerable<IEnumerable<LabelValue>> ColumnGroups { get; set; }
        public IEnumerable<LabelValue> Columns { get; set; }
        public IEnumerable<IEnumerable<LabelValue>> RowGroups { get; set; }
        public IEnumerable<LabelValue> Rows { get; set; }
    }
}