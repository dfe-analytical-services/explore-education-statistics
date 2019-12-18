using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using GovUk.Education.ExploreEducationStatistics.Common.Converters;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Converters;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

// ReSharper disable NotAccessedField.Global
// ReSharper disable UnusedMember.Global
// ReSharper disable ClassNeverInstantiated.Global

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

        [JsonConverter(typeof(StringEnumConverter))]
        public GeographicLevel? GeographicLevel;

        public TimePeriod TimePeriod;
        public List<string> Filters;
        public List<string> Indicators;

        public List<string> Country;
        public List<string> Institution;
        public List<string> LocalAuthority;
        public List<string> LocalAuthorityDistrict;
        public List<string> LocalEnterprisePartnership;
        public List<string> MultiAcademyTrust;
        public List<string> MayoralCombinedAuthority;
        public List<string> OpportunityArea;
        public List<string> ParliamentaryConstituency;
        public List<string> Region;
        public List<string> RscRegion;
        public List<string> Sponsor;
        public List<string> Ward;
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
        string Title { get; set; }
        int Height { get; set; }
        int Width { get; set; }
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
        public Institution Institution;
        public LocalAuthority LocalAuthority;
        public LocalAuthorityDistrict LocalAuthorityDistrict;
        public LocalEnterprisePartnership LocalEnterprisePartnership;
        public Mat MultiAcademyTrust;
        public MayoralCombinedAuthority MayoralCombinedAuthority;
        public OpportunityArea OpportunityArea;
        public ParliamentaryConstituency ParliamentaryConstituency;
        public Region Region;
        public RscRegion RscRegion;
        public Sponsor Sponsor;
        public Ward Ward;
    }

    // this enum needs these like this as they match what is used in the front end
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public enum AxisGroupBy
    {
        timePeriod,
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

    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public enum LineStyle
    {
        solid,
        dashed,
        dotted
    }

    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public enum Legend
    {
        none,
        bottom,
        top
    }

    // this enum needs these like this as they match what is used in the front end
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public enum LabelPosition
    {
        axis,
        graph,
        top,
        left,
        right,
        bottom,
        inside,
        outside,
        insideLeft,
        insideRight,
        insideTop,
        insideBottom,
        insideTopLeft,
        insideBottomLeft,
        insideTopRight
    }

    // this enum needs these like this as they match what is used in the front end
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public enum AxisType
    {
        major,
        minor
    }

    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public enum TickConfig
    {
        [EnumMember(Value = "default")] Default,
        startEnd,
        custom
    }

    public class AxisConfigurationItem
    {
        public string Name;

        [JsonConverter(typeof(StringEnumConverter))]
        public AxisType Type;

        [JsonConverter(typeof(StringEnumConverter))]
        public AxisGroupBy GroupBy;

        public string SortBy;
        public bool SortAsc = true;

        public List<ChartDataSet> DataSets;
        public List<ReferenceLine> ReferenceLines;
        public bool Visible = true;
        public string Title;
        public string Unit;
        public bool ShowGrid = true;

        [JsonConverter(typeof(StringEnumConverter))]
        public LabelPosition LabelPosition;

        public int? Min;
        public int? Max;
        public string Size;

        [JsonConverter(typeof(StringEnumConverter))]
        public TickConfig TickConfig;
        public string TickSpacing;
    }

    public class ReferenceLine
    {
        public string Label;
        public string Position;
    }

    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class ChartConfiguration
    {
        public string Label;
        public string Value;
        public string Name;
        public string Unit;
        public string Colour;

        [JsonConverter(typeof(StringEnumConverter))]
        public ChartSymbol symbol;

        [JsonConverter(typeof(StringEnumConverter))]
        public LineStyle LineStyle = LineStyle.solid;
    }

    public class LineChart : IContentBlockChart
    {
        public string Type => "line";
        public string Title { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public Legend Legend;

        public int LegendHeight { get; set; }

        public Dictionary<string, ChartConfiguration> Labels;
        public Dictionary<string, AxisConfigurationItem> Axes;
    }

    public class HorizontalBarChart : IContentBlockChart
    {
        public string Type => "horizontalbar";
        public string Title { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public Legend Legend;

        public int LegendHeight { get; set; }

        public Dictionary<string, ChartConfiguration> Labels;
        public Dictionary<string, AxisConfigurationItem> Axes;
        public bool Stacked;
    }

    public class VerticalBarChart : IContentBlockChart
    {
        public string Type => "verticalbar";
        public Dictionary<string, ChartConfiguration> Labels;
        public Dictionary<string, AxisConfigurationItem> Axes;
        public bool Stacked;
        public string Title { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public Legend Legend;

        public int LegendHeight { get; set; }
    }

    public class MapChart : IContentBlockChart
    {
        public string Type => "map";
        public string Title { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public Dictionary<string, ChartConfiguration> Labels;
        public Dictionary<string, AxisConfigurationItem> Axes;
    }

    public class InfographicChart : IContentBlockChart
    {
        public string Type => "infographic";
        public string Title { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }

        public string ReleaseId;
        public string FileId;
    }
}