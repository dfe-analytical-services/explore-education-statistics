using GovUk.Education.ExploreEducationStatistics.Common.Converters;
using GovUk.Education.ExploreEducationStatistics.Common.Database;
using Newtonsoft.Json;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Common.Tests.Converters
{
    public class EnumToEnumValueJsonConverterTests
    {
        private enum SampleEnum
        {
            [EnumLabelValue("SampleLabel", "SampleValue")]
            Sample
        }

        private class SampleClass
        {
            [JsonConverter(typeof(EnumToEnumValueJsonConverter<SampleEnum>))]
            public SampleEnum SampleField { get; set; }

            protected bool Equals(SampleClass other)
            {
                return SampleField == other.SampleField;
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
                return (int) SampleField;
            }
        }

        [Fact]
        public void SerializeObject()
        {
            var objectToSerialize = new SampleClass
            {
                SampleField = SampleEnum.Sample
            };

            Assert.Equal("{\"SampleField\":\"SampleValue\"}", JsonConvert.SerializeObject(objectToSerialize));
        }

        [Fact]
        public void DeserializeObject()
        {
            const string jsonText = "{\"SampleField\":\"SampleValue\"}";

            var expected = new SampleClass
            {
                SampleField = SampleEnum.Sample
            };

            Assert.Equal(expected, JsonConvert.DeserializeObject<SampleClass>(jsonText));
        }
    }
}