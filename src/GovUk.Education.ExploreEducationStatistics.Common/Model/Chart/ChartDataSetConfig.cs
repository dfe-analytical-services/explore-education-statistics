#nullable enable
using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace GovUk.Education.ExploreEducationStatistics.Common.Model.Chart
{
    public class ChartDataSetConfig
    {
        public ChartBaseDataSet DataSet;
        public ChartDataGrouping DataGrouping;
        public BoundaryLevel BoundaryLevels;
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum ChartDataGroupingType
    {
        EqualIntervals,
        Quantiles,
        Custom,
    }

    public class ChartDataGrouping
    {
        public ChartDataGroupingType Type;
        public int NumberOfGroups;
        public List<ChartCustomDataGroup> CustomGroups = new();
    }
}
