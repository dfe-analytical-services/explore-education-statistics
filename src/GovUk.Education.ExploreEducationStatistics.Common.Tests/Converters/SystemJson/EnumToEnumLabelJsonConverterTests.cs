using GovUk.Education.ExploreEducationStatistics.Common.Converters.SystemJson;
using GovUk.Education.ExploreEducationStatistics.Common.Database;
using System.Text.Json;
using System.Text.Json.Serialization;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Common.Tests.Converters.SystemJson;

public class EnumToEnumLabelJsonConverterTests
{
    private enum SampleEnum
    {
        [EnumLabelValue("SampleLabel", "SampleValue")]
        Sample
    }

    private class SampleClass
    {
        [JsonConverter(typeof(EnumToEnumLabelJsonConverter<SampleEnum>))]
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
            return Equals((SampleClass)obj);
        }

        public override int GetHashCode()
        {
            return (int)SampleField;
        }
    }

    [Fact]
    public void SerializeObject()
    {
        var objectToSerialize = new SampleClass
        {
            SampleField = SampleEnum.Sample
        };

        Assert.Equal("{\"SampleField\":\"SampleLabel\"}", JsonSerializer.Serialize(objectToSerialize));
    }

    [Fact]
    public void DeserializeObject()
    {
        const string jsonText = "{\"SampleField\":\"SampleLabel\"}";

        var expected = new SampleClass
        {
            SampleField = SampleEnum.Sample
        };

        Assert.Equal(expected, JsonSerializer.Deserialize<SampleClass>(jsonText));
    }
}
