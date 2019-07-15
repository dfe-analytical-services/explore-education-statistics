using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using GovUk.Education.ExploreEducationStatistics.Common.Converters;
using GovUk.Education.ExploreEducationStatistics.Model.Service;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Converters;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Converters;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model
{
    public class DataQuery
    {
        public string path;
        public string method;
        public string body;
    }

    public class DataBlockRequest
    {
        public int SubjectId;
        public string GeographicLevel;
        public TimePeriod TimePeriod;
        public List<string> Filters;
        public List<string> Indicators;
        
        public List<string> Country;
        public List<string> LocalAuthority;
        public List<string> Region;
    }

    public class TimePeriod
    {
        public string StartYear;
        [JsonConverter(typeof(EnumToEnumValueJsonConverter<TimeIdentifier>))]
        public TimeIdentifier StartCode;
        public string EndYear;
        [JsonConverter(typeof(EnumToEnumValueJsonConverter<TimeIdentifier>))]
        public TimeIdentifier EndCode;
    }


    [JsonConverter(typeof(ContentBlockChartConverter))]
    public interface IContentBlockChart
    {
        string Type { get; }
    }

    public class ChartDataSet
    {
        public string Indicator;
        public List<string> Filters;
        public List<ChartDataLocation> Location;
        public string TimePeriod;
    }

    public class ChartDataLocation
    {
        public Country Country;
        public Region Region;
        public LocalAuthority LocalAuthority;
        public LocalAuthorityDistrict LocalAuthorityDistrict;
    }

    // this enum needs these like this as they match what is used in the front end
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public enum AxisGroupBy
    {
        timePeriods,
        locations,
        filters,
        indicators
    }

    // this enum needs these like this as they match what is used in the front end
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public enum ChartSymbol
    {
        circle,
        cross,
        diamond,
        square,
        star,
        triangle,
        wye
    }

    public class AxisConfigurationItem
    {
        public string Name;

        [JsonConverter(typeof(StringEnumConverter))]
        public AxisGroupBy GroupBy;

        public List<ChartDataSet> DataSets;
        public bool Visible;
        public string Title;
    }

    public class ChartConfiguration
    {
        public string Label;
        public string Value;
        public string Name;
        public string Unit;
        public string Colour;
        
        [JsonConverter(typeof(StringEnumConverter))]
        public ChartSymbol symbol;
    }

    public class LineChart : IContentBlockChart
    {
        public string Type => "line";
        public Dictionary<string, ChartConfiguration> Labels;
        public Dictionary<string, AxisConfigurationItem> Axes;
    }

    public class HorizontalBarChart : IContentBlockChart
    {
        public string Type => "horizontalbar";
        public Dictionary<string, ChartConfiguration> Labels;
        public Dictionary<string, AxisConfigurationItem> Axes;
    }

    public class VerticalBarChart : IContentBlockChart
    {
        public string Type => "verticalbar";
        public Dictionary<string, ChartConfiguration> Labels;
        public Dictionary<string, AxisConfigurationItem> Axes;
    }

    public class MapChart : IContentBlockChart
    {
        public string Type => "map";
        public Dictionary<string, ChartConfiguration> Labels;
        public Dictionary<string, AxisConfigurationItem> Axes;
    }
}