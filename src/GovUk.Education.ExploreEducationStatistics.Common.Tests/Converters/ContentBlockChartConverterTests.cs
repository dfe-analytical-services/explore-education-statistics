using GovUk.Education.ExploreEducationStatistics.Common.Converters;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Chart;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Common.Tests.Converters
{
    public class ContentBlockChartConverterTests
    {
        [Fact]
        public void ContentBlockChartConverterReturnsLine()
        {
            const string testString = "{\"Type\":\"line\"}";

            var serializerSettings = new JsonSerializerSettings();
            serializerSettings.Converters.Add(new ContentBlockChartConverter());

            var serializer = JsonSerializer.Create(serializerSettings);

            var jObject = JObject.Parse(testString);
            var result = jObject.ToObject<LineChart>(serializer);

            Assert.IsType<LineChart>(result);
        }

        [Fact]
        public void ContentBlockChartConverterReturnsHorizontalBar()
        {
            const string testString = "{\"Type\":\"horizontalbar\"}";

            var serializerSettings = new JsonSerializerSettings();
            serializerSettings.Converters.Add(new ContentBlockChartConverter());

            var serializer = JsonSerializer.Create(serializerSettings);

            var jObject = JObject.Parse(testString);
            var result = jObject.ToObject<HorizontalBarChart>(serializer);

            Assert.IsType<HorizontalBarChart>(result);
        }

        [Fact]
        public void ContentBlockChartConverterReturnsVerticalBar()
        {
            const string testString = "{\"Type\":\"verticalbar\"}";

            var serializerSettings = new JsonSerializerSettings();
            serializerSettings.Converters.Add(new ContentBlockChartConverter());

            var serializer = JsonSerializer.Create(serializerSettings);

            var jObject = JObject.Parse(testString);
            var result = jObject.ToObject<VerticalBarChart>(serializer);

            Assert.IsType<VerticalBarChart>(result);
        }

        [Fact]
        public void ContentBlockChartConverterReturnsMap()
        {
            const string testString = "{\"Type\":\"map\"}";

            var serializerSettings = new JsonSerializerSettings();
            serializerSettings.Converters.Add(new ContentBlockChartConverter());

            var serializer = JsonSerializer.Create(serializerSettings);

            var jObject = JObject.Parse(testString);
            var result = jObject.ToObject<MapChart>(serializer);

            Assert.IsType<MapChart>(result);
        }
    }
}