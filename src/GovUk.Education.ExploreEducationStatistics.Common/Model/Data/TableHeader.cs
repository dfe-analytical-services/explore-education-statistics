#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace GovUk.Education.ExploreEducationStatistics.Common.Model.Data
{
    public class TableHeader
    {
        public string? Level { get; set; }
        public string Value { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public TableHeaderType Type { get; set; }

        private TableHeader()
        {
        }

        public TableHeader(string value, TableHeaderType type)
        {
            Value = value;
            Type = type;
        }

        public static TableHeader NewLocationHeader(GeographicLevel level, string value)
        {
            return new()
            {
                Level = level.ToString().CamelCase(),
                Value = value,
                Type = TableHeaderType.Location
            };
        }

        public TableHeader Clone()
        {
            return (TableHeader)MemberwiseClone();
        }
    }

    public enum TableHeaderType
    {
        Filter,
        Indicator,
        Location,
        TimePeriod
    }
}
