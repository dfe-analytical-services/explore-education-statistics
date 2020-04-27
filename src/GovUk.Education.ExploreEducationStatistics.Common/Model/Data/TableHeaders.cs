using System.Collections.Generic;

namespace GovUk.Education.ExploreEducationStatistics.Common.Model.Data
{
    public class TableHeaders
    {
        public IEnumerable<IEnumerable<TableHeader>> ColumnGroups { get; set; }
        public IEnumerable<TableHeader> Columns { get; set; }
        public IEnumerable<IEnumerable<TableHeader>> RowGroups { get; set; }
        public IEnumerable<TableHeader> Rows { get; set; }
    }
}