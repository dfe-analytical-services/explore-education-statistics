using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model.Converters
{
    public class ContentBlockChartConverter : JsonConverter
    {
        
        public override bool CanWrite => false;
        public override bool CanRead => true;
        
        public override bool CanConvert(Type objectType)
        {
            return (objectType == typeof(IContentBlockChart));
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var jsonObject = JObject.Load(reader);
            var contentBlock = default(IContentBlockChart);
            
            switch (jsonObject["Type"].Value<string>())
            {
                case "line":
                    contentBlock = new LineChart();
                    break;
            }
            serializer.Populate(jsonObject.CreateReader(), contentBlock);
            return contentBlock;
        }
 
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new InvalidOperationException("Use default serialization.");
            // serializer.Serialize(writer, value, typeof(MarkDownBlock));
        }

        
        
    }
}