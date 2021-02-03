using GovUk.Education.ExploreEducationStatistics.Data.Model;

namespace GovUk.Education.ExploreEducationStatistics.Data.Processor.Models
{
    public class MetaRow
    {
        public string ColumnName { get; set; }
        public ColumnType ColumnType { get; set; }
        public string Label { get; set; }
        public string FilterGroupingColumn { get; set; }
        public string FilterHint { get; set; }
        public string IndicatorGrouping { get; set; }
        public Unit IndicatorUnit { get; set; }
        public int? DecimalPlaces { get; set; }
    }
}