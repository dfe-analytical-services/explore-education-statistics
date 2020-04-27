using System.Collections.Generic;

namespace GovUk.Education.ExploreEducationStatistics.Common.Model.Data
{
    // TODO EES-17 rename to TableHeaders?
    public class TableBuilderTableHeadersConfig
    {
        // TODO EES-17 changed to TableHeader 
        public IEnumerable<IEnumerable<TableHeader>> ColumnGroups { get; set; }
        // TODO EES-17 changed to TableHeader
        public IEnumerable<TableHeader> Columns { get; set; }
        // TODO EES-17 EES-228 changed to TableHeader (uses level)
        public IEnumerable<IEnumerable<TableHeader>> RowGroups { get; set; }
        // TODO EES-17 changed to TableHeader
        public IEnumerable<TableHeader> Rows { get; set; }
    }
}