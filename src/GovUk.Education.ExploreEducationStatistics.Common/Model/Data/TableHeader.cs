using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace GovUk.Education.ExploreEducationStatistics.Common.Model.Data
{
    public class TableHeader
    {
        public string Label { get; set; }
        public string Level { get; set; }
        public string Value { get; set; }
        [JsonConverter(typeof(StringEnumConverter))]
        public TableHeaderType? Type { get; set; }

        private TableHeader()
        {
        }

        public TableHeader(string label, string value, TableHeaderType type)
        {
            Label = label;
            Value = value;
            Type = type;
        }

        public static TableHeader NewLocationHeader(string label, GeographicLevel level, string value)
        {
            return new TableHeader
            {
                Label = label,
                Level = level.ToString().CamelCase(),
                Value = value,
                Type = TableHeaderType.Location
            };
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