#nullable enable
using System.Text.Json;
using System.Text.Json.Serialization;
using GovUk.Education.ExploreEducationStatistics.Common.Converters.SystemJson;
using GovUk.Education.ExploreEducationStatistics.Common.Database;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Common.Tests.Converters.SystemJson;

public class EnumToEnumLabelJsonConverterTests
{
    private enum SampleEnum
    {
        [EnumLabelValue("SampleLabel", "SampleValue")]
        Sample,
    }

    private record SampleClass
    {
        [JsonConverter(typeof(EnumToEnumLabelJsonConverter<SampleEnum>))]
        public SampleEnum SampleField { get; init; }
    }

    [Fact]
    public void SerializeObject()
    {
        var objectToSerialize = new SampleClass { SampleField = SampleEnum.Sample };

        Assert.Equal(
            "{\"SampleField\":\"SampleLabel\"}",
            JsonSerializer.Serialize(objectToSerialize)
        );
    }

    [Fact]
    public void DeserializeObject()
    {
        const string jsonText = "{\"SampleField\":\"SampleLabel\"}";

        var expected = new SampleClass { SampleField = SampleEnum.Sample };

        Assert.Equal(expected, JsonSerializer.Deserialize<SampleClass>(jsonText));
    }

    [Fact]
    public void DeserializeObject_Null_Throws()
    {
        const string jsonText = "{\"SampleField\":null}";

        Assert.Throws<NullReferenceException>(() =>
            JsonSerializer.Deserialize<SampleClass>(jsonText)
        );
    }

    [Fact]
    public void DeserializeObject_InvalidLabel_Throws()
    {
        const string jsonText = "{\"SampleField\":\"Invalid label\"}";

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            JsonSerializer.Deserialize<SampleClass>(jsonText)
        );
    }
}
