#nullable enable
using System;
using GovUk.Education.ExploreEducationStatistics.Common.Converters.SystemJson;
using GovUk.Education.ExploreEducationStatistics.Common.Database;
using System.Text.Json;
using System.Text.Json.Serialization;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Common.Tests.Converters.SystemJson;

public class EnumToEnumValueJsonConverterTests
{
    private enum SampleEnum
    {
        [EnumLabelValue("SampleLabel", "SampleValue")]
        Sample
    }

    private record SampleClass
    {
        [JsonConverter(typeof(EnumToEnumValueJsonConverter<SampleEnum>))]
        public SampleEnum SampleField { get; init; }
    }

    [Fact]
    public void SerializeObject()
    {
        var objectToSerialize = new SampleClass
        {
            SampleField = SampleEnum.Sample
        };

        Assert.Equal("{\"SampleField\":\"SampleValue\"}", JsonSerializer.Serialize(objectToSerialize));
    }

    [Fact]
    public void DeserializeObject()
    {
        const string jsonText = "{\"SampleField\":\"SampleValue\"}";

        var expected = new SampleClass
        {
            SampleField = SampleEnum.Sample
        };

        Assert.Equal(expected, JsonSerializer.Deserialize<SampleClass>(jsonText));
    }

    [Fact]
    public void DeserializeObject_Null_Throws()
    {
        const string jsonText = "{\"SampleField\":null}";

        Assert.Throws<NullReferenceException>(() => JsonSerializer.Deserialize<SampleClass>(jsonText));
    }

    [Fact]
    public void DeserializeObject_InvalidLabel_Throws()
    {
        const string jsonText = "{\"SampleField\":\"Invalid label\"}";

        Assert.Throws<ArgumentOutOfRangeException>(() => JsonSerializer.Deserialize<SampleClass>(jsonText));
    }
}
