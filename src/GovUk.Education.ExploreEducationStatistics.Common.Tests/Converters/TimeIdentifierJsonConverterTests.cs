using GovUk.Education.ExploreEducationStatistics.Common.Converters;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Newtonsoft.Json;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Common.Tests.Converters
{
    public class TimeIdentifierJsonConverterTests
    {
        private class SampleClass
        {
            public string StringFieldBefore { get; set; }

            [JsonConverter(typeof(TimeIdentifierJsonConverter))]
            public TimeIdentifier? SampleField { get; set; }
            
            public string StringFieldAfter { get; set; }

            protected bool Equals(SampleClass other)
            {
                return string.Equals(StringFieldBefore, other.StringFieldBefore) && SampleField == other.SampleField && string.Equals(StringFieldAfter, other.StringFieldAfter);
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != this.GetType()) return false;
                return Equals((SampleClass) obj);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    var hashCode = (StringFieldBefore != null ? StringFieldBefore.GetHashCode() : 0);
                    hashCode = (hashCode * 397) ^ SampleField.GetHashCode();
                    hashCode = (hashCode * 397) ^ (StringFieldAfter != null ? StringFieldAfter.GetHashCode() : 0);
                    return hashCode;
                }
            }
        }

        [Fact]
        public void SerializeObject()
        {
            var objectToSerialize = new SampleClass
            {
                StringFieldBefore = "Hello",
                SampleField = TimeIdentifier.April,
                StringFieldAfter = "Goodbye",
            };
            
            Assert.Equal("{\"StringFieldBefore\":\"Hello\",\"SampleField\":{\"value\":\"M4\",\"label\":\"April\"},\"StringFieldAfter\":\"Goodbye\"}", JsonConvert.SerializeObject(objectToSerialize));
        }
        
        
        [Fact]
        public void SerializeObjectNull()
        {
            var objectToSerialize = new SampleClass
            {
                StringFieldBefore = "Hello",
                SampleField = null,
                StringFieldAfter = "Goodbye",
            };
            
            Assert.Equal("{\"StringFieldBefore\":\"Hello\",\"SampleField\":null,\"StringFieldAfter\":\"Goodbye\"}", JsonConvert.SerializeObject(objectToSerialize));
        }
        
        [Fact]
        public void DeserializeObject()
        {
            const string jsonText =
                "{\"StringFieldBefore\":\"Hello\",\"SampleField\":{\"value\":\"M4\",\"label\":\"April\"},\"StringFieldAfter\":\"Goodbye\"}";

            var expected = new SampleClass
            {
                StringFieldBefore = "Hello",
                SampleField = TimeIdentifier.April,
                StringFieldAfter = "Goodbye",
            };

            Assert.Equal(expected, JsonConvert.DeserializeObject<SampleClass>(jsonText));
        }
        
        [Fact]
        public void DeserializeObjectNull()
        {
            const string jsonText =
                "{\"StringFieldBefore\":\"Hello\",\"SampleField\":null,\"StringFieldAfter\":\"Goodbye\"}";

            var expected = new SampleClass
            {
                StringFieldBefore = "Hello",
                SampleField = null,
                StringFieldAfter = "Goodbye",
            };

            Assert.Equal(expected, JsonConvert.DeserializeObject<SampleClass>(jsonText));
        }
        
        [Fact]
        public void DeserializeObjectOutOfOrder()
        {
            const string jsonText =
                "{\"StringFieldAfter\":\"Goodbye\",\"SampleField\":{\"value\":\"M4\",\"label\":\"April\"},\"StringFieldBefore\":\"Hello\"}";

            var expected = new SampleClass
            {
                StringFieldBefore = "Hello",
                SampleField = TimeIdentifier.April,
                StringFieldAfter = "Goodbye",
            };

            Assert.Equal(expected, JsonConvert.DeserializeObject<SampleClass>(jsonText));
        }
    }
}