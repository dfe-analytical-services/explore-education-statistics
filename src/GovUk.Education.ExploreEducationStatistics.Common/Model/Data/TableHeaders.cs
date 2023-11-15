#nullable enable
using System.Collections.Generic;

namespace GovUk.Education.ExploreEducationStatistics.Common.Model.Data;

public class TableHeaders
{
    public List<List<TableHeader>> ColumnGroups { get; set; } = new();
    public List<TableHeader> Columns { get; set; } = new();
    public List<List<TableHeader>> RowGroups { get; set; } = new();
    public List<TableHeader> Rows { get; set; } = new();
}