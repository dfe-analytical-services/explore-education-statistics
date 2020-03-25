using System;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Chart;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace GovUk.Education.ExploreEducationStatistics.Common.Converters
{
    public class ContentBlockChartConverter : JsonConverter
    {
        public override bool CanWrite => false;
        public override bool CanRead => true;

        public override bool CanConvert(Type objectType)
        {
            return (objectType == typeof(IContentBlockChart));
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue,
            JsonSerializer serializer)
        {
            try
            {
                var jsonObject = JObject.Load(reader);
                var contentBlock = default(IContentBlockChart);

                var type = jsonObject["Type"] ?? jsonObject["type"];

                switch (type.Value<string>())
                {
                    case "line":
                        contentBlock = new LineChart();
                        break;
                    case "horizontalbar":
                        contentBlock = new HorizontalBarChart();
                        break;
                    case "verticalbar":
                        contentBlock = new VerticalBarChart();
                        break;
                    case "map":
                        contentBlock = new MapChart();
                        break;
                    case "infographic":
                        contentBlock = new InfographicChart();
                        break;
                }

                if (contentBlock != null)
                {
                    serializer.Populate(jsonObject.CreateReader(), contentBlock);
                }

                return contentBlock;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new InvalidOperationException("Use default serialization.");
        }
    }
}