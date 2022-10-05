#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace GovUk.Education.ExploreEducationStatistics.Common.Model.Chart
{
    public class ChartLegend
    {
        public ChartLegendPosition? Position;
        public List<ChartLegendItem> Items = new List<ChartLegendItem>();
    }

    [SuppressMessage("ReSharper", "InconsistentNaming")]
    [JsonConverter(typeof(StringEnumConverter))]
    public enum ChartLegendPosition
    {
        none,
        bottom,
        top,
        inline
    }

    public class ChartLegendItem
    {
        public ChartLegendItemDataSet DataSet;
        public string Label;
        public string Colour;

        [JsonConverter(typeof(StringEnumConverter))]
        public ChartLineSymbol? Symbol;

        [JsonConverter(typeof(StringEnumConverter))]
        public ChartLineStyle? LineStyle;

        [JsonConverter(typeof(StringEnumConverter))]
        public ChartInlinePosition? InlinePosition;
    }

    public class ChartLegendItemDataSet
    {
        public Guid? Indicator;
        public List<Guid> Filters = new List<Guid>();
        public ChartDataSetLocation? Location;
        public string? TimePeriod;
    }
}