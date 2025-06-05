#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Model;

namespace GovUk.Education.ExploreEducationStatistics.Data.Processor.Models;

public class MetaRow
{
    public string ColumnName { get; set; } = string.Empty;
    public ColumnType ColumnType { get; set; }
    public string Label { get; set; } = string.Empty;
    public string? FilterGroupingColumn { get; set; }
    public string? ParentFilter { get; set; }
    public string? FilterHint { get; set; }
    public string? AutoSelectFilterItemLabel { get; set; }
    public string? IndicatorGrouping { get; set; }
    public IndicatorUnit IndicatorUnit { get; set; }
    public int? DecimalPlaces { get; set; }
}
