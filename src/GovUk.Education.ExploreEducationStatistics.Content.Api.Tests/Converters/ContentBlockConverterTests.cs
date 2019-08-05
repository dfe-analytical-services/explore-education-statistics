using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Converters;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Tests.Converters
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
            
            Assert.Equal(result.Body, "Markdown text");
        }
        
        [Fact]
        public void ContentBlockConverterReturnsInsetTextBlock()
        {
            const string testString = "{\"Type\":\"InsetTextBlock\", \"Heading\": \"Heading text\",\"Body\":\"heading body text\"}";
            
            var serializerSettings = new JsonSerializerSettings();
            serializerSettings.Converters.Add(new ContentBlockConverter());

            var serializer = JsonSerializer.Create(serializerSettings);

            var jObject = JObject.Parse(testString);
            
            var result = jObject.ToObject<InsetTextBlock>(serializer);
            
            Assert.Equal("Heading text", result.Heading);
            Assert.Equal("heading body text", result.Body);
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
    }
}