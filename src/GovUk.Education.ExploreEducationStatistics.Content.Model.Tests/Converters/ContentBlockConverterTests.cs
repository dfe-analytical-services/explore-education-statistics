using GovUk.Education.ExploreEducationStatistics.Content.Model.Converters;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Converters
{
    public class ContentBlockConverterTests
    {
        [Fact]
        public void ContentBlockConverterReturnsMarkdownBlock()
        {
            const string testString = "{\"Type\":\"MarkDownBlock\",\"Body\":\"Markdown text\"}";
            
            var serializerSettings = new JsonSerializerSettings();
            serializerSettings.Converters.Add(new ContentBlockConverter());

            var serializer = JsonSerializer.Create(serializerSettings);

            var jObject = JObject.Parse(testString);
            var result = jObject.ToObject<MarkDownBlock>(serializer);
            
            Assert.Equal("Markdown text", result.Body);
        }

        [Fact]
        public void ContentBlockConverterReturnsHtmlBlock()
        {
            const string testString = "{\"Type\":\"HtmlBlock\", \"Body\":\"some html\"}";
            
            var serializerSettings = new JsonSerializerSettings();
            serializerSettings.Converters.Add(new ContentBlockConverter());

            var serializer = JsonSerializer.Create(serializerSettings);

            var jObject = JObject.Parse(testString);
            
            var result = jObject.ToObject<HtmlBlock>(serializer);
            
            Assert.Equal("some html", result.Body);
        }
        
        [Fact]
        public void ContentBlockConverterReturnsDataBlock()
        {
            const string testString = "{\"Type\":\"DataBlock\", \"Heading\":\"heading\"}";
            
            var serializerSettings = new JsonSerializerSettings();
            serializerSettings.Converters.Add(new ContentBlockConverter());

            var serializer = JsonSerializer.Create(serializerSettings);

            var jObject = JObject.Parse(testString);
            
            var result = jObject.ToObject<DataBlock>(serializer);
            
            Assert.Equal("heading", result.Heading);
        }
        
        [Fact]
        public void ContentBlockConverterReturnsBlockWithNullSummaryDescription()
        {
            const string testString = "{\"Type\":\"DataBlock\", \"Heading\":\"heading\", \"Summary\": {description: null}}";
            
            var serializerSettings = new JsonSerializerSettings();
            serializerSettings.Converters.Add(new ContentBlockConverter());

            var serializer = JsonSerializer.Create(serializerSettings);

            var jObject = JObject.Parse(testString);
            
            var result = jObject.ToObject<DataBlock>(serializer);
            
            Assert.Equal("heading", result.Heading);
        }
    }
}