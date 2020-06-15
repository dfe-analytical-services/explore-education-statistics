using System;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Chart;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace GovUk.Education.ExploreEducationStatistics.Common.Converters
{
    public class ContentBlockChartConverter : JsonConverter<IContentBlockChart>
    {
        public override bool CanWrite => false;

        public override IContentBlockChart ReadJson(JsonReader reader, Type objectType,
            IContentBlockChart existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var jsonObject = JObject.Load(reader);
            var type = jsonObject["Type"] ?? jsonObject["type"];
            var chartType = EnumUtil.GetFromString<ChartType>(type.Value<string>());

            IContentBlockChart contentBlock = chartType switch
            {
                ChartType.Line => new LineChart(),
                ChartType.HorizontalBar => new HorizontalBarChart(),
                ChartType.VerticalBar => new VerticalBarChart(),
                ChartType.Map => new MapChart(),
                ChartType.Infographic => new InfographicChart(),
                _ => throw new ArgumentOutOfRangeException()
            };

            serializer.Populate(jsonObject.CreateReader(), contentBlock);
            return contentBlock;
        }

        public override void WriteJson(JsonWriter writer, IContentBlockChart value, JsonSerializer serializer)
        {
            throw new InvalidOperationException("Use default serialization.");
        }
    }
}