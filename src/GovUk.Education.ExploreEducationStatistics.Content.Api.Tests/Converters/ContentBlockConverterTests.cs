using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Converters;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;

namespace GovUk.Education.ExploreStatistics.Content.Api.Tests.Converters
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
            
            Assert.Equal(result.Heading, "Heading text");
            Assert.Equal(result.Body, "heading body text");
        }
    }
}