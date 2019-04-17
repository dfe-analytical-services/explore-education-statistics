using GovUk.Education.ExploreEducationStatistics.Data.Model.Meta;

namespace GovUk.Education.ExploreEducationStatistics.Data.Importer.Models
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
    }
}