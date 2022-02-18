#nullable enable
using System.Collections.Generic;
using System.Linq;

namespace GovUk.Education.ExploreEducationStatistics.Common.Model.Data
{
    public class TableHeaders
    {
        public List<List<TableHeader>> ColumnGroups { get; set; } = new();
        public List<TableHeader> Columns { get; set; } = new();
        public List<List<TableHeader>> RowGroups { get; set; } = new();
        public List<TableHeader> Rows { get; set; } = new();

        public TableHeaders Clone()
        {
            return new TableHeaders
            {
                ColumnGroups = ColumnGroups.Select(CloneGroup).ToList(),
                Columns = Columns.Select(CloneTableHeader).ToList(),
                RowGroups = RowGroups.Select(CloneGroup).ToList(),
                Rows = Rows.Select(CloneTableHeader).ToList()
            };
        }

        private static List<TableHeader> CloneGroup(List<TableHeader> group)
        {
            return group.Select(CloneTableHeader).ToList();
        }

        private static TableHeader CloneTableHeader(TableHeader tableHeader)
        {
            return tableHeader.Clone();
        }
    }
}
