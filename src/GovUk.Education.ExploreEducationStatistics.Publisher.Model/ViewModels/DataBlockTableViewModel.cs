using System.Collections.Generic;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Model.ViewModels
{
    public class DataBlockTableViewModel
    {
        public DataBlockTableHeadersViewModel TableHeaders { get; set; }
    }

    public class DataBlockTableHeadersViewModel
    {
        public List<List<DataBlockTableOptionViewModel>> ColumnGroups { get; set; }
        public List<DataBlockTableOptionViewModel> Columns { get; set; }
        public List<List<DataBlockTableRowGroupOptionViewModel>> RowGroups { get; set; }
        public List<DataBlockTableOptionViewModel> Rows { get; set; }
    }

    public class DataBlockTableRowGroupOptionViewModel
    {
        public string Label { get; set; }
        public string Level { get; set; }
        public string Value { get; set; }
    }

    public class DataBlockTableOptionViewModel
    {
        public string Label { get; set; }
        public string Value { get; set; }
    }
}