#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace GovUk.Education.ExploreEducationStatistics.Common.Model.Chart
{
    public class ChartDataSetConfig
    {
        public ChartDataSetConfigDataSet DataSet;
        public ChartDataGrouping DataGrouping;
        public BoundaryLevel BoundaryLevels;
    }

    [SuppressMessage("ReSharper", "InconsistentNaming")]
    [JsonConverter(typeof(StringEnumConverter))]
    public enum ChartDataGroupingType
    {
        EqualIntervals,
        Quantiles,
        Custom,
    }

    public class ChartDataSetConfigDataSet
    {
        public Guid Indicator;
        public List<Guid> Filters = new List<Guid>();
        public ChartDataSetLocation Location;
        public string TimePeriod;
    }

    public class ChartDataGrouping
    {
        public ChartDataGroupingType Type;
        public int NumberOfGroups;
        public List<ChartCustomDataGroup> CustomGroups;
    }
}
